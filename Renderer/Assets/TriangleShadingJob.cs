using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct TriangleShadingJob : IJobParallelFor
{


    public NativeArray<VSOutBuf> VsOutBuf;

    public Matrix4x4 varying_uv;
    public Matrix4x4 varying_normal;
    public Matrix4x4 varying_tri;

    public void Execute(int index)
    {

    }

    public bool fragment(Vector3 bar, out Color color, Texture2D output)
    {

        float z = varying_tri[2, 0] * bar.x + varying_tri[2, 1] * bar.y + varying_tri[2, 2] * bar.z;
        color = new Color(z * 5, z * 5, z * 5, 1);
        //color = Color.white;


        return !SoftRenderer.Instance.shadow || SoftRenderer.Instance.model;
    }
}
