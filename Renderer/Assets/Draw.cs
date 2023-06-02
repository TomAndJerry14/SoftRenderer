using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;


namespace SoftRenderer
{
    internal class Draw
    {
        public static Color[,] buffers = new Color[SoftRenderer.width, SoftRenderer.height];

        private Camera camera;

        public Draw(Camera camera)
        {
            this.camera = camera;
            for (int i = 0; i < SoftRenderer.width; i++)
            {
                for (int j = 0; j < SoftRenderer.height; j++)
                {
                    buffers[i, j] = Color.black;
                }
            }
        }

        public void DrawPixel(int x0, int y0, Color color)
        {
            camera.renderer.SetPixel(x0, y0, color);
        }

        public void DrawLine(int x0, int y0, int x1, int y1, Color color)
        {
            if (x0 > x1)
                DrawLineInternal(x1, y1, x0, y0, color);
            else
                DrawLineInternal(x0, y0, x1, y1, color);
        }

        private void DrawLineInternal(int x0, int y0, int x1, int y1, Color color)
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
                    DrawPixel(y, x, color);
                else
                    DrawPixel(x, y, color);

            }
        }

        public void DrawObj(Object obj)
        {
            var fv = obj.face_vertices;
            int len = fv.Length;

            SoftRenderer.Instance.StartCoroutine(DrawObject(len, obj, fv));
        }

        IEnumerator DrawObject(int len, Object obj, int[][] fv)
        {
            for (int i = 0; i < len; i++)
            {
                Vector3[] oriPos = new Vector3[3];
                for (int j = 0; j < 3; j++)
                {
                    oriPos[j] = obj.vertices[fv[i][j] - 1];
                }

                Vector3[] worldPos = new Vector3[3]
                {
                      camera.Matrix2CameraLocal(oriPos[0]),
                      camera.Matrix2CameraLocal(oriPos[1]),
                      camera.Matrix2CameraLocal(oriPos[2]),
                };

                Vector3[] vt = new Vector3[3];
                Color[] uvColor = new Color[3];
                for (int j = 0; j < 3; j++)
                {
                    var temp = obj.texture_vertices[fv[i][j] - 1];
                    vt[j] = temp;
                        var texture = SoftRenderer.Instance.texture;
                    uvColor[j] = SoftRenderer.Instance.texture.GetPixel(temp.x * texture.width, temp.y * texture.height);
                }

                Vector3 normal = Vector3.Cross(oriPos[2] - oriPos[0], oriPos[1] - oriPos[0]);
                normal.Normalize();
                Vector3 lightDir = -Vector3.forward;
                lightDir.Normalize();
                float intensity = Vector3.Dot(lightDir, normal);

                for (int j = 0; j < 3; j++)
                {
                    worldPos[j] += new Vector3(SoftRenderer.width / 2, SoftRenderer.height / 2, 0);
                }

                if (intensity > 0)
                    DrawTriangle(worldPos, vt, new Color(intensity, intensity, intensity, 1),uvColor);
                //DrawTriangle( worldPos[0], worldPos[1], worldPos[2], new Color(intensity, intensity, intensity, 1));

                DrawPixel(i, i, Color.white);
                if (i % 100 == 0)
                {
                    camera.renderer.Apply();
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        private Vector3 Barycentric(Vector3 pos0, Vector3 pos1, Vector3 pos2, Vector3 p)
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
        public void DrawTriangle(Vector3[] points, Vector3[] vts, Color color)
        {
            //读取uv
            //SoftRenderer.width

            Vector2 boxMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 boxMax = new Vector2();
            Vector2 clamp = new Vector2(camera.renderer.width - 1, camera.renderer.height - 1);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    boxMin[j] = Math.Max(0f, (int)Math.Min(boxMin[j], points[i][j]));
                    boxMax[j] = Math.Min((int)clamp[j], (int)Math.Max(boxMax[j], points[i][j]));
                }
            }

            Vector3 p = new Vector3();
            for (p.x = boxMin.x; p.x <= boxMax.x; p.x++)
            {
                for (p.y = boxMin.y; p.y <= boxMax.y; p.y++)
                {
                    var barycentric = Barycentric(points[0], points[1], points[2], p);

                    if (barycentric.x < 0 || barycentric.y < 0 || barycentric.z < 0) { continue; }

                    Debug.Log(barycentric);
                    p.z = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        p.z += points[i].z * barycentric[i];
                    }
                    if (camera.zBuffer[(int)p.x, (int)p.y] <= p.z)
                    {
                        camera.zBuffer[(int)p.x, (int)p.y] = p.z;

                        //如果绘制,读取点插值贴图的数据
                        var uvPoint = p - points[0];
                        int w = (int)(uvPoint.x * SoftRenderer.width);
                        int h = (int)(uvPoint.y * SoftRenderer.height);
                        color = SoftRenderer.Instance.texture.GetPixel(h, w);
                        DrawPixel((int)p.x, (int)p.y, color);
                        Color uvColor0 = SoftRenderer.Instance.texture.GetPixel((int)(vts[0].x * SoftRenderer.Instance.texture.width), (int)(vts[0].y * SoftRenderer.Instance.texture.height));
                        Color uvColor1 = SoftRenderer.Instance.texture.GetPixel((int)(vts[1].x * SoftRenderer.Instance.texture.width), (int)(vts[1].y * SoftRenderer.Instance.texture.height));
                        Color uvColor2 = SoftRenderer.Instance.texture.GetPixel((int)(vts[2].x * SoftRenderer.Instance.texture.width), (int)(vts[2].y * SoftRenderer.Instance.texture.height));
                        Color uvColor = (1 - barycentric.y - barycentric.z) * uvColor0 + barycentric.y * uvColor1 + barycentric.z * uvColor2;
                        if ((1 - barycentric.y - barycentric.z) == 1f)
                        {
                            Debug.Log(points[0], vts[0]);
                        }

                        if ((barycentric.y ) == 1f)
                        {
                            Debug.Log(points[1], vts[1]);
                        }

                        if ((barycentric.z) == 1f)
                        {
                            Debug.Log(points[2], vts[2]);
                        }
                        //Debug.Log(uvColor0, (int)(vts[0].x * SoftRenderer.Instance.texture.width), (int)(vts[0].y * SoftRenderer.Instance.texture.height));
                        //Debug.Log(uvColor0, (int)(vts[1].x * SoftRenderer.Instance.texture.width), (int)(vts[1].y * SoftRenderer.Instance.texture.height));
                        //Debug.Log(uvColor0, (int)(vts[2].x * SoftRenderer.Instance.texture.width), (int)(vts[2].y * SoftRenderer.Instance.texture.height));
                        //Debug.Log(uvColor);

                        //Debug.Log((int)p.x, (int)p.y, uvColor);

                        DrawPixel((int)p.x, (int)p.y, uvColor);
                    }

                }
            }
        }


        //public void DrawTriangle(Vector2 pos0, Vector2 pos1, Vector2 pos2, Color color)
        //{
        //    for (int i = 0; i < 2; i++)
        //    {
        //        pos0[i] *= 200;
        //        pos1[i] *= 200;
        //        pos2[i] *= 200;
        //        pos0[i] += 200;
        //        pos1[i] += 200;
        //        pos2[i] += 200;
        //    }

        //    if (pos0.y == pos1.y && pos1.y == pos2.y)
        //    {
        //        Debug.Log("============");
        //        return;
        //    }
        //    if (pos0.y > pos1.y)
        //        Swap(ref pos0, ref pos1);
        //    if (pos0.y > pos2.y)
        //        Swap(ref pos0, ref pos2);
        //    if (pos1.y > pos2.y)
        //        Swap(ref pos1, ref pos2);


        //    if (pos0.y == pos1.y)
        //    {
        //        if (pos0.x > pos1.x)
        //            Swap(ref pos0, ref pos1);
        //        DrawTriangleInternal(pos0, pos1, pos2, color);
        //    }
        //    else if (pos1.y == pos2.y)
        //    {
        //        DrawTriangleInternal(pos1, pos2, pos0, color);
        //    }
        //    else
        //    {
        //        float CulculateX(float x1, float x2, float y1, float y2, float y)
        //        {
        //            if (y2 == y1)
        //            {
        //                throw new Exception($"不能为直线,x1:{x1},x2:{x2},y1:{y1},y2:{y2}");
        //            }
        //            if (x2 == x1)
        //                return x1;
        //            float x = (x2 - x1) / (y2 - y1) * (y - y1) + x1;
        //            return x;
        //        }
        //        //拆分两个三角形
        //        int totalHeight = (int)(pos2.y - pos0.y);
        //        Vector2 centerPos = new Vector2(CulculateX(pos0.x, pos2.x, pos0.y, pos2.y, pos1.y), pos1.y);

        //        //上三角形
        //        Vector2 pos2_t = pos2;
        //        Vector2 pos1_t = centerPos.x > pos1.x ? centerPos : pos1;
        //        Vector2 pos0_t = centerPos.x > pos1.x ? pos1 : centerPos;
        //        DrawTriangleInternal(pos0_t, pos1_t, pos2_t, color);

        //        //下三角形，需要往下一格
        //        Vector2 pos2_b = pos0;//最低点
        //        int y = (int)pos1.y - 1;
        //        Vector2 pos0_b = new Vector2(CulculateX(pos0_t.x, pos2_b.x, pos0_t.y, pos2_b.y, y), y);
        //        Vector2 pos1_b = new Vector2(CulculateX(pos1_t.x, pos2_b.x, pos1_t.y, pos2_b.y, y), y);
        //        if (pos0_b.x > pos1_b.x)
        //            Swap(ref pos0_b, ref pos1_b);
        //        DrawTriangleInternal(pos0_b, pos1_b, pos2_b, color);
        //    }
        //}

        public void DrawTriangleInternal(Vector2 pos0, Vector2 pos1, Vector2 pos2, Color color)
        {
            if (pos0.y == pos1.y && pos1.y == pos2.y)
            {
                DrawLine((int)Math.Min(Math.Min(pos0.x, pos1.x), pos2.x), (int)pos0.y, (int)Math.Max(Math.Max(pos0.x, pos1.x), pos2.x), (int)pos0.y, color);
                Debug.Log((int)Math.Min(Math.Min(pos0.x, pos1.x), pos2.x), (int)pos0.y, (int)Math.Max(Math.Max(pos0.x, pos1.x), pos2.x), (int)pos0.y);
                return;
            }

            int totalHeight = (int)pos2.y - (int)pos0.y;
            bool up = pos2.y >= pos0.y;
            for (int i = 0; up ? i < totalHeight : i > totalHeight; i = (up ? i + 1 : i - 1))
            {
                var t = (float)i / totalHeight;
                Vector2 A = pos0 + (pos2 - pos0) * t;
                Vector2 B = pos1 + (pos2 - pos1) * t;
                for (int j = (int)A.x; j <= (int)B.x; j++)
                {
                    DrawPixel(j, (int)pos0.y + i, color);
                }
            }
        }

        public void DrawBottomTriangle(Vector2 pos0, Vector2 pos1, Vector2 pos2, Color color)
        {
            if (pos0.y == pos1.y && pos1.y == pos2.y)
            {
                return;
            }

            int totalHeight = (int)pos2.y - (int)pos0.y;
            for (int i = 0; i < totalHeight; i++)
            {
                var t = (float)i / totalHeight;
                Vector2 A = pos0 + (pos2 - pos0) * t;
                Vector2 B = pos1 + (pos2 - pos1) * t;
                for (int j = (int)A.x; j <= (int)B.x; j++)
                {
                    DrawPixel(j, (int)pos0.y + i, color);
                }
            }
        }

        public static Vector2 WorldPos2ScreenPos(Vector3 pos)
        {
            int x = (int)((pos.x + 1) * 400);
            int y = (int)((pos.y + 1) * 400);
            return new Vector2(x, y);
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
