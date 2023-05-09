using Cysharp.Threading.Tasks;
using System;
using UnityEngine;


namespace SoftRenderer
{
    internal static class Draw
    {
        public static Color[,] buffers = new Color[Screen.width, Screen.height];
        static Draw()
        {
            for (int i = 0; i < Screen.width; i++)
            {
                for (int j = 0; j < Screen.height; j++)
                {
                    buffers[i, j] = Color.black;
                }
            }
        }

        public static void DrawPixel(Texture2D texture, int x0, int y0, Color color)
        {
            UniTask.DelayFrame(10);
            texture.SetPixel(x0, y0, color);
        }

        public static void DrawLine(Texture2D texture, int x0, int y0, int x1, int y1, Color color)
        {
            if (x0 > x1)
                DrawLineInternal(texture, x1, y1, x0, y0, color);
            else
                DrawLineInternal(texture, x0, y0, x1, y1, color);
        }

        private static void DrawLineInternal(Texture2D texture, int x0, int y0, int x1, int y1, Color color)
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
                    DrawPixel(texture, y, x, color);
                else
                    DrawPixel(texture, x, y, color);

            }
        }

        public static void DrawObj(Texture2D texture, Object obj)
        {
            var fv = obj.face_vertices;
            int len = fv.Length;

            for (int i = 0; i < len; i++)
            {
                Vector3[] worldPos = new Vector3[3]
                {
                      obj.vertices[fv[i][0] - 1],
                      obj.vertices[fv[i][1] - 1],
                      obj.vertices[fv[i][2] - 1],
                };

                Vector2[] screenPos = new Vector2[3]
                {
                    WorldPos2ScreenPos(worldPos[0]),
                    WorldPos2ScreenPos(worldPos[1]),
                    WorldPos2ScreenPos(worldPos[2]),
                };

                Vector3 normal = Vector3.Cross(worldPos[2] - worldPos[0], worldPos[1] - worldPos[0]);
                normal.Normalize();
                Vector3 lightDir = Quaternion.Euler(SoftRenderer.light.rot.x, SoftRenderer.light.rot.y, SoftRenderer.light.rot.z) * Vector3.forward;
                lightDir.Normalize();
                float intensity = Vector3.Dot(lightDir, normal);
                Debug.Log(i == 0, intensity);

                if (intensity > 0)
                    DrawTriangle(texture, screenPos[0], screenPos[1], screenPos[2], new Color(intensity, intensity, intensity, 1));
            }
        }

        public static void DrawTriangle(Texture2D texture, Vector2 pos0, Vector2 pos1, Vector2 pos2, Color color)
        {
            if (pos0.y == pos1.y && pos1.y == pos2.y)
            {
                Debug.Log("============");
                return;
            }
            if (pos0.y > pos1.y)
                Swap(ref pos0, ref pos1);
            if (pos0.y > pos2.y)
                Swap(ref pos0, ref pos2);
            if (pos1.y > pos2.y)
                Swap(ref pos1, ref pos2);


            if (pos0.y == pos1.y)
            {
                if (pos0.x > pos1.x)
                    Swap(ref pos0, ref pos1);
                DrawTriangleInternal(texture, pos0, pos1, pos2, color);
            }
            else if (pos1.y == pos2.y)
            {
                DrawTriangleInternal(texture, pos1, pos2, pos0, color);
            }
            else
            {
                float CulculateX(float x1, float x2, float y1, float y2, float y)
                {
                    if (y2 == y1)
                    {
                        throw new Exception($"不能为直线,x1:{x1},x2:{x2},y1:{y1},y2:{y2}");
                    }
                    if (x2 == x1)
                        return x1;
                    float x = (x2 - x1) / (y2 - y1) * (y - y1) + x1;
                    return x;
                }
                //拆分两个三角形
                int totalHeight = (int)(pos2.y - pos0.y);
                Vector2 centerPos = new Vector2(CulculateX(pos0.x, pos2.x, pos0.y, pos2.y, pos1.y), pos1.y);

                //上三角形
                Vector2 pos2_t = pos2;
                Vector2 pos1_t = centerPos.x > pos1.x ? centerPos : pos1;
                Vector2 pos0_t = centerPos.x > pos1.x ? pos1 : centerPos;
                DrawTriangleInternal(texture, pos0_t, pos1_t, pos2_t, color);

                //下三角形，需要往下一格
                Vector2 pos2_b = pos0;//最低点
                int y = (int)pos1.y - 1;
                Vector2 pos0_b = new Vector2(CulculateX(pos0_t.x, pos2_b.x, pos0_t.y, pos2_b.y, y), y);
                Vector2 pos1_b = new Vector2(CulculateX(pos1_t.x, pos2_b.x, pos1_t.y, pos2_b.y, y), y);
                if (pos0_b.x > pos1_b.x)
                    Swap(ref pos0_b, ref pos1_b);
                DrawTriangleInternal(texture, pos0_b, pos1_b, pos2_b, color);
            }
        }

        public static void DrawTriangleInternal(Texture2D texture, Vector2 pos0, Vector2 pos1, Vector2 pos2, Color color)
        {
            if (pos0.y == pos1.y && pos1.y == pos2.y)
            {
                DrawLine(texture, (int)Math.Min(Math.Min(pos0.x, pos1.x), pos2.x), (int)pos0.y, (int)Math.Max(Math.Max(pos0.x, pos1.x), pos2.x), (int)pos0.y, color);
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
                    DrawPixel(texture, j, (int)pos0.y + i, color);
                }
            }
        }

        public static void DrawBottomTriangle(Texture2D texture, Vector2 pos0, Vector2 pos1, Vector2 pos2, Color color)
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
                    DrawPixel(texture, j, (int)pos0.y + i, color);
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