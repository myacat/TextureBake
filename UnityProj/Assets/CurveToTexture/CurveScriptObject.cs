using UnityEngine;

namespace DataToTextureTools
{
    public class CurveScriptObject : ScriptableObject
    {
        public string savePath = "Assets";
        public string cureveTexName = "cureveTex";
        public bool square = true;
        public DataType type = DataType.Curve;
        public Size _sizeH = Size.SizeX128;
        public Size _sizeV = Size.SizeX128;
        public CureveTextureFormat cureveTexFM = CureveTextureFormat.Png;
        public MappingType mappingType = MappingType.LinearHorizontal;
        public AnimationCurve ac = AnimationCurve.Linear(0, 0, 1, 1);
        public Gradient gd = new Gradient();

        public bool dual_data = false;
        public AnimationCurve ac_dual = AnimationCurve.Linear(0, 0, 1, 1);
        public Gradient gd_dual = new Gradient();
        public float lerp_pow = 1;
        public AnimationCurve data_dua_curve = AnimationCurve.Linear(0, 0, 1, 1);
        public bool AutoRefresh = false;

        public Texture2D cureveTex;

        public bool use_link = false;
        public Material link_mat;
        public string[] link_propety = new string[] { "texture propety", "texturedescription" };

        public void RefreshCureveTex()
        {
            int h = (int)_sizeH;
            int v = (int)_sizeV;
            cureveTex = new Texture2D(h, v, TextureFormat.RGBAFloat, false);
            cureveTex.filterMode = FilterMode.Point;
            cureveTex.wrapMode = TextureWrapMode.Clamp;
            for (int x = 0; x < h; x++)
            {
                for (int y = 0; y < v; y++)
                {
                    float samplingKey = 0;
                    switch (mappingType)
                    {
                        case MappingType.LinearHorizontal:
                            samplingKey = (float)x / h;
                            break;
                        case MappingType.LinearVertical:
                            samplingKey = (float)y / v;
                            break;
                        case MappingType.Radial:
                            samplingKey = 2 * Vector2.Distance(new Vector2((float)x / h, (float)y / v), new Vector2(0.5f, 0.5f));
                            break;
                        case MappingType.Box:
                            samplingKey = Mathf.Max(Mathf.Abs(2f * (float)x / h - 1), Mathf.Abs(2f * (float)y / v - 1));
                            break;
                        case MappingType.MirrorH:
                            samplingKey = Mathf.Abs(2f * (float)x / h - 1);
                            break;
                        case MappingType.MirrorV:
                            samplingKey = Mathf.Abs(2f * (float)y / v - 1);
                            break;
                        case MappingType.DualData:
                            samplingKey = (float)x / h;
                            break;
                    }

                    float lerpValue = data_dua_curve.Evaluate((float)y / v);
                    switch (type)
                    {
                        case DataType.Curve:
                            float curveValue = ac.Evaluate(samplingKey);
                            if (mappingType == MappingType.DualData)
                            {
                                AnimationCurve ad_temp = new AnimationCurve();
                                Keyframe[] keyframes = new Keyframe[Mathf.Min(ac_dual.keys.Length, ac.keys.Length)];
                                for (int keyCount = 0; keyCount < keyframes.Length; keyCount++)
                                {
                                    keyframes[keyCount].weightedMode = ac.keys[keyCount].weightedMode;
                                    keyframes[keyCount].time = Mathf.Lerp(ac_dual.keys[keyCount].time, ac.keys[keyCount].time, lerpValue);
                                    keyframes[keyCount].value = Mathf.Lerp(ac_dual.keys[keyCount].value, ac.keys[keyCount].value, lerpValue);
                                }

                                ad_temp.keys = keyframes;
                                curveValue = ad_temp.Evaluate(samplingKey);
                            }

                            cureveTex.SetPixel(x, y, new Color(curveValue, curveValue, curveValue, curveValue));
                            break;

                        case DataType.Gradient:
                            Color colorValue = gd.Evaluate(samplingKey);
                            if (mappingType == MappingType.DualData)
                            {
                                Gradient gd_temp = new Gradient();
                                GradientColorKey[] ck = new GradientColorKey[Mathf.Min(gd_dual.colorKeys.Length, gd.colorKeys.Length)];
                                for (int colorKeyCount = 0; colorKeyCount < Mathf.Min(gd_dual.colorKeys.Length, gd.colorKeys.Length); colorKeyCount++)
                                {
                                    ck[colorKeyCount].time = Mathf.Lerp(gd_dual.colorKeys[colorKeyCount].time, gd.colorKeys[colorKeyCount].time, lerpValue);
                                    ck[colorKeyCount].color = Color.Lerp(gd_dual.colorKeys[colorKeyCount].color, gd.colorKeys[colorKeyCount].color, lerpValue);
                                }

                                GradientAlphaKey[] ak = new GradientAlphaKey[Mathf.Min(gd_dual.alphaKeys.Length, gd.alphaKeys.Length)];
                                for (int alphaKeyCount = 0; alphaKeyCount < ak.Length; alphaKeyCount++)
                                {
                                    ak[alphaKeyCount].time = Mathf.Lerp(gd_dual.alphaKeys[alphaKeyCount].time, gd.alphaKeys[alphaKeyCount].time, lerpValue);
                                    ak[alphaKeyCount].alpha = Mathf.Lerp(gd_dual.alphaKeys[alphaKeyCount].alpha, gd.alphaKeys[alphaKeyCount].alpha, lerpValue);
                                }

                                gd_temp.SetKeys(ck, ak);
                                colorValue = gd_temp.Evaluate(samplingKey);
                            }

                            cureveTex.SetPixel(x, y, colorValue);
                            break;
                    }
                }
            }

            cureveTex.Apply();
        }
    }

    public enum MappingType
    {
        LinearHorizontal,
        LinearVertical,
        Radial,
        Box,
        MirrorH,
        MirrorV,
        DualData,
    }

    public enum Size
    {
        SizeX1 = 1,
        SizeX2 = 2,
        SizeX4 = 4,
        SizeX8 = 8,
        SizeX16 = 16,
        SizeX32 = 32,
        SizeX64 = 64,
        SizeX128 = 128,
        SizeX256 = 256,
        SizeX512 = 512,
    }

    public enum CureveTextureFormat
    {
        Png,
        Tga,
        Jpg,
        Exr,
    }

    public enum DataType
    {
        Gradient,
        Curve,
    }

}