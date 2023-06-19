using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SoftRenderer
{

    public struct TriangleShadingJob : IJobParallelFor
    {


        public NativeArray<VSOutBuf> vsOutBuf;

        public NativeArray<float> zbuffer;

        public Matrix4x4 varying_uv;
        public Matrix4x4 varying_normal;
        public Matrix4x4 varying_tri;

        public void Execute(int index)
        {
            Vector2 boxMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 boxMax = new Vector2();
            Vector2 clamp = new Vector2(SoftRenderer.width - 1, SoftRenderer.height - 1);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    vsOutBuf[index].

                    boxMin[j] = Math.Max(0f, (int)Math.Min(boxMin[j], vsOutBuf[index].worldPos[i] / vsOutBuf[index].worldPos[i][3]));
                    boxMax[j] = Math.Min((int)clamp[j], (int)Math.Max(boxMax[j], points[i][j] / points[i][3]));
                }
            }
        }

        public bool fragment(Vector3 bar, out Color color, Texture2D output)
        {

            float z = varying_tri[2, 0] * bar.x + varying_tri[2, 1] * bar.y + varying_tri[2, 2] * bar.z;
            color = new Color(z * 5, z * 5, z * 5, 1);


            return !SoftRenderer.Instance.shadow || SoftRenderer.Instance.model;
        }
    }

}