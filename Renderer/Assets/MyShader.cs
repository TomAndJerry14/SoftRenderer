using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoftRenderer
{
    internal class MyShader : IShader
    {
        Model obj;
        //public Vector3 varying_intensity; // written by vertex shader, read by fragment shader
        public Matrix4x4 varying_uv;
        public Matrix4x4 varying_normal;
        public Matrix4x4 varying_tri;
        public Matrix4x4 ndc_tri;
        public Matrix4x4 varying_world;

        Camera cam;


        public MyShader(Model obj, Camera cam)
        {
            this.obj = obj;
            this.cam = cam;
        }



        public Vector4 vertex(int faceIndex, int vertexIndex)
        {
            varying_uv.SetColumn(vertexIndex, obj.uv(faceIndex, vertexIndex));
            var normal = obj.vertex_normals[obj.face_normal_vertices[faceIndex][vertexIndex] - 1];
            //法线要经过投影变换才能切换成切线空间，而我们需要的就是切线空间下的法线
            varying_normal.SetColumn(vertexIndex, (cam.Projection * cam.ModelView).inverse.transpose * new Vector4(normal.x, normal.y, normal.z, 0));

            Vector3 vertex = obj.vertex(faceIndex, vertexIndex);

            //世界坐标转化到摄像机坐标
            Vector4 gl_Vertex = cam.Viewport * this.cam.Projection * this.cam.ModelView * new Vector4(vertex.x, vertex.y, vertex.z, 1);
            gl_Vertex = gl_Vertex / gl_Vertex[3];
            varying_tri.SetColumn(vertexIndex, gl_Vertex);
            ndc_tri.SetColumn(vertexIndex, new Vector3(gl_Vertex.x, gl_Vertex.y, gl_Vertex.z));

            return gl_Vertex; // transform it to screen coordinates
        }

        public bool fragment(Vector3 bar, out Color color, Texture2D output)
        {
            Vector2 uv = varying_uv * bar;
            color = tex2D(obj.diffuseMap, uv);

            Vector3 bn = (varying_normal * bar).normalized;

            //var ind = Vector3.Dot(bn, -SoftRenderer.light.dir);
            //color.r *= ind;
            //color.g *= ind;
            //color.b *= ind;

            #region 切线空间但不知道为什么没生效
            //Matrix4x4 A = Matrix4x4.identity;
            //A[3, 3] = 0;
            //A.SetColumn(0, ndc_tri.GetColumn(1) - ndc_tri.GetColumn(0));
            //A.SetColumn(1, ndc_tri.GetColumn(2) - ndc_tri.GetColumn(0));
            //A.SetColumn(2, bn);

            //Matrix4x4 AI = A.inverse;

            //Vector3 i = AI * new Vector3(varying_uv[0, 1] - varying_uv[0, 0], varying_uv[0, 2] - varying_uv[0, 0], 0);
            //Vector3 j = AI * new Vector3(varying_uv[1, 1] - varying_uv[1, 0], varying_uv[1, 2] - varying_uv[1, 0], 0);

            //Matrix4x4 B = Matrix4x4.identity;
            //B.SetColumn(0, i.normalized);
            //B.SetColumn(1, j.normalized);
            //B.SetColumn(2, bn);

            #endregion

            //摄像机下的坐标
            Vector4 crood = varying_tri * bar;
            //转回世界坐标
            Vector4 _crood = (cam.Viewport * cam.Projection * cam.ModelView).inverse * crood;
            //世界坐标转到阴影坐标
            Vector4 sb_p = cam.Viewport * cam.ProjectionForLight * cam.ModelViewForLight * _crood;
            sb_p = sb_p / sb_p[3];
            int idx = (int)sb_p[0] + (int)sb_p[1] * output.width;

            if (sb_p[2] < cam.depth[idx])
            {
                //在阴影里
                color.r *= 0.3f;
                color.g *= 0.3f;
                color.b *= 0.3f;
                //color = Color.black;
            }


            //高光 冯着色
            //Vector3 n = (B * obj.normal(uv));
            //Vector3 l = -SoftRenderer.light.dir.normalized;
            //Vector3 r = (n * (Vector3.Dot(n, l) * 2f) - l).normalized; //反射向量


            //float intensity = Math.Max(0f, Vector3.Dot(n, l)) * 0.5f;

            //color.r *= 0.5f + intensity;
            //color.g *= 0.5f + intensity;
            //color.b *= 0.5f + intensity;

            ////阴影
            //Vector4 v0 = new Vector4(varying_world[0, 0], varying_world[1, 0], varying_world[2, 0], 1);
            //Vector4 v1 = new Vector4(varying_world[0, 1], varying_world[1, 1], varying_world[2, 1], 1);
            //Vector4 v2 = new Vector4(varying_world[0, 2], varying_world[1, 2], varying_world[2, 2], 1);
            ////Debug.Log(v0);

            //v0 = uniform_Mshadow * v0 + new Vector4(SoftRenderer.width / 2, SoftRenderer.height / 2);
            //v1 = uniform_Mshadow * v1 + new Vector4(SoftRenderer.width / 2, SoftRenderer.height / 2);
            //v2 = uniform_Mshadow * v2;

            //float x = v0.x * bar.x + v1.x * bar.y + v2.x * bar.z;
            //float y = v0.y * bar.x + v1.y * bar.y + v2.y * bar.z;
            //float z = v0.z * bar.x + v1.z * bar.y + v2.z * bar.z;

            ////Debug.Log(x, y);
            ////Debug.Log(cam.depth[(int)(output.width * y + x)], z);
            //if (Math.Abs(cam.depth[(int)(output.width * y + x)] - z) < 10)
            //{
            //    //找到阴影的点
            //    color = new Color(color.r * 0.3f, color.g * 0.3f, color.b * 0.3f, 1);
            //    color = Color.red;
            //}
            //else
            //{
            //}

            return false;
        }

        public Color tex2D(Texture2D map, Vector2 uv)
        {
            return map.GetPixel((int)(map.width * uv.x + 0.5f), (int)(map.height * uv.y + 0.5f));
        }



        public Color sample2D(Texture2D texture, Vector2 uv)
        {
            return Color.white;
        }
    }
}
