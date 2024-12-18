using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode]
[RequireComponent(typeof(SplineContainer), typeof(MeshRenderer), typeof(MeshFilter))]
public class SplineMeshGenerator : MonoBehaviour
{
    [SerializeField]
    private SplineContainer m_SplineContainer;

    [SerializeField]
    private Mesh m_Mesh;

    [SerializeField]
    private int m_SegmentsPerMeter = 1;

    [SerializeField]
    private float m_LengthScale = 1f;

    [SerializeField]
    private float m_Thickness = 0.5f;

    private List<Vector3> m_Positions = new List<Vector3>();
    private List<Vector3> m_Normals = new List<Vector3>();
    private List<Vector2> m_UVs = new List<Vector2>();
    private List<int> m_Indices = new List<int>();

    private void OnEnable()
    {
        GenerateMesh();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            GenerateMesh();
        }
    }

    private void GenerateMesh()
    {
        if (m_Mesh == null)
            m_Mesh = new Mesh();

        m_Mesh.Clear();
        m_Positions.Clear();
        m_Normals.Clear();
        m_UVs.Clear();
        m_Indices.Clear();

        if (m_SplineContainer == null)
            m_SplineContainer = GetComponent<SplineContainer>();

        foreach (var spline in m_SplineContainer.Splines)
        {
            if (spline == null || spline.Count < 2)
                continue;

            GenerateSplineMesh(spline);
        }

        m_Mesh.SetVertices(m_Positions);
        m_Mesh.SetNormals(m_Normals);
        m_Mesh.SetUVs(0, m_UVs);
        m_Mesh.SetIndices(m_Indices, MeshTopology.Triangles, 0);
        m_Mesh.UploadMeshData(false);

        GetComponent<MeshFilter>().sharedMesh = m_Mesh;

        GetComponent<MeshRenderer>().material = Resources.Load<Material>("Road2");
    }

    private void GenerateSplineMesh(Spline spline)
    {
        float length = spline.GetLength() * m_LengthScale;
        if (length <= 0.001f)
            return;

        int segments = Mathf.CeilToInt(m_SegmentsPerMeter * length);
        segments = Mathf.Max(segments, 1);
        float step = 1f / segments;
        int prevVertexCount = m_Positions.Count;

        for (float t = 0; t <= 1f; t += step)
        {
            SplineUtility.Evaluate(spline, t, out var position, out var direction, out var up);
            Vector3 tangent = Vector3.Cross(up, direction).normalized * m_Thickness;

            m_Positions.Add((Vector3)position - tangent);
            m_Positions.Add((Vector3)position + tangent);

            m_Normals.Add(up);
            m_Normals.Add(up);

            m_UVs.Add(new Vector2(0, t));
            m_UVs.Add(new Vector2(1, t));
        }

        int vertexCount = m_Positions.Count - prevVertexCount;
        for (int i = 0; i < vertexCount - 2; i += 2)
        {
            m_Indices.Add(prevVertexCount + i);
            m_Indices.Add(prevVertexCount + i + 2);
            m_Indices.Add(prevVertexCount + i + 1);

            m_Indices.Add(prevVertexCount + i + 1);
            m_Indices.Add(prevVertexCount + i + 2);
            m_Indices.Add(prevVertexCount + i + 3);
        }
    }

}
