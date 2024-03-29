﻿using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;


namespace SoftRenderer
{
    //internal class Draw
    //{
    //    public static Color[,] buffers = new Color[SoftRenderer.width, SoftRenderer.height];

    //    private Camera camera;

    //    public Draw(Camera camera)
    //    {
    //        this.camera = camera;
    //        for (int i = 0; i < SoftRenderer.width; i++)
    //        {
    //            for (int j = 0; j < SoftRenderer.height; j++)
    //            {
    //                buffers[i, j] = Color.black;
    //            }
    //        }
    //    }

    //    public void DrawPixel(int x0, int y0, Color color)
    //    {
    //        camera.renderer.SetPixel(x0, y0, color);
    //    }

    //    public void DrawLine(int x0, int y0, int x1, int y1, Color color)
    //    {
    //        if (x0 > x1)
    //            DrawLineInternal(x1, y1, x0, y0, color);
    //        else
    //            DrawLineInternal(x0, y0, x1, y1, color);
    //    }

    //    private void DrawLineInternal(int x0, int y0, int x1, int y1, Color color)
    //    {
    //        bool steep = false;
    //        if (Mathf.Abs(x1 - x0) < Mathf.Abs(y1 - y0))
    //        {
    //            Swap(ref x0, ref y0);
    //            Swap(ref x1, ref y1);
    //            steep = true;
    //        }

    //        for (int i = 0; i <= x1 - x0; i++)
    //        {
    //            int x = x0 + i;
    //            float t = (float)(x - x0) / (x1 - x0);
    //            int y = (int)((y1 - y0) * t + y0);
    //            if (steep)
    //                DrawPixel(y, x, color);
    //            else
    //                DrawPixel(x, y, color);

    //        }
    //    }

    //    public void DrawObj(Object obj)
    //    {
    //        SoftRenderer.Instance.StartCoroutine(DrawObject(obj));
    //    }

    //    IEnumerator DrawObject(Object obj)
    //    {
    //        var fv = obj.face_vertices;
    //        var ftv = obj.face_texture_vertices;
    //        var fvn = obj.face_normal_vertices;
    //        int len = fv.Length;
    //        for (int i = 0; i < len; i++)
    //        {
    //            Vector3[] oriPos = new Vector3[3];
    //            for (int j = 0; j < 3; j++)
    //            {
    //                oriPos[j] = obj.vertices[fv[i][j] - 1];
    //            }
    //            Vector3[] worldPos = new Vector3[3]
    //            {
    //                  camera.Matrix2CameraLocal(oriPos[0]),
    //                  camera.Matrix2CameraLocal(oriPos[1]),
    //                  camera.Matrix2CameraLocal(oriPos[2]),
    //            };

    //            Vector3[] vts = new Vector3[3];
    //            Color[] uvColor = new Color[3];
    //            Vector3[] vns = new Vector3[3];
    //            for (int j = 0; j < 3; j++)
    //            {
    //                var temp = obj.texture_vertices[ftv[i][j] - 1];
    //                vts[j] = temp;
    //                var texture = SoftRenderer.Instance.texture;
    //                uvColor[j] = SoftRenderer.Instance.texture.GetPixel((int)(temp.x * texture.width), (int)(temp.y * texture.height));

    //                vns[j] = obj.vertex_normals[fvn[i][j] - 1];
    //            }


    //            for (int j = 0; j < 3; j++)
    //            {
    //                worldPos[j] += new Vector3(SoftRenderer.width / 2, SoftRenderer.height / 2, 0);
    //            }

    //            //if (intensity > 0)
    //            DrawTriangle(worldPos, vns, uvColor);
    //            //DrawTriangle( worldPos[0], worldPos[1], worldPos[2], new Color(intensity, intensity, intensity, 1));

    //            if (i % 100 == 0)
    //            {
    //                camera.renderer.Apply();
    //                yield return new WaitForEndOfFrame();
    //            }
    //        }
    //    }

    //    private Vector3 Barycentric(Vector3 pos0, Vector3 pos1, Vector3 pos2, Vector3 p)
    //    {
    //        Vector3[] s = new Vector3[3];
    //        for (int i = 0; i <= 2; i++)
    //        {
    //            s[i].x = pos2[i] - pos0[i];
    //            s[i].y = pos1[i] - pos0[i];
    //            s[i].z = pos0[i] - p[i];
    //        }
    //        Vector3 u = Vector3.Cross(s[0], s[1]);

    //        //叉积第三位
    //        if (Math.Abs(u.z) > 1e-2)
    //        {
    //            return new Vector3(1f - (u.x + u.y) / u.z, u.y / u.z, u.x / u.z);
    //        }
    //        return new Vector3(-1, 1, 1);
    //    }

    //    public void DrawTriangleInternal(Vector2 pos0, Vector2 pos1, Vector2 pos2, Color color)
    //    {
    //        if (pos0.y == pos1.y && pos1.y == pos2.y)
    //        {
    //            DrawLine((int)Math.Min(Math.Min(pos0.x, pos1.x), pos2.x), (int)pos0.y, (int)Math.Max(Math.Max(pos0.x, pos1.x), pos2.x), (int)pos0.y, color);
    //            Debug.Log((int)Math.Min(Math.Min(pos0.x, pos1.x), pos2.x), (int)pos0.y, (int)Math.Max(Math.Max(pos0.x, pos1.x), pos2.x), (int)pos0.y);
    //            return;
    //        }

    //        int totalHeight = (int)pos2.y - (int)pos0.y;
    //        bool up = pos2.y >= pos0.y;
    //        for (int i = 0; up ? i < totalHeight : i > totalHeight; i = (up ? i + 1 : i - 1))
    //        {
    //            var t = (float)i / totalHeight;
    //            Vector2 A = pos0 + (pos2 - pos0) * t;
    //            Vector2 B = pos1 + (pos2 - pos1) * t;
    //            for (int j = (int)A.x; j <= (int)B.x; j++)
    //            {
    //                DrawPixel(j, (int)pos0.y + i, color);
    //            }
    //        }
    //    }

    //    public void DrawBottomTriangle(Vector2 pos0, Vector2 pos1, Vector2 pos2, Color color)
    //    {
    //        if (pos0.y == pos1.y && pos1.y == pos2.y)
    //        {
    //            return;
    //        }

    //        int totalHeight = (int)pos2.y - (int)pos0.y;
    //        for (int i = 0; i < totalHeight; i++)
    //        {
    //            var t = (float)i / totalHeight;
    //            Vector2 A = pos0 + (pos2 - pos0) * t;
    //            Vector2 B = pos1 + (pos2 - pos1) * t;
    //            for (int j = (int)A.x; j <= (int)B.x; j++)
    //            {
    //                DrawPixel(j, (int)pos0.y + i, color);
    //            }
    //        }
    //    }

    //    public static Vector2 WorldPos2ScreenPos(Vector3 pos)
    //    {
    //        int x = (int)((pos.x + 1) * 400);
    //        int y = (int)((pos.y + 1) * 400);
    //        return new Vector2(x, y);
    //    }

    //    private static void Swap(ref int a, ref int b)
    //    {
    //        int t = a;
    //        a = b;
    //        b = t;
    //    }

    //    private static void Swap(ref Vector2 a, ref Vector2 b)
    //    {
    //        Vector2 t = a;
    //        a = b;
    //        b = t;
    //    }
    //}
}
