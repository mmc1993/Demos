using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCBuild : MonoBehaviour
{
    public float UnitCubeScale = 1;
    public int UnitCubeCountX = 0;
    public int UnitCubeCountY = 0;
    public int UnitCubeCountZ = 0;

    public Material BatchMaterial;

    public GameObject Colliders;

    private static Vector3[] kCubePoints = new Vector3[]
    {
        new Vector3(-0.5f,  0.5f, -0.5f),
        new Vector3(-0.5f,  0.5f,  0.5f),
        new Vector3(-0.5f, -0.5f,  0.5f),
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3( 0.5f,  0.5f, -0.5f),
        new Vector3( 0.5f,  0.5f,  0.5f),
        new Vector3( 0.5f, -0.5f,  0.5f),
        new Vector3( 0.5f, -0.5f, -0.5f),
    };

    [System.Serializable]
    class Element
    {
        public Mesh   Mesh;
        public bool[] Flag;
    }

    [SerializeField]
    private List<Element> mElements = new();

    private void OnEnable()
    {
        mElements.Clear();

        var length = UnitCubeCountX
                   * UnitCubeCountY
                   * UnitCubeCountZ;
        for (var i = 0; i != length; ++i)
        {
            mElements.Add(new Element
            {
                Mesh = new(), Flag = new bool[8]
            });
        }

        Build();
    }

    private void Update()
    {
        for (var i = 0; i != mElements.Count; ++i)
        {
            Graphics.DrawMesh(mElements[i].Mesh, transform.localToWorldMatrix, BatchMaterial, 0);
        }
    }


    private System.ValueTuple<int, int, int> GetIndex3(int index)
    {
        var yz = index % (UnitCubeCountY * UnitCubeCountZ);
        var ix = index / (UnitCubeCountY * UnitCubeCountZ);
        var iy = yz / UnitCubeCountZ;
        var iz = yz % UnitCubeCountZ;
        return (ix, iy, iz);
    }

    private Vector3 GetPoint(int ix, int iy, int iz, int ip)
    {
        return GetPoint(ix, iy, iz) + kCubePoints[ip] * UnitCubeScale;
    }

    private Vector3 GetPoint(int ix, int iy, int iz)
    {
        var startX = -UnitCubeCountX * UnitCubeScale * 0.5f + UnitCubeScale * 0.5f;
        var startY = -UnitCubeCountY * UnitCubeScale * 0.5f + UnitCubeScale * 0.5f;
        var startZ = -UnitCubeCountZ * UnitCubeScale * 0.5f + UnitCubeScale * 0.5f;

        var originX = startX + ix * UnitCubeScale;
        var originY = startY + iy * UnitCubeScale;
        var originZ = startZ + iz * UnitCubeScale;

        return new Vector3(originX, originY, originZ);
    }

    private void Build()
    {
        var colliders = Colliders.GetComponentsInChildren<BoxCollider>();
        for (var i = 0; i != mElements.Count; ++i)
        {
            var element = mElements[i];
            var (ix, iy, iz) = GetIndex3(i);
            var p0 = transform.TransformPoint(GetPoint(ix, iy, iz, 0));
            var p1 = transform.TransformPoint(GetPoint(ix, iy, iz, 1));
            var p2 = transform.TransformPoint(GetPoint(ix, iy, iz, 2));
            var p3 = transform.TransformPoint(GetPoint(ix, iy, iz, 3));
            var p4 = transform.TransformPoint(GetPoint(ix, iy, iz, 4));
            var p5 = transform.TransformPoint(GetPoint(ix, iy, iz, 5));
            var p6 = transform.TransformPoint(GetPoint(ix, iy, iz, 6));
            var p7 = transform.TransformPoint(GetPoint(ix, iy, iz, 7));
            element.Flag[0] = !IsContains(p0, colliders);
            element.Flag[1] = !IsContains(p1, colliders);
            element.Flag[2] = !IsContains(p2, colliders);
            element.Flag[3] = !IsContains(p3, colliders);
            element.Flag[4] = !IsContains(p4, colliders);
            element.Flag[5] = !IsContains(p5, colliders);
            element.Flag[6] = !IsContains(p6, colliders);
            element.Flag[7] = !IsContains(p7, colliders);

            var flag = 0;
            if (element.Flag[0]) { flag |= 1; }
            if (element.Flag[1]) { flag |= 2; }
            if (element.Flag[2]) { flag |= 4; }
            if (element.Flag[3]) { flag |= 8; }
            if (element.Flag[4]) { flag |= 16; }
            if (element.Flag[5]) { flag |= 32; }
            if (element.Flag[6]) { flag |= 64; }
            if (element.Flag[7]) { flag |= 128; }
            if (flag != 0 && flag != 255)
            {
                BuildCube(GetPoint(ix, iy, iz), flag, ref element.Mesh);
            }
        }
    }

    private void BuildCube(Vector3 origin, int flag, ref Mesh mesh)
    {
        List<int> indices = new();
        List<Vector3> vertexs = new();
        List<Vector3> normals = new();

        var indexBase = flag * 16;
        for (var i = 0; i != 16; ++i)
        {
            var i0 = MCLut.kPointLut[indexBase + i * 3    ];
            var i1 = MCLut.kPointLut[indexBase + i * 3 + 1];
            var i2 = MCLut.kPointLut[indexBase + i * 3 + 2];
            var i0i1 = MCLut.kOffsetLut[i1] - MCLut.kOffsetLut[i0];
            var i1i2 = MCLut.kOffsetLut[i2] - MCLut.kOffsetLut[i1];
            var normal = Vector3.Cross(i0i1, i1i2).normalized;

            vertexs.Add(MCLut.kOffsetLut[i0] * UnitCubeScale + origin);
            vertexs.Add(MCLut.kOffsetLut[i1] * UnitCubeScale + origin);
            vertexs.Add(MCLut.kOffsetLut[i2] * UnitCubeScale + origin);
            indices.Add(vertexs.Count - 3);
            indices.Add(vertexs.Count - 2);
            indices.Add(vertexs.Count - 1);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);

            if (MCLut.kPointLut[indexBase + i * 3 + 3] == -1) { break; }
        }

        mesh.Clear();
        mesh.SetVertices(vertexs);
        mesh.SetNormals(normals);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
    }

    private bool IsContains(Vector3 worldPoint, BoxCollider[] colliders)
    {
        return System.Array.FindIndex(colliders, v =>
        {
            var localPoint = v.transform.InverseTransformPoint(worldPoint);
            return Math3D.IsContains(Vector3.one * 0.5f, localPoint);
        }) != -1;
    }

    private void OnDrawGizmos()
    {
        var halfX = UnitCubeCountX * UnitCubeScale * 0.5f;
        var halfY = UnitCubeCountY * UnitCubeScale * 0.5f;
        var halfZ = UnitCubeCountZ * UnitCubeScale * 0.5f;

        var p0 = transform.TransformPoint(new Vector3(-halfX, -halfY, -halfZ));
        var p1 = transform.TransformPoint(new Vector3(-halfX, -halfY,  halfZ));
        var p2 = transform.TransformPoint(new Vector3(-halfX,  halfY,  halfZ));
        var p3 = transform.TransformPoint(new Vector3(-halfX,  halfY, -halfZ));

        var p4 = transform.TransformPoint(new Vector3( halfX, -halfY, -halfZ));
        var p5 = transform.TransformPoint(new Vector3( halfX, -halfY,  halfZ));
        var p6 = transform.TransformPoint(new Vector3( halfX,  halfY,  halfZ));
        var p7 = transform.TransformPoint(new Vector3( halfX,  halfY, -halfZ));

        Gizmos.color = Color.black;
        Gizmos.DrawLine(p0, p1); Gizmos.DrawLine(p1, p2); Gizmos.DrawLine(p2, p3); Gizmos.DrawLine(p3, p0);
        Gizmos.DrawLine(p4, p5); Gizmos.DrawLine(p5, p6); Gizmos.DrawLine(p6, p7); Gizmos.DrawLine(p7, p4);
        Gizmos.DrawLine(p0, p4); Gizmos.DrawLine(p1, p5); Gizmos.DrawLine(p2, p6); Gizmos.DrawLine(p3, p7);

        for (var x = 0; x != UnitCubeCountX; ++x)
        for (var y = 0; y != UnitCubeCountY; ++y)
        for (var z = 0; z != UnitCubeCountZ; ++z)
        {
            p0 = transform.TransformPoint(GetPoint(x, y, z, 0));
            p1 = transform.TransformPoint(GetPoint(x, y, z, 1));
            p2 = transform.TransformPoint(GetPoint(x, y, z, 2));
            p3 = transform.TransformPoint(GetPoint(x, y, z, 3));

            p4 = transform.TransformPoint(GetPoint(x, y, z, 4));
            p5 = transform.TransformPoint(GetPoint(x, y, z, 5));
            p6 = transform.TransformPoint(GetPoint(x, y, z, 6));
            p7 = transform.TransformPoint(GetPoint(x, y, z, 7));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(p0, p1); Gizmos.DrawLine(p1, p2); Gizmos.DrawLine(p2, p3); Gizmos.DrawLine(p3, p0);
            Gizmos.DrawLine(p4, p5); Gizmos.DrawLine(p5, p6); Gizmos.DrawLine(p6, p7); Gizmos.DrawLine(p7, p4);
            Gizmos.DrawLine(p0, p4); Gizmos.DrawLine(p1, p5); Gizmos.DrawLine(p2, p6); Gizmos.DrawLine(p3, p7);
        }
    }
}
