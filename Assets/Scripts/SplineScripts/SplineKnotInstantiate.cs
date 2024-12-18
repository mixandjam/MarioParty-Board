using UnityEngine;
using UnityEngine.Splines;
using UnityEditor;
using System.Collections.Generic;

[ExecuteAlways]
public class SplineKnotInstantiate : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private GameObject prefabToInstantiate;

    [Header("Parameters")]
    public Vector3 positionOffset;

    [Header("Data")]
    public List<SplineData> splineDatas = new List<SplineData>();

    private void OnEnable()
    {
        if (splineContainer == null)
            splineContainer = GetComponent<SplineContainer>();

        Spline.Changed += OnSplineChanged;
        UpdateSplineData();
    }

    private void OnDisable()
    {
        Spline.Changed -= OnSplineChanged;
    }

    private void OnSplineChanged(Spline spline, int knotIndex, SplineModification modification)
    {
        UpdateSplineData();
        UpdateKnotPositions();
    }


    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (splineContainer == null)
                splineContainer = GetComponent<SplineContainer>();

            UpdateSplineData();
            UpdateKnotPositions();
        }
    }

    private void UpdateSplineData()
    {
        if (splineContainer == null || prefabToInstantiate == null)
            return;

        // Create a dictionary of existing knot data to preserve references
        Dictionary<(int, int), SplineKnotData> existingKnotData = new Dictionary<(int, int), SplineKnotData>();
        foreach (var splineData in splineDatas)
        {
            for (int i = 0; i < splineData.knots.Count; i++)
            {
                if (splineData.knots[i] != null && splineData.knots[i].gameObject != null)
                {
                    existingKnotData.Add((splineDatas.IndexOf(splineData), i), splineData.knots[i]);
                }
            }
        }

        // Clear and rebuild spline data
        splineDatas.Clear();

        for (int splineIndex = 0; splineIndex < splineContainer.Splines.Count; splineIndex++)
        {
            var spline = splineContainer.Splines[splineIndex];
            var splineData = new SplineData { knots = new List<SplineKnotData>() };

            for (int knotIndex = 0; knotIndex < spline.Count; knotIndex++)
            {
                // First check if this knot is linked to another knot
                var splineKnotIndex = new SplineKnotIndex(splineIndex, knotIndex);
                if (splineContainer.KnotLinkCollection.TryGetKnotLinks(splineKnotIndex, out var connectedKnots))
                {
                    // Look for the original knot (one with lowest indices)
                    var originalKnot = (splineIndex, knotIndex);
                    foreach (var linkedKnot in connectedKnots)
                    {
                        if (linkedKnot.Spline < originalKnot.Item1 ||
                            (linkedKnot.Spline == originalKnot.Item1 && linkedKnot.Knot < originalKnot.Item2))
                        {
                            originalKnot = (linkedKnot.Spline, linkedKnot.Knot);
                        }
                    }

                    // If we found an original knot with existing data, use that
                    if (existingKnotData.TryGetValue(originalKnot, out var originalKnotData))
                    {
                        splineData.knots.Add(originalKnotData);
                        continue;
                    }
                }

                // If we get here, either:
                // 1. This is not a linked knot, or
                // 2. This is the original knot in a linked set
                // In either case, check for existing data first
                if (existingKnotData.TryGetValue((splineIndex, knotIndex), out var existingKnot))
                {
                    splineData.knots.Add(existingKnot);
                }
                else
                {
                    InstantiateNewKnot(spline[knotIndex], splineIndex, knotIndex, splineData);
                }
            }

            splineDatas.Add(splineData);
        }

        // Schedule cleanup for next frame to avoid OnValidate errors
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorApplication.delayCall += () =>
            {
                if (this == null) return; // Check if object still exists
                CleanupUnusedKnots();
            };
        }
        else
#endif
        {
            CleanupUnusedKnots();
        }
    }

    private void CleanupUnusedKnots()
    {
        var knotsToDelete = new List<SplineKnotData>();

        // Find all knots that need to be deleted
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent<SplineKnotData>(out SplineKnotData leftOverData))
            {
                bool isUsed = false;
                foreach (SplineData splineData in splineDatas)
                {
                    if (splineData.knots.Contains(leftOverData))
                    {
                        isUsed = true;
                        break;
                    }
                }

                if (!isUsed)
                {
                    knotsToDelete.Add(leftOverData);
                }
            }
        }

        // Delete the unused knots
        foreach (var knotToDelete in knotsToDelete)
        {
            if (knotToDelete != null && knotToDelete.gameObject != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(knotToDelete.gameObject);
                else
#endif
                    Destroy(knotToDelete.gameObject);
            }
        }
    }


    private void InstantiateNewKnot(BezierKnot knot, int splineIndex, int knotIndex, SplineData splineData)
    {
        GameObject instantiatedObject;
#if UNITY_EDITOR
        if (PrefabUtility.IsPartOfPrefabAsset(prefabToInstantiate))
        {
            instantiatedObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabToInstantiate, transform);
        }
        else
        {
            instantiatedObject = Instantiate(prefabToInstantiate, transform);
        }
#else
        instantiatedObject = Instantiate(prefabToInstantiate, transform);
#endif

        instantiatedObject.name = $"S{splineIndex}K{knotIndex}";
        instantiatedObject.transform.position = (Vector3)knot.Position + splineContainer.transform.position;
        instantiatedObject.transform.rotation = knot.Rotation;

        if (instantiatedObject.TryGetComponent<SplineKnotData>(out SplineKnotData data))
        {
            data.knotIndex = new SplineKnotIndex(splineIndex, knotIndex);
            splineData.knots.Add(data);
        }
        else
        {
            Debug.LogError("The instantiated prefab does not have a SplineKnotData component!");
            return;
        }
    }

    private void UpdateKnotPositions()
    {
        try
        {
            for (int i = 0; i < splineDatas.Count; i++)
            {
                // Check if spline index is valid
                if (i >= splineContainer.Splines.Count)
                {
                    UpdateSplineData(); // Refresh data if splines were removed
                    return;
                }

                var spline = splineContainer.Splines[i];

                for (int j = 0; j < splineDatas[i].knots.Count; j++)
                {
                    // Check if knot index is valid
                    if (j >= spline.Count)
                    {
                        UpdateSplineData(); // Refresh data if knots were removed
                        return;
                    }

                    var knot = spline[j];
                    var knotData = splineDatas[i].knots[j];

                    if (knotData != null && knotData.gameObject != null)
                    {
                        knotData.gameObject.transform.position = (Vector3)knot.Position + splineContainer.transform.position + positionOffset;
                        knotData.gameObject.transform.rotation = knot.Rotation;
                    }
                }
            }
        }
        catch (System.ArgumentOutOfRangeException)
        {
            // If we hit any array bounds issues, refresh the data
            UpdateSplineData();
        }
    }

    private void OnDestroy()
    {
        ResetDataAndObjects();
    }

    [ContextMenu("Delete Data")]
    public void ResetDataAndObjects()
    {
        foreach (SplineData splineData in splineDatas)
        {
            foreach (SplineKnotData knotData in splineData.knots)
            {
                if (knotData != null && knotData.gameObject != null)
                {
                    DestroyImmediate(knotData.gameObject);
                }
            }
        }
        splineDatas.Clear();
    }

    [ContextMenu("Clear and Populate")]
    public void ResetAndPopulate()
    {
        ResetDataAndObjects();
        UpdateSplineData();
        UpdateKnotPositions();
    }

    [System.Serializable]
    public class SplineData
    {
        public List<SplineKnotData> knots;
    }
}
