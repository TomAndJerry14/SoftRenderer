using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct VSOutBuf
{
    public Vector4 screen_crood;
    public Vector4 clipPos;
    public Vector3 worldPos;
    public Vector3 worldNormal;
    public Vector3 normal;

    public Matrix4x4 varying_uv;
    public Matrix4x4 varying_normal;
    public Matrix4x4 varying_tri;
}
