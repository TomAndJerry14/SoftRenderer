using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SoftRenderer
{
    public interface IShader
    {
        Vector4 vertex( int faceIndex, int vertexIndex);//todo 替换成顶点
        bool fragment( Vector3 bar, out Color color, Texture2D output);
    }
}
