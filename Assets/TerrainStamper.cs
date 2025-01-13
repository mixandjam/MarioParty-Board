using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode]
public class TerrainStamper : MonoBehaviour
{
    [SerializeField] private Terrain terrain;
    [SerializeField] private SplineContainer splineContainer;
    private TerrainData terrainData;

    [Header("Path Settings")]
    [Range(1f, 40f)] public float pathWidth = 15f;

    [Header("Paint Settings")]
    [Range(0, 10)] public int textureLayer = 0;
    [Range(0, 1)] public float textureOpacity = 0.8f;
    [Range(0f, 10f)] public float paintFalloff = 7f;

    [Header("Height Settings")]
    [Range(0f, 10f)] public float heightFalloff = 7f;
    [Range(-10f, 10f)] public float baseHeight = 0f;

    [Header("Advanced")]
    [Range(0f, 1f)] public float spacing = .1f;

    [Header("Debug")]
    public bool showSpacingGizmo = false;

    [ContextMenu("Stamp Terrain")]
    public void StampTerrain()
    {
        InitializeTerrainData();

        if (terrain == null || terrainData == null)
        {
            Debug.LogError("Terrain or TerrainData is not assigned!");
            return;
        }

        Vector3[] points = GetSplinePoints();

        if (points == null || points.Length == 0)
        {
            Debug.LogError("No points found on spline!");
            return;
        }

        ModifyTerrainHeight(points);
        ModifyTerrainTextures(points);

        terrainData.SyncHeightmap();
        Debug.Log("Stamping completed.");
    }

    [ContextMenu("Reset and Stamp Terrain")]
    public void ResetAndStampTerrain()
    {
        InitializeTerrainData();

        if (terrain == null || terrainData == null)
        {
            Debug.LogError("Terrain or TerrainData is not assigned!");
            return;
        }

        ResetTerrain();
        StampTerrain();
    }

    private void ResetTerrain()
    {
        // Reset heights
        int heightmapResolution = terrainData.heightmapResolution;
        float[,] resetHeights = new float[heightmapResolution, heightmapResolution];
        terrainData.SetHeights(0, 0, resetHeights);

        // Reset splatmap
        int splatmapResolution = terrainData.alphamapResolution;
        float[,,] resetSplatmap = new float[splatmapResolution, splatmapResolution, terrainData.alphamapLayers];
        for (int y = 0; y < splatmapResolution; y++)
        {
            for (int x = 0; x < splatmapResolution; x++)
            {
                resetSplatmap[y, x, 0] = 1f;
                for (int layer = 1; layer < terrainData.alphamapLayers; layer++)
                {
                    resetSplatmap[y, x, layer] = 0f;
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, resetSplatmap);

        Debug.Log("Terrain reset to default state.");
    }


    private void InitializeTerrainData()
    {
        terrainData = terrain?.terrainData;
    }

    private void ModifyTerrainHeight(Vector3[] points)
    {
        int resolution = terrainData.heightmapResolution;
        float[,] heights = terrainData.GetHeights(0, 0, resolution, resolution);

        foreach (var point in points)
        {
            Vector3 terrainLocalPos = WorldToTerrainPosition(point);
            ApplyHeightChange(terrainLocalPos, heights, resolution);
        }

        terrainData.SetHeights(0, 0, heights);
    }

    private void ModifyTerrainTextures(Vector3[] points)
    {
        int alphamapWidth = terrainData.alphamapWidth;
        int alphamapHeight = terrainData.alphamapHeight;
        float[,,] alphaMaps = terrainData.GetAlphamaps(0, 0, alphamapWidth, alphamapHeight);

        foreach (var point in points)
        {
            Vector3 terrainLocalPos = WorldToTerrainPosition(point);
            ApplyTextureChange(terrainLocalPos, alphaMaps, alphamapWidth, alphamapHeight);
        }

        terrainData.SetAlphamaps(0, 0, alphaMaps);
    }

    private void ApplyHeightChange(Vector3 terrainPos, float[,] heights, int resolution)
    {
        int centerX = Mathf.FloorToInt(terrainPos.x * resolution);
        int centerY = Mathf.FloorToInt(terrainPos.z * resolution);

        int radius = Mathf.FloorToInt((pathWidth + heightFalloff) / terrainData.size.x * resolution);
        float pathRadius = pathWidth / terrainData.size.x * resolution; // Convert pathWidth to heightmap scale
        float falloffRadius = heightFalloff / terrainData.size.x * resolution;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                int posX = Mathf.Clamp(centerX + x, 0, resolution - 1);
                int posY = Mathf.Clamp(centerY + y, 0, resolution - 1);

                float distance = Vector2.Distance(new Vector2(centerX, centerY), new Vector2(posX, posY));

                float falloff = 0f;
                if (distance <= pathRadius)
                {
                    // Inside the pathWidth, fully apply the spline height
                    falloff = 1f;
                }
                else if (distance <= pathRadius + falloffRadius)
                {
                    // Within the falloff region, calculate a smooth transition
                    float normalizedDistance = (distance - pathRadius) / falloffRadius;
                    falloff = Mathf.Clamp01(1f - normalizedDistance);
                }

                float targetHeight = (terrainPos.y + baseHeight) / terrainData.size.y;
                heights[posY, posX] = Mathf.Lerp(heights[posY, posX], targetHeight, falloff);
            }
        }
    }



    private void ApplyTextureChange(Vector3 terrainPos, float[,,] alphaMaps, int width, int height)
    {
        int centerX = Mathf.FloorToInt(terrainPos.x * width);
        int centerY = Mathf.FloorToInt(terrainPos.z * height);

        int radius = Mathf.FloorToInt(paintFalloff / terrainData.size.x * width);
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                int posX = Mathf.Clamp(centerX + x, 0, width - 1);
                int posY = Mathf.Clamp(centerY + y, 0, height - 1);

                float distance = Vector2.Distance(new Vector2(centerX, centerY), new Vector2(posX, posY));
                float falloff = Mathf.Clamp01(1 - (distance / radius));

                for (int layer = 0; layer < terrainData.alphamapLayers; layer++)
                {
                    if (layer == textureLayer)
                        alphaMaps[posY, posX, layer] = Mathf.Lerp(alphaMaps[posY, posX, layer], textureOpacity, falloff);
                    else
                        alphaMaps[posY, posX, layer] = Mathf.Lerp(alphaMaps[posY, posX, layer], 0, falloff);
                }
            }
        }
    }

    private Vector3 WorldToTerrainPosition(Vector3 worldPos)
    {
        Vector3 terrainPos = worldPos - terrain.transform.position;
        return new Vector3(
            terrainPos.x / terrainData.size.x,
            (terrainPos.y - baseHeight) / terrainData.size.y,
            terrainPos.z / terrainData.size.z
        );
    }

    private Vector3[] GetSplinePoints()
    {
        float currentT = 0;
        List<Vector3> points = new List<Vector3>();
        foreach (Spline spline in splineContainer.Splines)
        {
            while (currentT < 1)
            {
                points.Add(spline.EvaluatePosition(currentT));
                currentT += spacing;
            }
            currentT = 0;
        }
        return points.ToArray();
    }

    private void OnDrawGizmos()
    {
        if (showSpacingGizmo && terrain != null && GetSplinePoints() != null)
        {
            if (GetSplinePoints().Length <= 0 || spacing <= .01f)
                return;
            Gizmos.color = Color.green;
            foreach (var point in GetSplinePoints())
                Gizmos.DrawSphere(point, 1f);
        }
    }
}
