using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public struct VSOutBuf
{
    public NativeArray<Vector4> screen_crood;
    public NativeArray<Vector4> clipPos;
    public NativeArray<Vector3> worldPos;
    public NativeArray<Vector3> worldNormal;
    public NativeArray<Vector3> normal;

    public Matrix4x4 varying_uv;
    public Matrix4x4 varying_normal;
    public Matrix4x4 varying_tri;
}
