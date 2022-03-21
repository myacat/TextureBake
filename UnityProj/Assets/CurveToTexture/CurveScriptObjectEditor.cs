using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering;

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
        {
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

            EditorGUILayout.Space();

            cso.type = (DataType)EditorGUILayout.EnumPopup("Data Type", cso.type);
            switch (cso.type)
            {
                case DataType.Curve:
                    cso.ac = EditorGUILayout.CurveField(cso.ac, Color.red, new Rect(0, 0, 1, 1));
                    break;
                case DataType.Gradient:
                    cso.gd = EditorGUILayout.GradientField(cso.gd);
                    break;
            }
        }
        if (EditorGUI.EndChangeCheck())
        {
            if (AutoRefresh)
            {
                RefreshCureveTex();
            }
        }

        GUILayout.FlexibleSpace();
        
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            AutoRefresh = GUILayout.Toggle(AutoRefresh, "AutoRefresh",GUILayout.MaxWidth(80f));

            if (GUILayout.Button("Refresh", GUILayout.MaxWidth(80f)) && !AutoRefresh)
            {
                RefreshCureveTex();
            }
            if (GUILayout.Button("Save", GUILayout.MaxWidth(80f)))
            {
                SaveCureveTex();
            }
        }
        EditorGUILayout.EndHorizontal();
    }
    public void DrawPreview(Rect r)
    {
        EditorGUILayout.BeginHorizontal("preToolbar");
        GUILayout.Label("preview:", "preToolbar2");
        GUILayout.FlexibleSpace();
        boxHeight = VerticalDragBar(boxHeight ,36, r.height-200);
        OnPreviewSettings();
        
        EditorGUILayout.EndHorizontal();
        if (cureveTex != null)
        {
            
            EditorGUILayout.BeginHorizontal("PreBackground", GUILayout.Height(boxHeight));

            Rect rect = GUILayoutUtility.GetRect(50, EditorGUIUtility.currentViewWidth, 50,1000);
            OnPreviewGUI(rect,"PreBackground");

            EditorGUILayout.EndHorizontal();

        }

    }
    static float boxHeight = 200;
    private static bool draggingFlag = false;
    private static float VerticalDragBar( float height, float min = 36, float max = 512)
    {
        //Rect dragBarRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth , 0);
        Rect dragBarRect = GUILayoutUtility.GetLastRect();
        dragBarRect.x += 3;
        dragBarRect.y += 7;
        dragBarRect.height = 16;
        dragBarRect.width -= 6;

        if (Event.current.type == EventType.Repaint)
        {
            GUIStyle s = new GUIStyle("RL DragHandle");
            s.Draw(dragBarRect, GUIContent.none, false, false, false, false);
        }
            
        EditorGUIUtility.AddCursorRect(dragBarRect, MouseCursor.ResizeVertical);//切换鼠标样式

        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        switch (Event.current.GetTypeForControl(controlID))
        {
            case EventType.MouseDown:
                //点击时判断是否落入感应区域，落入感应区域才可拖拽
                draggingFlag = dragBarRect.Contains(Event.current.mousePosition);
                break;
            case EventType.MouseDrag:
                if (draggingFlag)
                {
                    height = Mathf.Clamp(height - Event.current.delta.y, min, max);
                    GUI.changed = true;
                    //HandleUtility.Repaint();
                }
                break;
            case EventType.MouseUp:
                draggingFlag = false;
                break;
        }
        return height;
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
                switch (cso.type)
                {
                    case DataType.Curve:
                        float curveValue = cso.ac.Evaluate((float)x / (float)h);
                        cureveTex.SetPixel(x, y, new Color(curveValue, curveValue, curveValue, curveValue));
                        break;
                    case DataType.Gradient:
                        Color colorValue = cso.gd.Evaluate((float)x / (float)h);
                        cureveTex.SetPixel(x, y, colorValue);
                        break;
                }

            }
        }
        cureveTex.Apply();
    }

    public override bool HasPreviewGUI()
    {
        return (cureveTex != null);
    }

    private PreviewMode m_PreviewMode = PreviewMode.RGB;

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {


        ColorWriteMask colorWriteMask = ColorWriteMask.All;

        switch (m_PreviewMode)
        {
            case PreviewMode.R:
                colorWriteMask = ColorWriteMask.Red | ColorWriteMask.Alpha;
                break;
            case PreviewMode.G:
                colorWriteMask = ColorWriteMask.Green | ColorWriteMask.Alpha;
                break;
            case PreviewMode.B:
                colorWriteMask = ColorWriteMask.Blue | ColorWriteMask.Alpha;
                break;
        }

        if (m_PreviewMode == PreviewMode.A)
        {
            EditorGUI.DrawTextureAlpha(r, cureveTex, ScaleMode.ScaleToFit);
        }
        else
        {
            EditorGUI.DrawPreviewTexture(r, cureveTex, null, ScaleMode.ScaleToFit, 0, 0, colorWriteMask);
        }
    }
    public override void OnPreviewSettings()
    {

        List<PreviewMode> previewCandidates = new List<PreviewMode>(5);
        previewCandidates.Add(PreviewMode.RGB);
        previewCandidates.Add(PreviewMode.R);
        previewCandidates.Add(PreviewMode.G);
        previewCandidates.Add(PreviewMode.B);
        previewCandidates.Add(PreviewMode.A);


        if (previewCandidates.Count > 1 && cureveTex != null)
        {

            if (previewCandidates.Contains(PreviewMode.RGB))
                m_PreviewMode = GUILayout.Toggle(m_PreviewMode == PreviewMode.RGB, "RGB", "preButton")
                    ? PreviewMode.RGB
                    : m_PreviewMode;
            if (previewCandidates.Contains(PreviewMode.R))
                m_PreviewMode = GUILayout.Toggle(m_PreviewMode == PreviewMode.R, "R", "preButtonRed")
                    ? PreviewMode.R
                    : m_PreviewMode;
            if (previewCandidates.Contains(PreviewMode.G))
                m_PreviewMode = GUILayout.Toggle(m_PreviewMode == PreviewMode.G, "G", "preButtonGreen")
                    ? PreviewMode.G
                    : m_PreviewMode;
            if (previewCandidates.Contains(PreviewMode.B))
                m_PreviewMode = GUILayout.Toggle(m_PreviewMode == PreviewMode.B, "B", "preButtonBlue")
                    ? PreviewMode.B
                    : m_PreviewMode;
            if (previewCandidates.Contains(PreviewMode.A))
                m_PreviewMode = GUILayout.Toggle(m_PreviewMode == PreviewMode.A, "A", "preButton")
                    ? PreviewMode.A
                    : m_PreviewMode;
        }
    }
    enum PreviewMode
    {
        RGB,
        R,
        G,
        B,
        A,
    };
}
