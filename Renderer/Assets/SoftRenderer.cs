using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Light = SoftRenderer.Light;

namespace SoftRenderer
{
    public class SoftRenderer : MonoBehaviour
    {
        public static int width = 2340;
        public static int height = 2340;

        public RawImage UI;
        public Texture2D renderer;

        public static Light light = new Light();
        public GameObject rotLgiht;

        Object obj = null;

        public static float[,] zBuffer;

        public void Start()
        {
            string path = Application.dataPath + "/head.obj";
            obj = LoadObj(path);
            renderer = new Texture2D(width, height);
            UI.texture = renderer;

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
                    Draw.DrawPixel(renderer, i, j, Color.black);
                }
            }




            Draw.DrawLine(renderer, 13, 20, 80, 40, Color.white);
            Draw.DrawLine(renderer, 20, 13, 40, 80, Color.red);
            Draw.DrawLine(renderer, 80, 40, 13, 20, Color.white);


            //Draw.DrawTriangle(renderer, new Vector2(10, 10), new Vector2(100, 10), new Vector2(50, 50), Color.red);
            //Draw.DrawTriangle(renderer, new Vector2(131, 90), new Vector2(20, 90), new Vector2(50, 50), Color.white);
            //Draw.DrawTriangle(renderer, new Vector2(200, 90), new Vector2(200, 200), new Vector2(150, 120), Color.yellow);

            //Draw.DrawTriangle(renderer, new Vector3[] { new Vector2(10, 10), new Vector2(100, 10), new Vector2(50, 50) }, Color.red);
            //Draw.DrawTriangle(renderer, new Vector3[] { new Vector2(131, 90), new Vector2(20, 90), new Vector2(50, 50) }, Color.white);
            //Draw.DrawTriangle(renderer, new Vector3[] { new Vector2(200, 90), new Vector2(200, 200), new Vector2(150, 120) }, Color.yellow);


            Draw.DrawObj(renderer, obj);

            renderer.Apply();
        }

        private static Object LoadObj(string path)
        {
            Object obj = new Object(File.ReadAllLines(path));
            return obj;
        }

        float time = 1f;
        public void Update()
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                time = 1f;
                light.rot = rotLgiht.transform.localEulerAngles;
                Draw.DrawObj(renderer, obj);
                renderer.Apply();
            }
        }
    }
}