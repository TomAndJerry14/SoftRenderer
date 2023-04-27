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
                Vector2 p1 = WorldPos2ScreenPos(obj.vertices[fv[i][0] - 1]);
                Vector2 p2 = WorldPos2ScreenPos(obj.vertices[fv[i][1] - 1]);
                Vector2 p3 = WorldPos2ScreenPos(obj.vertices[fv[i][2] - 1]);

                //DrawLine(texture, (int)p1.x, (int)p1.y, (int)p2.x, (int)p2.y, Color.white);
                //DrawLine(texture, (int)p2.x, (int)p2.y, (int)p3.x, (int)p3.y, Color.white);
                //DrawLine(texture, (int)p3.x, (int)p3.y, (int)p1.x, (int)p1.y, Color.white);
                //DrawTriangle(texture, p1, p2, p3, Color.white);
            }

        }

        public static void DrawTriangle(Texture2D texture, Vector2 pos0, Vector2 pos1, Vector2 pos2, Color color)
        {
            if (pos0.y == pos1.y && pos1.y == pos2.y)
            {
                return;
            }
            //else if (pos0.y == pos1.y || pos1.y == pos2.y || pos0.y == pos2.y)
            //{

            //}
            //else
            //{
            //    if (pos0.y > pos1.y)
            //        Swap(ref pos0, ref pos1);
            //    if (pos0.y > pos2.y)
            //        Swap(ref pos0, ref pos2);
            //    if (pos1.x > pos2.x)
            //        Swap(ref pos1, ref pos2);

            //    int totalHeight = (int)pos2.y - (int)pos0.y;
            //    for (int i = 0; i < totalHeight; i++)
            //    {
            //        bool bottom = i >= pos1.y - pos0.y;
            //        float halfHegiht = bottom ? pos1.y - pos0.y : pos2.y - pos1.y;
            //        float k = (float)i / totalHeight;
            //        float k1 = (float)i / halfHegiht;
            //        Vector2 A = pos0 + (pos2 - pos0) * k;
            //        Debug.Log(i);
            //        Debug.Log(halfHegiht);
            //        Debug.Log(k1);
            //        Debug.Log("tt", ((pos1 - pos0) * k1).x);
            //        Vector2 B = bottom ? pos0 + (pos1 - pos0) * k1 : pos1 + (pos2 - pos1) * k1;
            //        Debug.Log(A.x, B.x);
            //        if (A.x > B.x)
            //        {
            //            Swap(ref A, ref B);
            //        }
            //        for (int j = (int)A.x; j <= (int)B.x; j++)
            //        {
            //            Debug.Log(j, (int)pos0.y + i);
            //            DrawPixel(texture, j, (int)pos0.y + i, color);
            //        }
            //        Debug.Log("==========");
            //    }
            //}


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
                //拆分两个三角形
                int totalHeight = (int)(pos2.y - pos0.y);
                Vector2 centerPos = new Vector2((pos2.x - pos0.x) * (pos1.y / totalHeight), pos1.y);
                
                //往下一格
                Vector2 pos0_b = new Vector2();
                Vector2 pos1_b = new Vector2();
                Vector2 pos2_b = pos0;
            }
        }

        public static void DrawTriangleInternal(Texture2D texture, Vector2 pos0, Vector2 pos1, Vector2 pos2, Color color)
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
                    Debug.Log(j, (int)pos0.y + i);
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
                    Debug.Log(j, (int)pos0.y + i);
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
            a = t;
        }
    }
}