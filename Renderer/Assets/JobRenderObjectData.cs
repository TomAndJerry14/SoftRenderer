using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public struct JobRenderObjectData
{
    public NativeArray<Vector3> points;
    public NativeArray<Vector3> normals;
    public NativeArray<Vector2> uvs;
    public NativeArray<Vector3Int> triangleVertexData;
    public NativeArray<Vector3Int> triangleUVData;

    public void Release()
    {
        points.Dispose();
        normals.Dispose();
        uvs.Dispose();
        triangleVertexData.Dispose();
    }
}
