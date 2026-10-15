using UnityEditor;
using UnityEngine;
using com.ktgame.utils.time_scale_on_toolbar;

namespace com.ktgame.utils.time_scale_on_toolbar.editor
{
    internal sealed class CustomTimeScalePopup : PopupWindowContent
    {
        private static readonly Vector2 WindowSize = new Vector2(240, 130);
        private float _sliderValue;

        public CustomTimeScalePopup(float initialValue)
        {
            _sliderValue = Mathf.Clamp(initialValue, 0f, 20f);
        }

        public override Vector2 GetWindowSize() => WindowSize;

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Custom Time Scale", EditorStyles.boldLabel);

            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                _sliderValue = EditorGUILayout.Slider(_sliderValue, 0f, 10f);
                if (EditorGUI.EndChangeCheck())
                {
                    TimeScaleManager.SetTimeScale(_sliderValue);
                }
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Quick Chips", EditorStyles.miniBoldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawChip(0.1f);
                DrawChip(0.5f);
                DrawChip(1.0f);
                DrawChip(2.0f);
                DrawChip(5.0f);
                DrawChip(10.0f);
            }

            EditorGUILayout.Space(8);
            if (GUILayout.Button("Close", EditorStyles.miniButton))
            {
                editorWindow.Close();
            }
        }

        private void DrawChip(float value)
        {
            bool isActive = Mathf.Approximately(TimeScaleManager.Current, value);
            var prevBg = GUI.backgroundColor;
            if (isActive)
            {
                GUI.backgroundColor = new Color(0.35f, 0.75f, 1f, 1f);
            }

            if (GUILayout.Button($"{value:0.#}x", EditorStyles.miniButton))
            {
                _sliderValue = value;
                TimeScaleManager.SetTimeScale(value);
            }

            GUI.backgroundColor = prevBg;
        }

        public static void Show(Rect activatorRect, float initialValue)
        {
            PopupWindow.Show(activatorRect, new CustomTimeScalePopup(initialValue));
        }
    }
}
