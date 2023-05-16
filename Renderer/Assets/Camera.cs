using System;
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

        //将(-1,1)(-1,1)(-1,1)的立方体映射到屏幕(x,x+w)(y,y+h)(0,d)立方体中
        public Matrix4x4 ViewPort = CreateViewPort(SoftRenderer.width / 8, SoftRenderer.height / 8, SoftRenderer.width / 3, SoftRenderer.height / 3);

        //视图视角
        public Matrix4x4 ModelView;

        public float[,] zBuffer;
        public Texture2D renderer;
        private Draw draw;

        Vector3 eye = new Vector3(SoftRenderer.Instance.cameraX, SoftRenderer.Instance.cameraY, SoftRenderer.Instance.cameraZ);
        Vector3 center = new Vector3(SoftRenderer.Instance.centerX, SoftRenderer.Instance.centerY, SoftRenderer.Instance.centerZ);
        Vector3 up = Vector3.up;

        public Camera()
        {
            renderer = new Texture2D(SoftRenderer.width, SoftRenderer.height);
            Projection = CreateProjection(SoftRenderer.Instance.cameraZ);
            SetLookAt(this.eye, this.center, this.up);

            draw = new Draw(this);

            Start();
        }

        static Matrix4x4 CreateProjection(float z)
        {
            Matrix4x4 matrix = Matrix4x4.identity;
            matrix[3, 2] = -1 / z;

            Debug.Log("Projection\n", matrix);
            return matrix;
        }

        static Matrix4x4 CreateViewPort(int x, int y, int w, int h)
        {
            return new Matrix4x4(
                new Vector4(w / 2f, 0, 0, x + w / 2f),
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
            Matrix4x4 matrixInViewSpace = ViewPort * Projection * ModelView * matrixOfObjInWorld;
            Debug.Log("matrixInViewSpace\n", matrixInViewSpace);
            Vector3 position = new Vector3();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    position[i] += matrixInViewSpace[i, j];
                }
            }
            Debug.Log(position);
            return position;
        }

        void SetLookAt(Vector3 _eye, Vector3 _center, Vector3 _up)
        {
            Vector3 z = (_eye - _center).normalized;
            Vector3 x = Vector3.Cross(_up, z).normalized;
            Vector3 y = Vector3.Cross(z, x).normalized;
            Debug.Log(x);
            Debug.Log(y);
            Debug.Log(z);

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
            Debug.Log("ModelView\n", ModelView);
        }

        public void Start()
        {
            int width = SoftRenderer.width;
            int height = SoftRenderer.height;
            zBuffer = new float[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    zBuffer[i, j] = float.MinValue;
                }
            }


            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    draw.DrawPixel(i, j, Color.black);
                }
            }

            draw.DrawLine(13, 20, 80, 40, Color.white);
            draw.DrawLine(20, 13, 40, 80, Color.red);
            draw.DrawLine(80, 40, 13, 20, Color.white);

            draw.DrawObj(SoftRenderer.obj);

            //Debug.Log(Matrix2CameraLocal(Vector3.one), "one");
            //Debug.Log(Matrix2CameraLocal(Vector3.right), "right");
            //Debug.Log(Matrix2CameraLocal(-Vector3.right), "-right");
            //Debug.Log(Matrix2CameraLocal(Vector3.up), "up");
            //Debug.Log(Matrix2CameraLocal(-Vector3.up), "-up");
            //Debug.Log(Matrix2CameraLocal(Vector3.forward), "forward");
            //Debug.Log(Matrix2CameraLocal(-Vector3.forward), "-forward");
            //Debug.Log(Matrix2CameraLocal(Vector3.zero), "zero");

            renderer.Apply();
        }


        public void Update()
        {
            if (SoftRenderer.Instance.cameraX != eye.x ||
                SoftRenderer.Instance.cameraY != eye.y ||
                SoftRenderer.Instance.cameraZ != eye.z ||
                SoftRenderer.Instance.centerX != center.x ||
                SoftRenderer.Instance.centerY != center.y ||
                SoftRenderer.Instance.centerZ != center.z
                )
            {
                int width = SoftRenderer.width;
                int height = SoftRenderer.height;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        zBuffer[i, j] = float.MinValue;
                    }
                }
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        draw.DrawPixel(i, j, Color.black);
                    }
                }
                Projection = CreateProjection(SoftRenderer.Instance.cameraZ);
                eye.x = SoftRenderer.Instance.cameraX;
                eye.y = SoftRenderer.Instance.cameraY;
                eye.z = SoftRenderer.Instance.cameraZ;
                SoftRenderer.Instance.centerX = center.x;
                SoftRenderer.Instance.centerY = center.y;
                SoftRenderer.Instance.centerZ = center.z;
                SetLookAt(eye, center, up);

                draw.DrawObj(SoftRenderer.obj);
                //Debug.Log(Matrix2CameraLocal(Vector3.one), "one");
                //Debug.Log(Matrix2CameraLocal(Vector3.right), "right");
                //Debug.Log(Matrix2CameraLocal(-Vector3.right), "-right");
                //Debug.Log(Matrix2CameraLocal(Vector3.up), "up");
                //Debug.Log(Matrix2CameraLocal(-Vector3.up), "-up");
                //Debug.Log(Matrix2CameraLocal(Vector3.zero), "zero");
                //Debug.Log(Matrix2CameraLocal(Vector3.forward), "forward");
                //Debug.Log(Matrix2CameraLocal(-Vector3.forward), "-forward");

                renderer.Apply();
            }
        }
    }
}
