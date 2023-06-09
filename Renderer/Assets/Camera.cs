using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoftRenderer
{
    public class Camera
    {
        //public Transform Transform { get; set; }

        //将摄像机坐标系的物体点转化到齐次坐标中
        public Matrix4x4 Projection { get; private set; }
        public Matrix4x4 ProjectionForLight { get; private set; }

        //将(-1,1)(-1,1)(-1,1)的立方体映射到屏幕(x,x+w)(y,y+h)(0,d)立方体中
        public Matrix4x4 Viewport { get; private set; }

        //视图视角
        public Matrix4x4 ModelView;

        public Matrix4x4 ModelViewForLight;

        public float[] zBuffer;
        public float[] depth;
        public Texture2D renderer;
        public Texture2D depthMap;

        Vector3 eye = new Vector3(SoftRenderer.Instance.cameraX, SoftRenderer.Instance.cameraY, SoftRenderer.Instance.cameraZ);
        Vector3 shadow = new Vector3(SoftRenderer.Instance.LightX, SoftRenderer.Instance.LightY, SoftRenderer.Instance.LightZ);
        Vector3 center = new Vector3(SoftRenderer.Instance.centerX, SoftRenderer.Instance.centerY, SoftRenderer.Instance.centerZ);
        Vector3 up = Vector3.up;

        private WaitForFixedUpdate wait = new WaitForFixedUpdate();

        public Camera()
        {
            renderer = new Texture2D(SoftRenderer.width, SoftRenderer.height);
            depthMap = new Texture2D(SoftRenderer.width, SoftRenderer.height);
            Projection = CreateProjection(SoftRenderer.Instance.cameraZ);
            Viewport = CreateViewPort(SoftRenderer.width / 8, SoftRenderer.height / 8, SoftRenderer.width / 3, SoftRenderer.height / 3);

            SetLookAt(this.shadow, this.center, this.up);
            ModelViewForLight = ModelView;
            ProjectionForLight = CreateProjection(0);
            SetLookAt(this.eye, this.center, this.up);
        }

        static Matrix4x4 CreateProjection(float z)
        {
            Matrix4x4 matrix = Matrix4x4.identity;
            matrix[3, 2] = z == 0? 0:-1 / z;

            return matrix;
        }

        static Matrix4x4 CreateViewPort(int x, int y, int w, int h)
        {
            return new Matrix4x4(
                new Vector4(w / 2f, 0, 0, x + w / 2),
                new Vector4(0, h / 2f, 0, y + h / 2f),
                new Vector4(0, 0, 1, 0),
                new Vector4(0, 0, 0, 1));
        }

        //顶点世界坐标转换到齐次空间
        public static Matrix4x4 Vertex2Matrix(Vector3 point)
        {
            return new Matrix4x4(
                new Vector4(point.x, 0, 0, 0),
                new Vector4(0, point.y, 0, 0),
                new Vector4(0, 0, point.z, 0),
                new Vector4(0, 0, 0, 1)
                );
        }

        //计算目标矩阵在视图里的点
        public Vector3 Matrix2CameraLocal(Vector3 worldPos)
        {
            Matrix4x4 matrixOfObjInWorld = Vertex2Matrix(worldPos);
            Matrix4x4 matrixInViewSpace = Viewport * Projection * ModelView * matrixOfObjInWorld;
            Vector3 position = new Vector3();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    position[i] += matrixInViewSpace[i, j];
                }
            }
            return position;
        }

        public void SetLookAt(Vector3 _eye, Vector3 _center, Vector3 _up)
        {
            Vector3 z = (_eye - _center).normalized;
            Vector3 x = Vector3.Cross(_up, z).normalized;
            Vector3 y = Vector3.Cross(z, x).normalized;

            Matrix4x4 Minv = Matrix4x4.identity;
            Matrix4x4 Tr = Matrix4x4.identity;
            for (int i = 0; i < 3; i++)
            {
                Minv[0, i] = x[i];
                Minv[1, i] = y[i];
                Minv[2, i] = z[i];
                Tr[i, 3] = -_eye[i];
            }
            ModelView = Minv * Tr;
        }

        public void DrawModel(Model obj)
        {
            SoftRenderer.Instance.StartCoroutine(DrawObject(obj));
        }

        IEnumerator DrawObject(Model obj)
        {
            var fv = obj.face_vertices;
            int len = fv.Length;
            if (SoftRenderer.Instance.shadow)
            {

                for (int i = 0; i < len; i++)
                {
                    Vector3[] screen_coords = new Vector3[3];
                    for (int j = 0; j < 3; j++)
                    {
                        screen_coords[j] = obj.shadowShader.vertex(i, j);
                    }

                    MyGL.Triangle(screen_coords, obj.shadowShader, this.renderer, ref depth);

                    if (i % 200 == 0)
                    {
                        renderer.Apply();
                        yield return wait;
                    }

                }
            }

            Vector3 eye = new Vector3(SoftRenderer.Instance.cameraX, SoftRenderer.Instance.cameraY, SoftRenderer.Instance.cameraZ);
            Vector3 center = new Vector3(SoftRenderer.Instance.centerX, SoftRenderer.Instance.centerY, SoftRenderer.Instance.centerZ);
            Vector3 up = Vector3.up;

            SetLookAt(eye, center, up);

            //SetLookAt(new Vector3(0, 0, 1), center, up);
            if (SoftRenderer.Instance.model)
            {
                for (int i = 0; i < len; i++)
                {
                    Vector3[] screen_coords = new Vector3[3];
                    for (int j = 0; j < 3; j++)
                    {
                        screen_coords[j] = obj.shader.vertex(i, j);
                    }

                    MyGL.Triangle(screen_coords, obj.shader, this.renderer, ref zBuffer);

                    if (i % 200 == 0)
                    {
                        renderer.Apply();
                        yield return wait;
                    }
                }
            }
           
        }

        public void Start()
        {
            int width = SoftRenderer.width;
            int height = SoftRenderer.height;
            zBuffer = new float[width * height];
            depth = new float[width * height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    zBuffer[j * width + i] = float.MinValue;
                    depth[j * width + i] = float.MinValue;
                }
            }


            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    MyGL.DrawPixel(this.renderer, i, j, new Color(0.5f, 0.5f, 0.5f, 0.5f));
                }
            }

            DrawModel(SoftRenderer.obj);

            renderer.Apply();
        }


        int x = 3;
        int y = 1;
        int z = 3;
        public void Update()
        {

            //SetLookAt(new Vector3((float)Math.Cos(x++ / 2 * 90), y, (float)Math.Sin(z++ / 2 * 90)), Vector3.zero, Vector3.up);

            //int width = SoftRenderer.width;
            //int height = SoftRenderer.height;
            //zBuffer = new float[width * height];
            //for (int i = 0; i < width; i++)
            //{
            //    for (int j = 0; j < height; j++)
            //    {
            //        zBuffer[j * width + i] = float.MinValue;
            //    }
            //}


            //for (int i = 0; i < width; i++)
            //{
            //    for (int j = 0; j < height; j++)
            //    {
            //        MyGL.DrawPixel(this.renderer, i, j, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            //    }
            //}

            //DrawModel(SoftRenderer.obj);

            //renderer.Apply();
        }
    }
}
