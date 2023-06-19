using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SoftRenderer
{
    public class Camera1
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

        public NativeArray<float> zBuffer;
        public NativeArray<float> depth;
        public Texture2D renderer;
        public Texture2D depthMap;

        Vector3 eye = new Vector3(SoftRenderer.Instance.cameraX, SoftRenderer.Instance.cameraY, SoftRenderer.Instance.cameraZ);
        Vector3 shadow = new Vector3(SoftRenderer.Instance.LightX, SoftRenderer.Instance.LightY, SoftRenderer.Instance.LightZ);
        Vector3 center = new Vector3(SoftRenderer.Instance.centerX, SoftRenderer.Instance.centerY, SoftRenderer.Instance.centerZ);
        Vector3 up = Vector3.up;

        private WaitForFixedUpdate wait = new WaitForFixedUpdate();

        public static Camera1 main;

        public Camera1()
        {
            main = this;
            renderer = new Texture2D(SoftRenderer.width, SoftRenderer.height);
            depthMap = new Texture2D(SoftRenderer.width, SoftRenderer.height);

            InitData();
        }


        public void InitData()
        {
            eye = new Vector3(SoftRenderer.Instance.cameraX, SoftRenderer.Instance.cameraY, SoftRenderer.Instance.cameraZ);
            shadow = new Vector3(SoftRenderer.Instance.LightX, SoftRenderer.Instance.LightY, SoftRenderer.Instance.LightZ);
            center = new Vector3(SoftRenderer.Instance.centerX, SoftRenderer.Instance.centerY, SoftRenderer.Instance.centerZ);

            Projection = CreateProjection(SoftRenderer.Instance.cameraZ);
            Viewport = CreateViewPort(SoftRenderer.width / 8, SoftRenderer.height / 8, SoftRenderer.width * 2 / 3, SoftRenderer.height * 2 / 3);

            SetLookAt(this.shadow, this.center, this.up);
            ModelViewForLight = ModelView;
            ProjectionForLight = CreateProjection(0);
            SetLookAt(this.eye, this.center, this.up);
        }

        static Matrix4x4 CreateProjection(float z)
        {
            Matrix4x4 matrix = Matrix4x4.identity;
            matrix[3, 2] = z == 0 ? 0 : -1 / z;

            return matrix;
        }

        static Matrix4x4 CreateViewPort(int x, int y, int w, int h)
        {
            var matrix = new Matrix4x4(
            new Vector4(w / 2f, 0, 0, x + w / 2),
            new Vector4(0, h / 2f, 0, y + h / 2f),
            new Vector4(0, 0, 1, 0),
            new Vector4(0, 0, 0, 1));
            return matrix.transpose;
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
            var fv = obj.face_vertices;
            int len = fv.Length;

            var data = new JobRenderObjectData();
            data.points = new NativeArray<Vector3>(obj.vertices.Length, Allocator.Persistent);
            data.points.CopyFrom(obj.vertices);

            data.normals = new NativeArray<Vector3>(obj.vertex_normals.Length, Allocator.Persistent);
            data.normals.CopyFrom(obj.vertex_normals);

            data.uvs = new NativeArray<Vector2>(obj.texture_vertices.Length, Allocator.Persistent);
            data.normals.CopyFrom(obj.texture_vertices);

            data.triangleVertexData = new NativeArray<Vector3Int>(obj.face_vertices.Length, Allocator.Persistent);
            for (int i = 0; i < obj.face_vertices.Length; i++)
            {
                data.triangleVertexData[i] = new Vector3Int(obj.face_vertices[i][0], obj.face_vertices[i][1], obj.face_vertices[i][2]);
            }
            data.triangleUVData = new NativeArray<Vector3Int>(obj.face_normal_vertices.Length, Allocator.Persistent);
            for (int i = 0; i < obj.face_normal_vertices.Length; i++)
            {
                data.triangleUVData[i] = new Vector3Int(obj.face_normal_vertices[i][0], obj.face_normal_vertices[i][1], obj.face_normal_vertices[i][2]);
            }

            if (SoftRenderer.Instance.shadow)
            {
                NativeArray<VSOutBuf> vSOutBufs = new NativeArray<VSOutBuf>(len, Allocator.Temp);
                VertexShadingJob vsJob = new VertexShadingJob();
                vsJob.points = data.points;
                vsJob.normals = data.normals;
                vsJob.uvs = data.uvs;
                vsJob.triangleVertexData = data.triangleVertexData;
                vsJob.triangleUVData = data.triangleUVData;
;
                vsJob.result = vSOutBufs;
                JobHandle vsHandle = vsJob.Schedule(len, 3);

            }
            if (SoftRenderer.Instance.model)
            {

            }
            renderer.Apply();
            Debug.Log("finish");
        }
        public void Start()
        {
            int width = SoftRenderer.width;
            int height = SoftRenderer.height;
            zBuffer = new NativeArray<float>(width * height, Allocator.Persistent);
            depth = new NativeArray<float>(width * height, Allocator.Persistent);
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
    }
}
