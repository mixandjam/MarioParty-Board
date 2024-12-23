using System;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(Terrain))]
[ExecuteAlways]
public class GenerateTerrain : MonoBehaviour
{
    private Terrain terrain;
    public SplineContainer splineContainer;

    [Header("Height Settings")]
    public bool modifyHeight = true;
    public float heightInfluence = 1f;
    public float baseHeight = 0f;

    [Header("Paint Settings")]
    public bool paintTexture = true;
    public TerrainLayer defaultLayer;
    public TerrainLayer targetLayer;
    public float paintStrength = 1f;

    [Header("Brush Settings")]
    public float brushSize = 5f;
    [Range(0.1f, 5f)]
    public float falloffPower = 2f;
    [Range(0.1f, 1f)]
    public float falloffScale = 0.5f;
    [Range(0.001f, 0.01f)]
    public float sampleDensity = 0.005f;
    [Range(0f, 1f)]
    public float centerStrength = 1f;

    private TerrainData terrainData;

    private void Awake()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
    }

    private void OnEnable()
    {
        Spline.Changed += OnSplineChanged;
    }


    private void OnDisable()
    {
        Spline.Changed -= OnSplineChanged;
    }
    private void OnSplineChanged(Spline spline, int arg2, SplineModification modification)
    {
        ModifyTerrain();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;

        ModifyTerrain();
    }

    private void ModifyTerrain()
    {
        InitializeTerrainLayers();

        if (modifyHeight)
            ModifyHeight();

        if (paintTexture && targetLayer != null)
            PaintTexture();
    }

    private void InitializeTerrainLayers()
    {
        if (defaultLayer == null && targetLayer == null)
        {
            Debug.LogError("Both default and target layers are null!");
            return;
        }

        int requiredLayers = 2;
        TerrainLayer[] newLayers = new TerrainLayer[requiredLayers];

        newLayers[0] = defaultLayer;
        newLayers[1] = targetLayer;

        terrainData.terrainLayers = newLayers;

        int alphamapRes = terrainData.alphamapResolution;
        float[,,] alphamaps = new float[alphamapRes, alphamapRes, requiredLayers];

        for (int y = 0; y < alphamapRes; y++)
        {
            for (int x = 0; x < alphamapRes; x++)
            {
                alphamaps[y, x, 0] = 1f;
                alphamaps[y, x, 1] = 0f;
            }
        }

        terrainData.SetAlphamaps(0, 0, alphamaps);
    }

    private void ModifyHeight()
    {
        int heightmapRes = terrainData.heightmapResolution;
        float[,] heights = new float[heightmapRes, heightmapRes];
        float[,] heightAccumulation = new float[heightmapRes, heightmapRes];
        float[,] weightAccumulation = new float[heightmapRes, heightmapRes];

        float normalizedBaseHeight = baseHeight / terrainData.size.y;

        for (int z = 0; z < heightmapRes; z++)
        {
            for (int x = 0; x < heightmapRes; x++)
            {
                heights[z, x] = normalizedBaseHeight;
                heightAccumulation[z, x] = normalizedBaseHeight;
                weightAccumulation[z, x] = 1;
            }
        }

        ProcessSplines(heightmapRes, heightAccumulation, weightAccumulation, true);


        for (int z = 0; z < heightmapRes; z++)
        {
            for (int x = 0; x < heightmapRes; x++)
            {
                if (weightAccumulation[z, x] > 0)
                {
                    heights[z, x] = heightAccumulation[z, x] / weightAccumulation[z, x];
                    heights[z, x] = Mathf.Clamp01(heights[z, x]);
                }
            }
        }

        terrainData.SetHeights(0, 0, heights);
    }

    private void PaintTexture()
    {
        if (defaultLayer == null || targetLayer == null)
        {
            Debug.LogError("Default or target layer is missing!");
            return;
        }

        int alphamapRes = terrainData.alphamapResolution;
        float[,,] alphamaps = terrainData.GetAlphamaps(0, 0, alphamapRes, alphamapRes);
        float[,] accumulation = new float[alphamapRes, alphamapRes];
        float[,] weightAccumulation = new float[alphamapRes, alphamapRes];

        ProcessSplines(alphamapRes, accumulation, weightAccumulation, false);


        for (int z = 0; z < alphamapRes; z++)
        {
            for (int x = 0; x < alphamapRes; x++)
            {
                if (weightAccumulation[z, x] > 0)
                {
                    float weight = (accumulation[z, x] / weightAccumulation[z, x]) * paintStrength;
                    alphamaps[z, x, 1] = weight;
                    alphamaps[z, x, 0] = 1f - weight;
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, alphamaps);
    }

    private void ProcessSplines(int resolution, float[,] accumulation, float[,] weightAccumulation, bool isHeight)
    {
        Vector3 terrainPosition = terrain.transform.position;
        float terrainWidth = terrainData.size.x;
        float terrainLength = terrainData.size.z;

        foreach (var spline in splineContainer.Splines)
        {
            for (float t = 0; t <= 1f; t += sampleDensity)
            {
                Vector3 splinePoint = splineContainer.transform.TransformPoint(spline.EvaluatePosition(t));

                int mapX = Mathf.RoundToInt(((splinePoint.x - terrainPosition.x) / terrainWidth) * (resolution - 1));
                int mapZ = Mathf.RoundToInt(((splinePoint.z - terrainPosition.z) / terrainLength) * (resolution - 1));

                int radius = Mathf.RoundToInt(brushSize * (resolution / terrainWidth));
                int startX = Mathf.Max(0, mapX - radius);
                int startY = Mathf.Max(0, mapZ - radius);
                int endX = Mathf.Min(resolution - 1, mapX + radius);
                int endY = Mathf.Min(resolution - 1, mapZ + radius);

                for (int z = startY; z <= endY; z++)
                {
                    for (int x = startX; x <= endX; x++)
                    {
                        float dx = (x - mapX);
                        float dz = (z - mapZ);
                        float distance = Mathf.Sqrt(dx * dx + dz * dz) / radius;

                        if (distance <= 1)
                        {
                            float falloff;
                            if (isHeight)
                            {
                                float scaledDistance = distance / falloffScale;
                                falloff = scaledDistance <= 1 ?
                                          1f :
                                          Mathf.Pow(Mathf.Cos((scaledDistance - 1f) * Mathf.PI * 0.5f), falloffPower);
                                falloff *= centerStrength;
                            }
                            else
                            {
                                falloff = Mathf.SmoothStep(1, 0, distance);
                            }

                            if (isHeight)
                            {
                                float normalizedBaseHeight = baseHeight / terrainData.size.y;
                                float normalizedInfluence = (heightInfluence / terrainData.size.y) - normalizedBaseHeight;
                                float newHeight = normalizedBaseHeight + (normalizedInfluence * falloff);
                                accumulation[z, x] += newHeight * falloff;
                            }
                            else
                            {
                                accumulation[z, x] += falloff;
                            }

                            weightAccumulation[z, x] += falloff;
                        }
                    }
                }
            }
        }
    }

}
