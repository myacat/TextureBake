using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(CurveScriptObject))]
public class CurveScriptObjectEditor : Editor
{
    CurveScriptObject cso;

    Texture2D cureveTex;
    public bool AutoRefresh = false;
    public void OnEnable()
    {
        cso = (CurveScriptObject)target;
        RefreshCureveTex();
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        cso.square = EditorGUILayout.Toggle("Square Size?", cso.square);
        if (cso.square)
        {
            cso._sizeH = (SIZE)EditorGUILayout.EnumPopup("Size:", cso._sizeH);
            cso._sizeV = cso._sizeH;
        }
        else
        {
            cso._sizeH = (SIZE)EditorGUILayout.EnumPopup("Horizontal Size:", cso._sizeH);
            cso._sizeV = (SIZE)EditorGUILayout.EnumPopup("Vertical Size:", cso._sizeV);
        }
        cso.cureveTexFM = (CureveTextureFormat)EditorGUILayout.EnumPopup("Texture Format:", cso.cureveTexFM);
        cso.ac = EditorGUILayout.CurveField(cso.ac);
        EditorGUILayout.BeginHorizontal();
        {
            AutoRefresh = GUILayout.Toggle(AutoRefresh, "AutoRefresh");

            if (GUILayout.Button("Refresh") && !AutoRefresh)
            {
                RefreshCureveTex();
            }
        }
        if (EditorGUI.EndChangeCheck())
        {
            if (AutoRefresh)
            {
                RefreshCureveTex();
            }
        }

        EditorGUILayout.EndHorizontal();

        DrawPreview();
        if (GUILayout.Button("Save"))
        {
            SaveCureveTex();
        }
    }
    public void DrawPreview()
    {
        GUILayout.Label("preview:", EditorStyles.boldLabel);
        if (cureveTex != null)
        {
            EditorGUILayout.BeginHorizontal("PreBackground");
            GUILayout.FlexibleSpace();
            Rect rect = GUILayoutUtility.GetRect(50, 150, 50, 150);
            EditorGUI.DrawPreviewTexture(rect, cureveTex);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

        }

    }
    public void SaveCureveTex()
    {
        if (cureveTex != null)
        {
            byte[] dataBytes = EncodeTexture(cureveTex, cso.cureveTexFM);
            //string savePath = Application.dataPath + "/SampleCircle.png";

            string savePath = EditorUtility.SaveFilePanelInProject("Save png", "cureveTex", cso.cureveTexFM.ToString().ToLower(), "Please enter a file name to save the texture to");

            if (savePath != string.Empty)
            {
                FileStream fileStream = File.Open(savePath, FileMode.OpenOrCreate);
                fileStream.Write(dataBytes, 0, dataBytes.Length);
                fileStream.Close();
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
            }
        }
    }
    public byte[] EncodeTexture(Texture2D t, CureveTextureFormat format)
    {
        switch (format)
        {
            case CureveTextureFormat.PNG:
                return t.EncodeToPNG();
            case CureveTextureFormat.JPG:
                return t.EncodeToJPG();
            case CureveTextureFormat.EXR:
                return t.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);
            case CureveTextureFormat.TGA:
                return t.EncodeToTGA();
            default:
                return null;
        }

    }

    public void RefreshCureveTex()
    {
        int h = (int)cso._sizeH;
        int v = (int)cso._sizeV;
        cureveTex = new Texture2D(h, v, TextureFormat.RGBAFloat, false);

        for (int x = 0; x < h; x++)
        {
            for (int y = 0; y < v; y++)
            {
                float curveValue = cso.ac.Evaluate((float)x / (float)h);
                cureveTex.SetPixel(x, y, new Color(curveValue, curveValue, curveValue, curveValue));
            }
        }
        cureveTex.Apply();
    }
}
