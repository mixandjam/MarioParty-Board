//This code is based on the spline-stamper-unity-terrain-tool repository
//https://github.com/Rumesic/spline-stamper-unity-terrain-tool/tree/master
//Check LICENSE file for more details

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode]
public class SplineTerrainStamper : MonoBehaviour
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
    [Range(1, 5)] public float spacing = 1;

    [Header("Performance")]
    [Tooltip("Delay in seconds after last change before updating terrain")]
    [Range(0.1f, 1f)] public float updateDelay = 0.3f;
    [Tooltip("Temporarily reduce spline sampling density during interaction")]
    [Range(1f, 10f)] public float interactiveSpacingMultiplier = 3f;

    [Header("Debug")]
    public bool showSpacingGizmo = false;

    private float[,] originalHeights;
    private float[,,] originalAlphaMaps;
    private bool needsUpdate = false;
    
    private float lastChangeTime;
    private float originalSpacing;

    private void OnEnable()
    {
        Spline.Changed += OnAnySplineChanged;
        originalSpacing = spacing;
        BackupTerrain();
    }

    private void OnDisable()
    {
        Spline.Changed -= OnAnySplineChanged;
    }

    private void Update()
    {
        // Restore original spacing after interaction
        if (Time.realtimeSinceStartup - lastChangeTime > updateDelay && spacing != originalSpacing)
        {
            spacing = originalSpacing;
            needsUpdate = true; // Ensure final high-res update
        }

        // Debounced update
        if (needsUpdate && Time.realtimeSinceStartup - lastChangeTime >= updateDelay)
        {
            needsUpdate = false;
            StampTerrain();
        }
    }

    private void OnAnySplineChanged(Spline spline, int knotIndex, SplineModification modificationType)
    {
        if (splineContainer != null && SplineIsInContainer(spline))
        {
            lastChangeTime = Time.realtimeSinceStartup;
            needsUpdate = true;
            // Increase spacing during interaction for fewer points
            spacing = originalSpacing * interactiveSpacingMultiplier;
        }
    }

    private bool SplineIsInContainer(Spline spline)
    {
        foreach (var containerSpline in splineContainer.Splines)
        {
            if (containerSpline == spline) return true;
        }
        return false;
    }

    [ContextMenu("Stamp Terrain")]
    public void StampTerrain()
    {
        InitializeTerrainData();

        if (terrain == null || terrainData == null) return;

        // Revert to original state
        if (originalHeights != null) terrainData.SetHeights(0, 0, originalHeights);
        if (originalAlphaMaps != null) terrainData.SetAlphamaps(0, 0, originalAlphaMaps);

        Vector3[] points = GetSplinePoints();
        if (points.Length == 0) return;

        // Batch process changes
        float[,] tempHeights = (float[,])originalHeights.Clone();
        float[,,] tempAlphaMaps = (float[,,])originalAlphaMaps.Clone();

        foreach (var point in points)
        {
            Vector3 terrainLocalPos = WorldToTerrainPosition(point);
            ApplyHeightChangeToTemp(terrainLocalPos, tempHeights, terrainData.heightmapResolution);
            ApplyTextureChangeToTemp(terrainLocalPos, tempAlphaMaps, terrainData.alphamapWidth, terrainData.alphamapHeight);
        }

        terrainData.SetHeights(0, 0, tempHeights);
        terrainData.SetAlphamaps(0, 0, tempAlphaMaps);
        terrainData.SyncHeightmap();
    }

    private void ApplyHeightChangeToTemp(Vector3 terrainPos, float[,] heights, int resolution)
    {
        int centerX = (int)(terrainPos.x * resolution);
        int centerY = (int)(terrainPos.z * resolution);
        float pathRadius = (pathWidth / 2f) / terrainData.size.x * resolution;
        float falloffRadius = heightFalloff / terrainData.size.x * resolution;
        float totalRadius = pathRadius + falloffRadius;
        float sqrTotalRadius = totalRadius * totalRadius;

        int minX = Mathf.Clamp(centerX - (int)totalRadius, 0, resolution - 1);
        int maxX = Mathf.Clamp(centerX + (int)totalRadius, 0, resolution - 1);
        int minY = Mathf.Clamp(centerY - (int)totalRadius, 0, resolution - 1);
        int maxY = Mathf.Clamp(centerY + (int)totalRadius, 0, resolution - 1);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float dx = x - centerX;
                float dy = y - centerY;
                float sqrDistance = dx * dx + dy * dy;

                if (sqrDistance > sqrTotalRadius) continue;

                float distance = Mathf.Sqrt(sqrDistance);
                float falloff = Mathf.Clamp01(1 - Mathf.Max(0, distance - pathRadius) / falloffRadius);
                float targetHeight = (terrainPos.y + baseHeight) / terrainData.size.y;
                heights[y, x] = Mathf.Lerp(heights[y, x], targetHeight, falloff);
            }
        }
    }
    private void ApplyTextureChangeToTemp(Vector3 terrainPos, float[,,] alphaMaps, int width, int height)
    {
        int centerX = (int)(terrainPos.x * width);
        int centerY = (int)(terrainPos.z * height);
        float radius = paintFalloff / terrainData.size.x * width;
        float sqrRadius = radius * radius;

        int minX = Mathf.Clamp(centerX - (int)radius, 0, width - 1);
        int maxX = Mathf.Clamp(centerX + (int)radius, 0, width - 1);
        int minY = Mathf.Clamp(centerY - (int)radius, 0, height - 1);
        int maxY = Mathf.Clamp(centerY + (int)radius, 0, height - 1);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float dx = x - centerX;
                float dy = y - centerY;
                float sqrDistance = dx * dx + dy * dy;

                if (sqrDistance > sqrRadius) continue;

                float distance = Mathf.Sqrt(sqrDistance);
                float falloff = Mathf.Clamp01(1 - (distance / radius));

                float totalWeight = 0f;
                for (int layer = 0; layer < terrainData.alphamapLayers; layer++)
                {
                    if (layer == textureLayer)
                    {
                        alphaMaps[y, x, layer] = Mathf.Lerp(
                            alphaMaps[y, x, layer], 
                            textureOpacity, 
                            falloff
                        );
                    }
                    else
                    {
                        alphaMaps[y, x, layer] = Mathf.Lerp(
                            alphaMaps[y, x, layer], 
                            0f, 
                            falloff
                        );
                    }
                    totalWeight += alphaMaps[y, x, layer];
                }

                // Normalize weights
                if (totalWeight > 0.0001f)
                {
                    for (int layer = 0; layer < terrainData.alphamapLayers; layer++)
                        alphaMaps[y, x, layer] /= totalWeight;
                }
            }
        }
    }

    [ContextMenu("Reset Terrain")]
    public void ResetTerrain()
    {
        InitializeTerrainData();
        
        if (terrainData == null)
        {
            Debug.LogError("TerrainData not initialized!");
            return;
        }

        // Reset heights to flat
        int heightmapResolution = terrainData.heightmapResolution;
        float[,] resetHeights = new float[heightmapResolution, heightmapResolution];
        terrainData.SetHeights(0, 0, resetHeights);

        // Reset textures to base layer
        int splatmapResolution = terrainData.alphamapResolution;
        float[,,] resetSplatmap = new float[splatmapResolution, splatmapResolution, terrainData.alphamapLayers];
        for (int y = 0; y < splatmapResolution; y++)
        {
            for (int x = 0; x < splatmapResolution; x++)
            {
                resetSplatmap[y, x, 0] = 1f; // Base layer
                for (int layer = 1; layer < terrainData.alphamapLayers; layer++)
                    resetSplatmap[y, x, layer] = 0f;
            }
        }
        terrainData.SetAlphamaps(0, 0, resetSplatmap);
        terrainData.SyncHeightmap();
        Debug.Log("Terrain fully reset to default state");
    }

    [ContextMenu("Load Backup")]
    public void LoadBackup()
    {
        InitializeTerrainData();
        
        if (terrainData == null)
        {
            Debug.LogError("TerrainData not initialized!");
            return;
        }

        if (originalHeights == null || originalAlphaMaps == null)
        {
            Debug.LogWarning("No backup exists! Use 'Backup Terrain' first.");
            return;
        }

        // Restore from backup
        terrainData.SetHeights(0, 0, originalHeights);
        terrainData.SetAlphamaps(0, 0, originalAlphaMaps);
        terrainData.SyncHeightmap();
        Debug.Log("Terrain restored from backup");
    }

    [ContextMenu("Backup Terrain")]
    public void BackupTerrain()
    {
        InitializeTerrainData();
        
        if (terrainData != null)
        {
            originalHeights = terrainData.GetHeights(0, 0, 
                terrainData.heightmapResolution, 
                terrainData.heightmapResolution);

            originalAlphaMaps = terrainData.GetAlphamaps(0, 0, 
                terrainData.alphamapWidth, 
                terrainData.alphamapHeight);
                
            Debug.Log("Terrain backup created");
        }
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
        List<Vector3> points = new List<Vector3>();

        foreach (Spline spline in splineContainer.Splines)
        {
            float splineLength = spline.GetLength();
            float stepSize = spacing / splineLength;
            float currentT = 0f;

            while (currentT < 1f)
            {
                points.Add(spline.EvaluatePosition(currentT));
                currentT += stepSize;
            }
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
