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
    public MappingType mappingType = MappingType.Linear_Horizontal;
    public AnimationCurve ac = AnimationCurve.Linear(0, 0, 1, 1);
    public Gradient gd = new Gradient();

    public bool dual_data = false;
    public AnimationCurve ac_dual = AnimationCurve.Linear(0, 0, 1, 1);
    public Gradient gd_dual = new Gradient();
    public float lerp_pow = 1;
    public AnimationCurve data_dua_curve = AnimationCurve.Linear(0, 0, 1, 1);
    public bool AutoRefresh = false;
}

public enum MappingType
{
    Linear_Horizontal,
    Linear_Vertical,
    Radial,
    Box,
    Mirror_H,
    Mirror_V,
    dual_data,
}

public enum SIZE
{
    x1 = 1,
    x2 = 2,
    x4 = 4,
    x8 = 8,
    x16 = 16,
    x32 = 32,
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
    EXR,
}

public enum DataType
{
    Gradient,
    Curve,
}