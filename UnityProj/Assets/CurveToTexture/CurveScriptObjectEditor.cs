#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering;

namespace DataToTextureTools
{
    [CustomEditor(typeof(CurveScriptObject))]
    public class CurveScriptObjectEditor : Editor
    {
        private CurveScriptObject cso;

        public void OnEnable()
        {
            cso = (CurveScriptObject)target;
            cso.RefreshCureveTex();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            {
                GUILayout.Label($"Save Path:{cso.savePath}");
                DoBaseSettingsArea();
                DoLinkArea();
                DoBottomArea();
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (cso.AutoRefresh)
                {
                    cso.RefreshCureveTex();
                }
            }
        }

        private void DoBaseSettingsArea()
        {
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            GUILayout.BeginVertical("Box");
            {
                cso.square = EditorGUILayout.Toggle("Square Size?", cso.square);
                if (cso.square)
                {
                    cso._sizeH = (Size)EditorGUILayout.EnumPopup("Size:", cso._sizeH);
                    cso._sizeV = cso._sizeH;
                }
                else
                {
                    cso._sizeH = (Size)EditorGUILayout.EnumPopup("Horizontal Size:", cso._sizeH);
                    cso._sizeV = (Size)EditorGUILayout.EnumPopup("Vertical Size:", cso._sizeV);
                }

                cso.cureveTexFM = (CureveTextureFormat)EditorGUILayout.EnumPopup("Texture Format:", cso.cureveTexFM);
                cso.mappingType = (MappingType)EditorGUILayout.EnumPopup("Mapping Type:", cso.mappingType);

                EditorGUILayout.Space();

                cso.type = (DataType)EditorGUILayout.EnumPopup("Data Type", cso.type);
                switch (cso.type)
                {
                    case DataType.Curve:
                        cso.ac = EditorGUILayout.CurveField("Curve_1:", cso.ac, Color.red, new Rect(0, 0, 1, 1));
                        break;
                    case DataType.Gradient:
                        cso.gd = EditorGUILayout.GradientField("Gradient_1", cso.gd);
                        break;
                }

                if (cso.mappingType == MappingType.DualData)
                {
                    switch (cso.type)
                    {
                        case DataType.Curve:
                            cso.ac_dual = EditorGUILayout.CurveField("Curve_2:", cso.ac_dual, Color.red, new Rect(0, 0, 1, 1));
                            if (cso.ac_dual.keys.Length != cso.ac.keys.Length)
                            {
                                EditorGUILayout.HelpBox("key 数量不一致，可能会导致预期外的效果！", MessageType.Warning);
                            }
                            break;
                        case DataType.Gradient:
                            cso.gd_dual = EditorGUILayout.GradientField("Gradient_2", cso.gd_dual);
                            if (cso.gd_dual.colorKeys.Length != cso.gd.colorKeys.Length)
                            {
                                EditorGUILayout.HelpBox("colorKeys 数量不一致，可能会导致预期外的效果！", MessageType.Warning);
                            }

                            if (cso.gd_dual.alphaKeys.Length != cso.gd.alphaKeys.Length)
                            {
                                EditorGUILayout.HelpBox("alphaKeys 数量不一致，可能会导致预期外的效果！", MessageType.Warning);
                            }
                            break;
                    }

                    cso.data_dua_curve = EditorGUILayout.CurveField("Blend Curve:", cso.data_dua_curve, Color.red, new Rect(0, 0, 1, 1));
                }

                GUILayout.EndVertical();
            }
        }

        private void DoLinkArea()
        {
            GUILayout.Label("Link Mat Settings", EditorStyles.boldLabel);
            GUILayout.BeginVertical("Box");
            cso.use_link = EditorGUILayout.Toggle("Link Material", cso.use_link);
            if (cso.use_link)
            {
                EditorGUILayout.BeginHorizontal();
                cso.link_mat = (Material)EditorGUILayout.ObjectField(cso.link_mat, typeof(Material), false);


                GUILayout.Label("=>", GUILayout.Width(30));
                EditorGUI.BeginDisabledGroup(cso.link_mat == null);
                GUIStyle s = new GUIStyle("DropDownButton");
                s.margin.top = -1;

                if (GUILayout.Button($"{cso.link_propety[0]}", s))
                {
                    ShowPropetySelectMenu(cso.link_mat.shader);
                }

                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
                if (cso.link_propety[1] != "texturedescription")
                {
                    if (cso.link_mat != null && cso.link_mat.GetTexture(cso.link_propety[1]) != cso.cureveTex)
                    {
                        cso.link_mat.SetTexture(cso.link_propety[1], cso.cureveTex);
                    }
                }
            }
            else
            {
                if (cso.link_propety[1] != "texturedescription")
                {
                    if (cso.link_mat != null && cso.link_mat.GetTexture(cso.link_propety[1]) == cso.cureveTex)
                    {
                        cso.link_mat.SetTexture(cso.link_propety[1], null);
                    }
                }
            }

            GUILayout.EndVertical();
        }

        private void ShowPropetySelectMenu(Shader shader)
        {
            GenericMenu menu = new GenericMenu();
            var propertiesCount = ShaderUtil.GetPropertyCount(cso.link_mat.shader);
            for (int i = 0; i < propertiesCount; ++i)
            {
                var type = ShaderUtil.GetPropertyType(shader, i);
                var name = ShaderUtil.GetPropertyName(shader, i);
                var description = ShaderUtil.GetPropertyDescription(shader, i);
                if (type == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    menu.AddItem(new GUIContent($"{description}({name})"), false, SetLinkPropety, new string[] { description, name });
                }
            }

            menu.ShowAsContext();
        }

        private void SetLinkPropety(object name)
        {
            cso.link_mat.SetTexture(cso.link_propety[1], null);
            cso.link_propety = (string[])name;
        }

        private void DoBottomArea()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                cso.AutoRefresh = GUILayout.Toggle(cso.AutoRefresh, "AutoRefresh", GUILayout.MaxWidth(80f));

                if (GUILayout.Button("Refresh", GUILayout.MaxWidth(60f)) && !cso.AutoRefresh)
                {
                    cso.RefreshCureveTex();
                }

                if (GUILayout.Button("Save", GUILayout.MaxWidth(60f)))
                {
                    if (cso.savePath == "Assets")
                    {
                        SaveAsCureveTex();
                    }
                    else
                    {
                        SaveCureveTex();
                    }
                }

                if (GUILayout.Button("SaveAs", GUILayout.MaxWidth(60f)))
                {
                    SaveAsCureveTex();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        //这段代码可以把生成的图片设为资产的缩略图，
        //但需要重新导入才能更新，而且太像纹理容易看错
        //所以暂时屏蔽掉这个功能
        //  public override Texture2D RenderStaticPreview
        //  (
        //      string assetPath,
        //      Object[] subAssets,
        //      int width,
        //      int height
        //  )
        //  {
        //      Debug.Log("Rendered Statics");
        //      var renderer = target as CurveScriptObject;
        //      var icon = renderer.cureveTex;
        //      var cache = new Texture2D(width, height);
        //      EditorUtility.CopySerialized(icon, cache);
        //      return cache;
        //  }

        /// <summary>
        /// 把cureveTex保存为本地文件
        /// </summary>
        public void SaveCureveTex()
        {
            if (cso.cureveTex != null)
            {
                if (cso.savePath != "Assets")
                {
                    byte[] dataBytes = EncodeTexture(cso.cureveTex, cso.cureveTexFM);
                    FileStream fileStream = File.Open(cso.savePath, FileMode.OpenOrCreate);
                    fileStream.Write(dataBytes, 0, dataBytes.Length);
                    fileStream.Close();
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    if (cso.use_link && cso.link_mat != null)
                    {
                        Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath(cso.savePath, typeof(Texture2D));
                        cso.link_mat.SetTexture(cso.link_propety[1], tex);
                        cso.use_link = false;
                    }
                }
            }
        }

        public void SaveAsCureveTex()
        {
            string savePath = EditorUtility.SaveFilePanelInProject("Save png", cso.cureveTexName, cso.cureveTexFM.ToString().ToLower(), "Please enter a file name to save the texture to", "Assets") + "";

            if (savePath != "")
            {
                cso.savePath = savePath;
                cso.cureveTexName = cso.savePath.Substring(cso.savePath.LastIndexOf("/") + 1);
            }

            SaveCureveTex();
        }

        public void DrawCustomPreview(Rect r)
        {
            EditorGUILayout.BeginHorizontal("preToolbar");
            GUILayout.Label("preview:", "preToolbar2");
            GUILayout.FlexibleSpace();
            boxHeight = VerticalDragBar(boxHeight, 36, r.height - 200);
            OnPreviewSettings();

            EditorGUILayout.EndHorizontal();
            if (cso.cureveTex != null)
            {

                EditorGUILayout.BeginHorizontal("PreBackground", GUILayout.Height(boxHeight));

                Rect rect = GUILayoutUtility.GetRect(50, EditorGUIUtility.currentViewWidth, 50, 1000);

                OnPreviewGUI(rect, "PreBackground");

                EditorGUILayout.EndHorizontal();

            }
        }

        private static float boxHeight = 200;
        private static bool draggingFlag = false;
        private static float VerticalDragBar(float height, float min = 36, float max = 512)
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

            //切换鼠标样式
            EditorGUIUtility.AddCursorRect(dragBarRect, MouseCursor.ResizeVertical);

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

        public byte[] EncodeTexture(Texture2D t, CureveTextureFormat format)
        {
            switch (format)
            {
                case CureveTextureFormat.Png:
                    return t.EncodeToPNG();
                case CureveTextureFormat.Jpg:
                    return t.EncodeToJPG();
                case CureveTextureFormat.Exr:
                    return t.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);
                case CureveTextureFormat.Tga:
                    return t.EncodeToTGA();
                default:
                    return null;
            }
        }

        public override bool HasPreviewGUI()
        {
            return (cso.cureveTex != null);
        }

        private PreviewMode m_PreviewMode = PreviewMode.ChannelRGB;

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            ColorWriteMask colorWriteMask = ColorWriteMask.All;

            switch (m_PreviewMode)
            {
                case PreviewMode.ChannelR:
                    colorWriteMask = ColorWriteMask.Red | ColorWriteMask.Alpha;
                    break;
                case PreviewMode.ChannelG:
                    colorWriteMask = ColorWriteMask.Green | ColorWriteMask.Alpha;
                    break;
                case PreviewMode.ChannelB:
                    colorWriteMask = ColorWriteMask.Blue | ColorWriteMask.Alpha;
                    break;
            }

            if (m_PreviewMode == PreviewMode.ChannelA)
            {

                EditorGUI.DrawTextureAlpha(r, cso.cureveTex, ScaleMode.ScaleToFit);
            }
            else
            {
                EditorGUI.DrawPreviewTexture(r, cso.cureveTex, null, ScaleMode.ScaleToFit, 0, 0, colorWriteMask);
            }
        }

        public override void OnPreviewSettings()
        {
            List<PreviewMode> previewCandidates = new List<PreviewMode>(5);
            previewCandidates.Add(PreviewMode.ChannelRGB);
            previewCandidates.Add(PreviewMode.ChannelR);
            previewCandidates.Add(PreviewMode.ChannelG);
            previewCandidates.Add(PreviewMode.ChannelB);
            previewCandidates.Add(PreviewMode.ChannelA);

            if (previewCandidates.Count > 1 && cso.cureveTex != null)
            {

                if (previewCandidates.Contains(PreviewMode.ChannelRGB))
                {
                    m_PreviewMode = GUILayout.Toggle(m_PreviewMode == PreviewMode.ChannelRGB, "RGB", "preButton")
                        ? PreviewMode.ChannelRGB
                        : m_PreviewMode;
                }

                if (previewCandidates.Contains(PreviewMode.ChannelR))
                {
                    m_PreviewMode = GUILayout.Toggle(m_PreviewMode == PreviewMode.ChannelR, "R", "preButtonRed")
                        ? PreviewMode.ChannelR
                        : m_PreviewMode;
                }

                if (previewCandidates.Contains(PreviewMode.ChannelG))
                {
                    m_PreviewMode = GUILayout.Toggle(m_PreviewMode == PreviewMode.ChannelG, "G", "preButtonGreen")
                        ? PreviewMode.ChannelG
                        : m_PreviewMode;
                }

                if (previewCandidates.Contains(PreviewMode.ChannelB))
                {
                    m_PreviewMode = GUILayout.Toggle(m_PreviewMode == PreviewMode.ChannelB, "B", "preButtonBlue")
                        ? PreviewMode.ChannelB
                        : m_PreviewMode;
                }

                if (previewCandidates.Contains(PreviewMode.ChannelA))
                {
                    m_PreviewMode = GUILayout.Toggle(m_PreviewMode == PreviewMode.ChannelA, "A", "preButton")
                        ? PreviewMode.ChannelA
                        : m_PreviewMode;
                }
            }
        }

        public enum PreviewMode
        {
            ChannelRGB,
            ChannelR,
            ChannelG,
            ChannelB,
            ChannelA,
        }
    }
}
#endif