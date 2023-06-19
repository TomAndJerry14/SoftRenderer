using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoftRenderer
{
    internal class DepthShader : IShader
    {
        Model obj;
        public Matrix4x4 varying_uv;
        public Matrix4x4 varying_normal;
        public Matrix4x4 varying_tri;
        public Matrix4x4 ndc_tri;

        private Camera cam;

        public DepthShader(Model obj, Camera cam)
        {
            this.obj = obj;
            this.cam = cam;
        }
        public Vector4 vertex(int faceIndex, int vertexIndex)
        {
            varying_uv.SetColumn(vertexIndex, obj.uv(faceIndex, vertexIndex));
            Vector3 vertex = obj.vertex(faceIndex, vertexIndex);
            Vector4 gl_Vertex = cam.Viewport * cam.ProjectionForLight * cam.ModelViewForLight * new Vector4(vertex.x, vertex.y, vertex.z, 1);
            //gl_Vertex /= gl_Vertex[3];
            varying_tri.SetColumn(vertexIndex, gl_Vertex);
            return gl_Vertex; // transform it to screen coordinates
        }

        public bool fragment(Vector3 bar, out Color color, Texture2D output)
        {

            float z = varying_tri[2, 0] * bar.x + varying_tri[2, 1] * bar.y + varying_tri[2, 2] * bar.z;
            color = new Color(z * 5, z * 5, z * 5, 1);
            //color = Color.white;


            return !SoftRenderer.Instance.shadow || SoftRenderer.Instance.model;
        }
    }
}
