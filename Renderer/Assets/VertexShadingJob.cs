using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct VertexShadingJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<Vector3> points;
    [ReadOnly]
    public NativeArray<Vector3> normals;
    [ReadOnly]
    public NativeArray<Vector2> uvs;
    [ReadOnly]
    public NativeArray<Vector3Int> triangleVertexData;
    [ReadOnly]
    public NativeArray<Vector3Int> triangleUVData;
    public NativeArray<VSOutBuf> result;

    [ReadOnly]
    public Matrix4x4 ModelView;
    [ReadOnly]
    public Matrix4x4 Projection;
    [ReadOnly]
    public Matrix4x4 Viewport;

    public void Execute(int index)
    {
        for (int i = 0; i < 3; i++)
        {
            vertex(index, i);
        }

    }

    private void vertex(int faceIndex, int vertexIndex)
    {
        result[faceIndex].varying_uv.SetColumn(vertexIndex, normals[triangleUVData[faceIndex][vertexIndex]]);

        Vector3 vertex = points[triangleVertexData[faceIndex][vertexIndex]];
        Vector4 gl_Vertex = Viewport * Projection * ModelView * new Vector4(vertex.x, vertex.y, vertex.z, 1);
        result[faceIndex].varying_tri.SetColumn(vertexIndex, gl_Vertex);
        result[faceIndex].screen_crood.Set(gl_Vertex.x, gl_Vertex.y, gl_Vertex.z, 1);
    }
}
