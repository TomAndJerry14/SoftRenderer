using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Light = SoftRenderer.Light;

namespace SoftRenderer
{
    public class SoftRenderer : MonoBehaviour
    {
        public static SoftRenderer Instance;

        public static int width = 2340;
        public static int height = 2340;

        public RawImage UI;

        public static Light light = new Light();
        public GameObject rotLgiht;

        public static Model obj = null;

        public Texture2D diffuse;
        public Texture2D normal;

        public float cameraX;
        public float cameraY;
        public float cameraZ;

        public float LightX;
        public float LightY;
        public float LightZ;

        public float centerX = 0f;
        public float centerY = 0f;
        public float centerZ = 0f;

        public bool shadow = true;
        public bool model = true;

        public Camera cam;

        public void Start()
        {
            Instance = this;
            

            cam = new Camera();
            UI.texture = cam.renderer;

            string path = Application.dataPath + "/Model/head.obj";
            obj = LoadObj(path);

            cam.Start();
        }

        float time = 2.5f;
        public void Update()
        {
            time -= Time.deltaTime;
            if (time < 0)
            {
                cam.Update();
                time = 2.5f;
            }
        }


        private static Model LoadObj(string path)
        {
            Model obj = new Model(File.ReadAllLines(path), Instance.diffuse, Instance.normal);
            obj.shader = new MyShader(obj, Instance.cam);
            obj.shadowShader = new DepthShader(obj, Instance.cam);
            return obj;
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(new Vector2(0, 800), new Vector2(100, 50)), "äÖÈ¾"))
            {
                int width = SoftRenderer.width;
                int height = SoftRenderer.height;
                cam.zBuffer = new float[width * height];
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        cam.zBuffer[j * width + i] = float.MinValue;
                        cam.depth[j * width + i] = float.MinValue;
                    }
                }
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        MyGL.DrawPixel(cam.renderer, i, j, new Color(0.5f, 0.5f, 0.5f, 0.5f));
                    }
                }

                cam.DrawModel(obj);
            }
        }
    }
}