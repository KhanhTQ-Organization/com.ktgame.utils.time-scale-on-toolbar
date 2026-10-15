using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.ktgame.utils.time_scale_on_toolbar.editor
{
    internal sealed class TimeScaleSliderOnToolbarSettingProvider : SettingsProvider
    {
        private const string PATH = "Ktgame/Time Scale Slider On Toolbar";

        private SerializedObject _serializedObject;
        private SerializedProperty _isEnableProp;
        private SerializedProperty _displayModeProp;
        private SerializedProperty _toolbarZoneProp;
        private SerializedProperty _timeScaleArrayProp;
        private SerializedProperty _muteAudioProp;
        private SerializedProperty _autoResetProp;
        private SerializedProperty _enableScrollWheelProp;
        private SerializedProperty _showConsoleLogProp;
        private SerializedProperty _enableInGameOverlayProp;

        private Vector2 _scrollPos;

        private TimeScaleSliderOnToolbarSettingProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            var instance = TimeScaleSliderOnToolbarSetting.instance;
            instance.hideFlags = HideFlags.HideAndDontSave & ~HideFlags.NotEditable;

            _serializedObject = new SerializedObject(instance);
            _isEnableProp = _serializedObject.FindProperty("isEnable");
            _displayModeProp = _serializedObject.FindProperty("displayMode");
            _toolbarZoneProp = _serializedObject.FindProperty("toolbarZone");
            _timeScaleArrayProp = _serializedObject.FindProperty("timeScaleArray");
            _muteAudioProp = _serializedObject.FindProperty("muteAudioOnFastForward");
            _autoResetProp = _serializedObject.FindProperty("autoResetOnPlayModeExit");
            _enableScrollWheelProp = _serializedObject.FindProperty("enableScrollWheel");
            _showConsoleLogProp = _serializedObject.FindProperty("showConsoleLog");
            _enableInGameOverlayProp = _serializedObject.FindProperty("enableInGameOverlay");
        }

        public override void OnGUI(string searchContext)
        {
            if (_serializedObject == null || _serializedObject.targetObject == null)
            {
                OnActivate(searchContext, null);
            }

            _serializedObject.Update();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Time Scale on Toolbar Settings", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Configure toolbar display mode, presets, audio protection, and mobile overlay.", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            // Group 1: Toolbar Appearance
            DrawSectionHeader("Toolbar Display");
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(_isEnableProp, new GUIContent("Enable On Toolbar"));
                EditorGUILayout.PropertyField(_displayModeProp, new GUIContent("Display Mode"));
                EditorGUILayout.PropertyField(_toolbarZoneProp, new GUIContent("Toolbar Zone"));
            }

            EditorGUILayout.Space(10);

            // Group 2: Presets & Controls
            DrawSectionHeader("Presets & Interactions");
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(_timeScaleArrayProp, new GUIContent("Presets Array"), true);
                EditorGUILayout.PropertyField(_enableScrollWheelProp, new GUIContent("Enable Mouse Scroll", "Scroll mouse wheel over toolbar pill to quickly step speed up or down."));
            }

            EditorGUILayout.Space(10);

            // Group 3: Behavior & Audio Protection
            DrawSectionHeader("Behavior & Audio");
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(_muteAudioProp, new GUIContent("Mute Audio On Fast-Forward", "Automatically mutes audio when Time.timeScale > 1.0 to protect ears."));
                EditorGUILayout.PropertyField(_autoResetProp, new GUIContent("Auto-Reset On Exit Play", "Resets Time.timeScale back to 1.0x when stopping Play Mode."));
                EditorGUILayout.PropertyField(_showConsoleLogProp, new GUIContent("Log To Console", "Log timescale changes to Unity Console."));
            }

            EditorGUILayout.Space(10);

            // Group 4: Runtime / Mobile QA
            DrawSectionHeader("Runtime / Mobile QA Debugger");
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(_enableInGameOverlayProp, new GUIContent("Enable In-Game Overlay", "Shows a floating timescale widget in Development Builds / Mobile for QA testers."));
            }

            EditorGUILayout.Space(20);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reset To Defaults", GUILayout.Width(140), GUILayout.Height(24)))
                {
                    if (EditorUtility.DisplayDialog("Reset Settings", "Are you sure you want to reset all TimeScale Toolbar settings to defaults?", "Reset", "Cancel"))
                    {
                        TimeScaleSliderOnToolbarSetting.instance.ResetToDefaults();
                        _serializedObject.Update();
                    }
                }
            }

            EditorGUILayout.EndScrollView();

            if (_serializedObject.ApplyModifiedProperties())
            {
                TimeScaleSliderOnToolbarSetting.instance.SaveAndNotify();
            }
        }

        private static void DrawSectionHeader(string title)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            var rect = GUILayoutUtility.GetRect(1f, 1f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
            EditorGUILayout.Space(4);
        }

        [SettingsProvider]
        private static SettingsProvider CreateSettingProvider()
        {
            return new TimeScaleSliderOnToolbarSettingProvider(PATH, SettingsScope.Project);
        }
    }
}