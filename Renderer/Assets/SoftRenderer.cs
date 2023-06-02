using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
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

        public static Object obj = null;

        public Texture2D texture;

        public float cameraX;
        public float cameraY;
        public float cameraZ;

        public float centerX = 0f;
        public float centerY = 0f;
        public float centerZ = 0f;


        Camera cam;

        public void Start()
        {
            Instance = this;
            string path = Application.dataPath + "/head.obj";
            obj = LoadObj(path);

            cam = new Camera();
            UI.texture = cam.renderer;

         
            



            //Draw.DrawTriangle(renderer, new Vector2(10, 10), new Vector2(100, 10), new Vector2(50, 50), Color.red);
            //Draw.DrawTriangle(renderer, new Vector2(131, 90), new Vector2(20, 90), new Vector2(50, 50), Color.white);
            //Draw.DrawTriangle(renderer, new Vector2(200, 90), new Vector2(200, 200), new Vector2(150, 120), Color.yellow);

            //Draw.DrawTriangle(renderer, new Vector3[] { new Vector2(10, 10), new Vector2(100, 10), new Vector2(50, 50) }, Color.red);
            //Draw.DrawTriangle(renderer, new Vector3[] { new Vector2(131, 90), new Vector2(20, 90), new Vector2(50, 50) }, Color.white);
            //Draw.DrawTriangle(renderer, new Vector3[] { new Vector2(200, 90), new Vector2(200, 200), new Vector2(150, 120) }, Color.yellow);


        }

        

        private static Object LoadObj(string path)
        {
            Object obj = new Object(File.ReadAllLines(path));
            return obj;
        }

        public void Update()
        {
            cam.Update();
        }
    }
}