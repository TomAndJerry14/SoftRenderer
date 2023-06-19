using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SoftRenderer
{
    internal struct shader_a2v
    {
        public int vertex_index;
        public Vector4 world_pos;
        public Vector3 normal;
        public Vector3 tangent;
        public Vector4 texcoord;
        public Vector4 texcoord1;
        public Vector4 texcoord2;
        public Vector4 texcoord3;
    }

    internal struct shader_v2f
    {
        public Vector4 position;
        public Color color;
    }


    public class MyGL
    {
        //public static Texture2D output;

        public static void Triangle(Vector4[] points, IShader shader, Texture2D output, ref float[] zbuffer)
        {
            Vector2 boxMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 boxMax = new Vector2();
            Vector2 clamp = new Vector2(output.width - 1, output.height - 1);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    boxMin[j] = Math.Max(0f, (int)Math.Min(boxMin[j], points[i][j] / points[i][3]));
                    boxMax[j] = Math.Min((int)clamp[j], (int)Math.Max(boxMax[j], points[i][j] / points[i][3]));
                }
            }

            Vector3 p = new Vector3();
            for (p.x = boxMin.x; p.x <= boxMax.x; p.x++)
            {
                for (p.y = boxMin.y; p.y <= boxMax.y; p.y++)
                {
                    //这个为经过MVP转换后再处理的质心坐标
                    var barycentric = Barycentric(points[0] / points[0][3], points[1] / points[1][3], points[2] / points[2][3], p);
                    if (barycentric.x < 0 || barycentric.y < 0 || barycentric.z < 0) { continue; }

                    //透视矫正 - 质心坐标矫正
                    Vector3 barycentric_revised = new Vector3();
                    for (int i = 0; i < 3; i++)
                    {
                        barycentric_revised[i] = barycentric[i] / points[i][3];
                    }
                    float z_n = 1f / (barycentric_revised[0] + barycentric_revised[1] + barycentric_revised[2]);
                    for (int i = 0; i < 3; i++)
                    {
                        barycentric_revised[i] *= z_n;
                    }

                    var zbufferIndex = (int)p.y * output.width + (int)p.x;
                    if (zbuffer[zbufferIndex] <= p.z)
                    {
                        zbuffer[zbufferIndex] = p.z;
                        bool discard = shader.fragment(barycentric_revised, out var color, output);
                        if (!discard)
                        {
                            DrawPixel(output, (int)p.x, (int)p.y, color);
                        }
                    }

                }
            }
        }

        public static Vector3 Barycentric(Vector3 pos0, Vector3 pos1, Vector3 pos2, Vector3 p)
        {
            Vector3[] s = new Vector3[3];
            for (int i = 0; i <= 2; i++)
            {
                s[i].x = pos2[i] - pos0[i];
                s[i].y = pos1[i] - pos0[i];
                s[i].z = pos0[i] - p[i];
            }
            Vector3 u = Vector3.Cross(s[0], s[1]);

            //叉积第三位
            if (Math.Abs(u.z) > 1e-2)
            {
                return new Vector3(1f - (u.x + u.y) / u.z, u.y / u.z, u.x / u.z);
            }
            return new Vector3(-1, 1, 1);
        }


        public static void DrawPixel(Texture2D output, int x0, int y0, Color color)
        {
            output.SetPixel(x0, y0, color);
        }

        public static void DrawLine(Texture2D output, int x0, int y0, int x1, int y1, Color color)
        {
            if (x0 > x1)
                DrawLineInternal(output, x1, y1, x0, y0, color);
            else
                DrawLineInternal(output, x0, y0, x1, y1, color);
        }

        private static void DrawLineInternal(Texture2D output, int x0, int y0, int x1, int y1, Color color)
        {
            bool steep = false;
            if (Mathf.Abs(x1 - x0) < Mathf.Abs(y1 - y0))
            {
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
                steep = true;
            }

            for (int i = 0; i <= x1 - x0; i++)
            {
                int x = x0 + i;
                float t = (float)(x - x0) / (x1 - x0);
                int y = (int)((y1 - y0) * t + y0);
                if (steep)
                    DrawPixel(output, y, x, color);
                else
                    DrawPixel(output, x, y, color);

            }
        }

        private static void Swap(ref int a, ref int b)
        {
            int t = a;
            a = b;
            b = t;
        }

        private static void Swap(ref Vector2 a, ref Vector2 b)
        {
            Vector2 t = a;
            a = b;
            b = t;
        }

    }
}
