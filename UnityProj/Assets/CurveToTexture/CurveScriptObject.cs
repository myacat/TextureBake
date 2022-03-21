using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveScriptObject : ScriptableObject
{
    public bool square = true;
    public DataType type = DataType.Curve;
    public SIZE _sizeH = SIZE.x128;
    public SIZE _sizeV = SIZE.x128;
    public CureveTextureFormat cureveTexFM = CureveTextureFormat.PNG;
    public AnimationCurve ac = AnimationCurve.Linear(0, 0, 1, 1);
    public Gradient gd = new Gradient();
    public bool AutoRefresh = false;
}

public enum SIZE
{
    x1 = 1,
    x64 = 64,
    x128 = 128,
    x256 = 256,
    x512 = 512,
}

public enum CureveTextureFormat
{
    PNG,
    TGA,
    JPG,
    EXR
}

public enum DataType
{
    Gradient,
    Curve
}