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
            for (int i = 0; i < len ; i++)
            {
                Vector2 p1 = WorldPos2ScreenPos(obj.vertices[fv[i][0] - 1]);
                Vector2 p2 = WorldPos2ScreenPos(obj.vertices[fv[i][1] - 1]);
                Vector2 p3 = WorldPos2ScreenPos(obj.vertices[fv[i][2] - 1]);

                DrawLine(texture, (int)p1.x, (int)p1.y, (int)p2.x, (int)p2.y, Color.white);
                DrawLine(texture, (int)p2.x, (int)p2.y, (int)p3.x, (int)p3.y, Color.white);
                DrawLine(texture, (int)p3.x, (int)p3.y, (int)p1.x, (int)p1.y, Color.white);
            }

        }

		public static void DrawTriangle(Vector2 pos1,Vector2 pos2,Vector2 pos3 ){
			
			if(pos1.y > pos2.y){
				Swap(ref pos1,  ref pos2);
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
		
		private void Swap(ref Vector2 a,ref Vector2 b){
			Vector2 t = a;
			a = b;
			a = t;
		}
    }
}