using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using com.ktgame.utils.time_scale_on_toolbar;

namespace com.ktgame.utils.time_scale_on_toolbar.editor
{
    [InitializeOnLoad]
    internal static class TimeScaleSliderOnToolbar
    {
        private static float _lastReportedTimeScale = 1.0f;
        private static bool _wasMutedByFastForward;
        private static bool _originalAudioPause;

        private static readonly Color ColorActiveCyan = new Color(0.32f, 0.78f, 0.98f, 1f);
        private static readonly Color ColorPauseOrange = new Color(1.0f, 0.55f, 0.18f, 1f);

        static TimeScaleSliderOnToolbar()
        {
            RefreshRegistration();
            TimeScaleSliderOnToolbarSetting.OnSettingsChanged += RefreshRegistration;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += OnEditorUpdate;
        }

        private static void RefreshRegistration()
        {
            ToolbarExtender.LeftToolbarGUI.Remove(OnToolbarGUI);
            ToolbarExtender.RightToolbarGUI.Remove(OnToolbarGUI);

            var setting = TimeScaleSliderOnToolbarSetting.instance;
            if (!setting.IsEnable) return;

            if (setting.ToolbarZone == ToolbarZoneAlignment.Left)
            {
                ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
            }
            else
            {
                ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            var setting = TimeScaleSliderOnToolbarSetting.instance;
            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
            {
                if (setting.AutoResetOnPlayModeExit)
                {
                    TimeScaleManager.ResetTimeScale();
                }

                if (_wasMutedByFastForward)
                {
                    AudioListener.pause = _originalAudioPause;
                    _wasMutedByFastForward = false;
                }
            }
        }

        private static void OnEditorUpdate()
        {
            var setting = TimeScaleSliderOnToolbarSetting.instance;

            // Audio Mute logic during Fast-Forward
            if (setting.MuteAudioOnFastForward && EditorApplication.isPlaying)
            {
                bool shouldMute = Time.timeScale > 1.05f;
                if (shouldMute && !_wasMutedByFastForward)
                {
                    _originalAudioPause = AudioListener.pause;
                    AudioListener.pause = true;
                    _wasMutedByFastForward = true;
                }
                else if (!shouldMute && _wasMutedByFastForward)
                {
                    AudioListener.pause = _originalAudioPause;
                    _wasMutedByFastForward = false;
                }
            }

            // Repaint toolbar when timescale changes externally
            if (!Mathf.Approximately(_lastReportedTimeScale, Time.timeScale))
            {
                _lastReportedTimeScale = Time.timeScale;
                EditorApplication.delayCall += () =>
                {
                    var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
                    var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
                    if (toolbars.Length > 0 && toolbars[0] is ScriptableObject so)
                    {
                        EditorUtility.SetDirty(so);
                    }
                };
            }
        }

        private static void OnToolbarGUI()
        {
            var setting = TimeScaleSliderOnToolbarSetting.instance;
            if (!setting.IsEnable) return;

            switch (setting.DisplayMode)
            {
                case ToolbarDisplayMode.Compact:
                    DrawCompactMode(setting);
                    break;
                case ToolbarDisplayMode.Slider:
                    DrawSliderMode(setting);
                    break;
                case ToolbarDisplayMode.PresetsBar:
                    DrawPresetsBarMode(setting);
                    break;
            }
        }

        #region Compact Mode (~75px)

        private static void DrawCompactMode(TimeScaleSliderOnToolbarSetting setting)
        {
            using (new EditorGUILayout.HorizontalScope(GUILayout.Width(76)))
            {
                DrawPauseButton();
                DrawSpeedPill(setting);
            }
        }

        private static void DrawPauseButton()
        {
            bool isPaused = TimeScaleManager.IsPaused;
            var prevBg = GUI.backgroundColor;

            if (isPaused)
            {
                GUI.backgroundColor = ColorPauseOrange;
            }

            string icon = isPaused ? "▶" : "⏸";
            var content = new GUIContent(icon, isPaused ? "Resume TimeScale (Left-Click)\nStep 1 Frame (Right-Click)" : "Pause TimeScale (Left-Click)\nStep 1 Frame (Right-Click)");

            var rect = GUILayoutUtility.GetRect(content, EditorStyles.toolbarButton, GUILayout.Width(24));

            var e = Event.current;
            if (e.type == EventType.ContextClick && rect.Contains(e.mousePosition))
            {
                EditorApplication.Step();
                e.Use();
            }
            else if (GUI.Button(rect, content, EditorStyles.toolbarButton))
            {
                TimeScaleManager.TogglePause();
            }

            GUI.backgroundColor = prevBg;
        }

        private static void DrawSpeedPill(TimeScaleSliderOnToolbarSetting setting)
        {
            float current = Time.timeScale;
            bool isCustom = !Mathf.Approximately(current, 1.0f) && !TimeScaleManager.IsPaused;

            var prevBg = GUI.backgroundColor;
            if (isCustom)
            {
                GUI.backgroundColor = ColorActiveCyan;
            }

            string label = $"{current:0.##}x ▾";
            var content = new GUIContent(label, "Click: Presets & Slider\nDouble-Click / Middle-Click: Reset 1.0x\nScroll: Adjust Speed");

            var rect = GUILayoutUtility.GetRect(content, EditorStyles.toolbarDropDown, GUILayout.Width(52));

            HandleScrollWheel(rect, setting);

            var e = Event.current;

            // Middle click to reset 1x
            if (e.type == EventType.MouseDown && e.button == 2 && rect.Contains(e.mousePosition))
            {
                ApplyTimeScale(1.0f);
                e.Use();
            }
            // Double click to reset 1x
            else if (e.type == EventType.MouseDown && e.button == 0 && e.clickCount >= 2 && rect.Contains(e.mousePosition))
            {
                ApplyTimeScale(1.0f);
                e.Use();
            }
            // Normal click to open Dropdown
            else if (GUI.Button(rect, content, EditorStyles.toolbarDropDown))
            {
                ShowCompactMenu(rect, setting);
            }

            GUI.backgroundColor = prevBg;
        }

        private static void ShowCompactMenu(Rect buttonRect, TimeScaleSliderOnToolbarSetting setting)
        {
            var menu = new GenericMenu();
            float current = TimeScaleManager.Current;

            foreach (var preset in setting.TimeScaleArray)
            {
                string label = preset == 0 ? "Pause (0x)" : $"{preset:0.##}x";
                if (Mathf.Approximately(preset, 1.0f)) label += " (Normal)";
                bool isActive = Mathf.Approximately(current, preset);

                menu.AddItem(new GUIContent(label), isActive, () => ApplyTimeScale(preset));
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Custom Slider..."), false, () =>
            {
                CustomTimeScalePopup.Show(buttonRect, current);
            });

            menu.AddItem(new GUIContent("Step 1 Frame"), false, () =>
            {
                EditorApplication.Step();
            });

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Mute Audio on Fast-Forward"), setting.MuteAudioOnFastForward, () =>
            {
                setting.MuteAudioOnFastForward = !setting.MuteAudioOnFastForward;
            });

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Settings..."), false, () =>
            {
                SettingsService.OpenProjectSettings("Ktgame/Time Scale Slider On Toolbar");
            });

            menu.DropDown(buttonRect);
        }

        #endregion

        #region Presets Bar Mode

        private static void DrawPresetsBarMode(TimeScaleSliderOnToolbarSetting setting)
        {
            float current = Time.timeScale;
            var presets = setting.TimeScaleArray;
            float totalWidth = 26 + (presets.Count * 28);

            using (new EditorGUILayout.HorizontalScope(GUILayout.Width(totalWidth)))
            {
                DrawPauseButton();

                for (int i = 0; i < presets.Count; i++)
                {
                    float preset = presets[i];
                    if (Mathf.Approximately(preset, 0f)) continue; // 0x is already covered by Pause button

                    bool isActive = Mathf.Approximately(current, preset);
                    var prevBg = GUI.backgroundColor;
                    if (isActive)
                    {
                        GUI.backgroundColor = ColorActiveCyan;
                    }

                    string text = $"{preset:0.##}";
                    var rect = GUILayoutUtility.GetRect(new GUIContent(text), EditorStyles.toolbarButton, GUILayout.Width(28));

                    var e = Event.current;
                    if (e.type == EventType.ContextClick && rect.Contains(e.mousePosition))
                    {
                        ShowCompactMenu(rect, setting);
                        e.Use();
                    }
                    else if (GUI.Button(rect, text, EditorStyles.toolbarButton))
                    {
                        // Toggle back to 1.0x if already active, otherwise set preset
                        if (isActive && !Mathf.Approximately(preset, 1.0f))
                        {
                            ApplyTimeScale(1.0f);
                        }
                        else
                        {
                            ApplyTimeScale(preset);
                        }
                    }

                    GUI.backgroundColor = prevBg;
                }
            }
        }

        #endregion

        #region Slider Mode

        private static void DrawSliderMode(TimeScaleSliderOnToolbarSetting setting)
        {
            using (new EditorGUILayout.HorizontalScope(GUILayout.Width(140)))
            {
                DrawPauseButton();

                EditorGUI.BeginChangeCheck();
                float newScale = GUILayout.HorizontalSlider(Time.timeScale, 0f, 8f, GUILayout.Width(64));
                if (EditorGUI.EndChangeCheck())
                {
                    ApplyTimeScale(newScale);
                }

                bool isCustom = !Mathf.Approximately(Time.timeScale, 1.0f) && !TimeScaleManager.IsPaused;
                var prevBg = GUI.backgroundColor;
                if (isCustom) GUI.backgroundColor = ColorActiveCyan;

                if (GUILayout.Button($"{Time.timeScale:0.#}x", EditorStyles.toolbarButton, GUILayout.Width(44)))
                {
                    ApplyTimeScale(1.0f);
                }

                GUI.backgroundColor = prevBg;
            }
        }

        #endregion

        #region Helpers

        private static void HandleScrollWheel(Rect rect, TimeScaleSliderOnToolbarSetting setting)
        {
            if (!setting.EnableScrollWheel) return;

            var e = Event.current;
            if (e.type == EventType.ScrollWheel && rect.Contains(e.mousePosition))
            {
                float delta = e.delta.y;
                float current = TimeScaleManager.Current;
                float next = GetNextPreset(current, setting.TimeScaleArray, delta < 0);
                ApplyTimeScale(next);
                e.Use();
            }
        }

        private static float GetNextPreset(float current, IReadOnlyList<float> presets, bool increase)
        {
            if (increase)
            {
                for (int i = 0; i < presets.Count; i++)
                {
                    if (presets[i] > current + 0.05f) return presets[i];
                }
                return current + 1.0f;
            }
            else
            {
                for (int i = presets.Count - 1; i >= 0; i--)
                {
                    if (presets[i] < current - 0.05f) return presets[i];
                }
                return 0f;
            }
        }

        private static void ApplyTimeScale(float value)
        {
            TimeScaleManager.SetTimeScale(value);
            var setting = TimeScaleSliderOnToolbarSetting.instance;
            if (setting.ShowConsoleLog)
            {
                Debug.Log($"[TimeScaleToolbar] Time Scale set to: {value:0.##}x");
            }
        }

        #endregion
    }
}