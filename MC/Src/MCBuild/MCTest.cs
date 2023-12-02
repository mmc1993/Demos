using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MCTest : MonoBehaviour
{
    public Material BatchMaterial;

    public bool[] PointArray = new bool[8];

    private Mesh mOutputMesh;

    private void Awake()
    {
        mOutputMesh = new();
    }

    private void Update()
    {
        var value = 0;
        for (var i = 0; i != PointArray.Length; ++i)
        {
            if (PointArray[i]) { value |= 1 << i; }
        }
        if (value != 0 && value != 255)
        {
            UpdateCubeValue(value);

            Graphics.DrawMesh(mOutputMesh, transform.localToWorldMatrix, BatchMaterial, 0);
        }
    }

    private void UpdateCubeValue(int value)
    {
        List<int>     indices = new();
        List<Vector3> vertexs = new();
        List<Vector3> normals = new();

        var indexBase = value * 16;
        for (var i = 0; i != 16; ++i)
        {
            var i0 = MCLut.kPointLut[indexBase + i * 3    ];
            var i1 = MCLut.kPointLut[indexBase + i * 3 + 1];
            var i2 = MCLut.kPointLut[indexBase + i * 3 + 2];
            var i0i1 = MCLut.kOffsetLut[i1] - MCLut.kOffsetLut[i0];
            var i1i2 = MCLut.kOffsetLut[i2] - MCLut.kOffsetLut[i1];
            var normal = Vector3.Cross(i0i1, i1i2).normalized;

            vertexs.Add(MCLut.kOffsetLut[i0]);
            vertexs.Add(MCLut.kOffsetLut[i1]);
            vertexs.Add(MCLut.kOffsetLut[i2]);
            indices.Add(vertexs.Count - 3);
            indices.Add(vertexs.Count - 2);
            indices.Add(vertexs.Count - 1);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);

            if (MCLut.kPointLut[indexBase + i * 3 + 3] == -1) { break; }
        }

        mOutputMesh.Clear();
        mOutputMesh.SetVertices(vertexs);
        mOutputMesh.SetNormals(normals);
        mOutputMesh.SetIndices(indices, MeshTopology.Triangles, 0);
    }

    private void OnDrawGizmos()
    {
        var points = new Vector3[]
        {
            transform.TransformPoint(new Vector3(-0.5f,  0.5f, -0.5f)),
            transform.TransformPoint(new Vector3(-0.5f,  0.5f,  0.5f)),
            transform.TransformPoint(new Vector3(-0.5f, -0.5f,  0.5f)),
            transform.TransformPoint(new Vector3(-0.5f, -0.5f, -0.5f)),
            transform.TransformPoint(new Vector3( 0.5f,  0.5f, -0.5f)),
            transform.TransformPoint(new Vector3( 0.5f,  0.5f,  0.5f)),
            transform.TransformPoint(new Vector3( 0.5f, -0.5f,  0.5f)),
            transform.TransformPoint(new Vector3( 0.5f, -0.5f, -0.5f)),
        };

        Gizmos.DrawLine(points[0], points[1]);
        Gizmos.DrawLine(points[1], points[2]);
        Gizmos.DrawLine(points[2], points[3]);
        Gizmos.DrawLine(points[3], points[0]);

        Gizmos.DrawLine(points[4], points[5]);
        Gizmos.DrawLine(points[5], points[6]);
        Gizmos.DrawLine(points[6], points[7]);
        Gizmos.DrawLine(points[7], points[4]);

        Gizmos.DrawLine(points[0], points[4]);
        Gizmos.DrawLine(points[1], points[5]);
        Gizmos.DrawLine(points[2], points[6]);
        Gizmos.DrawLine(points[3], points[7]);

        for (var i = 0; i != points.Length; ++i)
        {
            Gizmos.color = PointArray[i]
                            ? Color.white
                            : Color.black;
            Gizmos.DrawSphere(points[i], 0.1f);
        }
    }
}
