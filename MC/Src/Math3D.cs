using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Math3D
{
    public static bool IsContains(Vector3 halfAABB, Vector3 point)
    {
        return point.x >= -halfAABB.x && point.x <= halfAABB.x
            && point.y >= -halfAABB.x && point.y <= halfAABB.y
            && point.z >= -halfAABB.z && point.z <= halfAABB.z;
    }
}
