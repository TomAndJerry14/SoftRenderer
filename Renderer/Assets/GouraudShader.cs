using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoftRenderer
{
    internal class GouraudShader : IShader
    {
        Model obj;
        //public Vector3 varying_intensity; // written by vertex shader, read by fragment shader
        public Matrix4x4 varying_uv;
        public Matrix4x4 varying_normal;
        public Matrix4x4 varying_tri;
        public Matrix4x4 ndc_tri;
        public Matrix4x4 varying_world;


        public Matrix4x4 uniform_M;
        public Matrix4x4 uniform_MIT;
        public Matrix4x4 uniform_Mshadow;

        Camera cam;


        public GouraudShader(Model obj, Camera cam)
        {
            this.obj = obj;
            this.cam = cam;

            uniform_M = this.cam.Projection * this.cam.ModelView;
            uniform_MIT = uniform_M.inverse.transpose;

        }



        public Vector4 vertex(int faceIndex, int vertexIndex)
        {
            varying_uv.SetColumn(vertexIndex, obj.uv(faceIndex, vertexIndex));
            var normal = obj.vertex_normals[obj.face_normal_vertices[faceIndex][vertexIndex] - 1];
            //法线要经过投影变换才能切换成切线空间，而我们需要的就是切线空间下的法线
            varying_normal.SetColumn(vertexIndex, (cam.Projection * cam.ModelView).inverse.transpose * new Vector4(normal.x, normal.y, normal.z, 0));

            Vector3 vertex = obj.vertex(faceIndex, vertexIndex);
            varying_world.SetColumn(vertexIndex, vertex);
            Vector4 gl_Vertex = cam.Viewport * cam.Projection * cam.ModelView * new Vector4(vertex.x, vertex.y, vertex.z, 1);
            varying_tri.SetColumn(vertexIndex, gl_Vertex);
            ndc_tri.SetColumn(vertexIndex, new Vector3(gl_Vertex.x, gl_Vertex.y, gl_Vertex.z));


            return gl_Vertex; // transform it to screen coordinates
        }

        public bool fragment(Vector3 bar, out Color color, Texture2D output)
        {
            Vector2 uv = varying_uv * bar;
            color = tex2D(obj.diffuseMap, uv);

            Vector3 bn = (varying_normal * bar).normalized;

            Matrix4x4 A = Matrix4x4.identity;
            A[3, 3] = 0;
            A.SetColumn(0, ndc_tri.GetColumn(1) - ndc_tri.GetColumn(0));
            A.SetColumn(1, ndc_tri.GetColumn(2) - ndc_tri.GetColumn(0));
            A.SetColumn(2, bn);

            Matrix4x4 AI = A.inverse;

            Vector3 i = AI * new Vector3(varying_uv[0, 1] - varying_uv[0, 0], varying_uv[0, 2] - varying_uv[0, 0], 0);
            Vector3 j = AI * new Vector3(varying_uv[1, 1] - varying_uv[1, 0], varying_uv[1, 2] - varying_uv[1, 0], 0);

            Matrix4x4 B = Matrix4x4.identity;
            B.SetColumn(0, i.normalized);
            B.SetColumn(1, j.normalized);
            B.SetColumn(2, bn);

            Vector3 n = (B * obj.normal(uv));
            Vector3 l = -SoftRenderer.light.dir.normalized;
            //Vector3 r = (n * (Vector3.Dot(n, l) * 2f) - l).normalized; //反射向量


            float intensity = Math.Max(0f, Vector3.Dot(n, l)) * 0.5f;

            color.r *= 0.5f + intensity;
            color.g *= 0.5f + intensity;
            color.b *= 0.5f + intensity;

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
