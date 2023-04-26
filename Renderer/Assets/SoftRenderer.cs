using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SoftRenderer
{
    public class SoftRenderer : MonoBehaviour
    {
        public RawImage UI;
        public Texture2D renderer;


        public void Start()
        {
            renderer = new Texture2D(Screen.width, Screen.height);
            UI.texture = renderer;

            for (int i = 0; i < Screen.width; i++)
            {
                for (int j = 0; j < Screen.height; j++)
                {
                    Draw.DrawPixel(renderer, i, j, Color.black);
                }
            }
            Draw.DrawLine(renderer, 13, 20, 80, 40, Color.white);
            Draw.DrawLine(renderer, 20, 13, 40, 80, Color.red);
            Draw.DrawLine(renderer, 80, 40, 13, 20, Color.white);

            string path = Application.dataPath + "/head.txt";
            Object obj = LoadObj(path);
            Draw.DrawObj(renderer, obj);

            renderer.Apply();
        }

        private Object LoadObj(string path)
        {
            Object obj = new Object(File.ReadAllLines(path));
            return obj;
        }
    }
}