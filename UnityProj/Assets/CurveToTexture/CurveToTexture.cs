using UnityEngine;
using UnityEditor;

namespace DataToTextureTools
{
    public class CurveToTexture : EditorWindow, IHasCustomMenu
    {
        [MenuItem("MyaTools/CurveToTexture Window")]
        static private void Init()
        {
            // Get existing open window or if none, make a new one:
            CurveToTexture window = (CurveToTexture)EditorWindow.GetWindow(typeof(CurveToTexture));
            window.minSize = new Vector2(300, 400);
            window.Show();
        }

        private CurveScriptObject m_cso;
        private CurveScriptObject cso
        {
            get
            {
                if (m_cso == null)
                {
                    m_cso = CreateInstance<CurveScriptObject>();
                }

                return m_cso;
            }
            set
            {
                m_cso = value;
            }
        }

        private CurveScriptObjectEditor cttEditor;
        void OnGUI()
        {
            if (cso != null)
            {
                if (cttEditor == null)
                {
                    cttEditor = (CurveScriptObjectEditor)Editor.CreateEditor(cso, typeof(CurveScriptObjectEditor));
                    cttEditor.OnEnable();
                }

            }

            cttEditor.OnInspectorGUI();
            Rect lastTect = GUILayoutUtility.GetLastRect();
            Rect previewRect = position;
            previewRect.height -= lastTect.height;
            cttEditor.DrawCustomPreview(previewRect);
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(m_SaveSettings, false, MenuItemmSaveSettings);
        }

        private readonly GUIContent m_SaveSettings = new GUIContent("Save Settings");

        private void MenuItemmSaveSettings()
        {
            string savePath = EditorUtility.SaveFilePanelInProject("Save Sattings", "CurveToTextureSettings", "asset", "Please enter a file name to save the Settings");

            if (savePath != string.Empty)
            {
                cso = Instantiate(cso);
                AssetDatabase.CreateAsset(cso, savePath);
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = cso;
            }
        }
    }
}
