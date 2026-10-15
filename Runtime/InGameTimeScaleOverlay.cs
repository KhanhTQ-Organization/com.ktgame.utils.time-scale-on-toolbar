#if DEVELOPMENT_BUILD || UNITY_EDITOR
using UnityEngine;

namespace com.ktgame.utils.time_scale_on_toolbar
{
    /// <summary>
    /// Lightweight floating in-game TimeScale debug overlay for Mobile/QA devices.
    /// Only compiled into Development Builds and Unity Editor.
    /// </summary>
    public sealed class InGameTimeScaleOverlay : MonoBehaviour
    {
        private const string PREF_KEY_ENABLED = "Ktgame_TimeScale_InGameOverlay";
        private static bool _initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (_initialized) return;

            // Check if user disabled in-game overlay
            if (PlayerPrefs.GetInt(PREF_KEY_ENABLED, 1) == 0) return;

            var go = new GameObject("[Ktgame_InGameTimeScaleOverlay]");
            DontDestroyOnLoad(go);
            go.AddComponent<InGameTimeScaleOverlay>();
            _initialized = true;
        }

        private Rect _windowRect;
        private bool _isExpanded;
        private bool _isDragging;
        private Vector2 _dragOffset;
        private GUIStyle _pillStyle;
        private GUIStyle _btnStyle;
        private GUIStyle _activeBtnStyle;
        private GUIStyle _boxStyle;

        private static readonly float[] DefaultPresets = { 0f, 0.5f, 1f, 2f, 4f, 8f };

        private void Awake()
        {
            float scale = GetScreenScale();
            float initialWidth = 70f * scale;
            float initialHeight = 32f * scale;
            _windowRect = new Rect(Screen.width - initialWidth - 16f, 16f, initialWidth, initialHeight);
        }

        private float GetScreenScale()
        {
            float dpi = Screen.dpi;
            if (dpi <= 0) dpi = 160f;
            return Mathf.Clamp(dpi / 160f, 1f, 3f);
        }

        private void InitStyles(float scale)
        {
            if (_pillStyle != null) return;

            int fontSize = Mathf.RoundToInt(13 * scale);

            _pillStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            _btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = fontSize,
                alignment = TextAnchor.MiddleCenter
            };

            _activeBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = fontSize,
                alignment = TextAnchor.MiddleCenter
            };
        }

        private void OnGUI()
        {
            float scale = GetScreenScale();
            InitStyles(scale);

            var originalBg = GUI.backgroundColor;
            var originalColor = GUI.color;

            if (!_isExpanded)
            {
                // Collapsed Pill Button
                float width = 64f * scale;
                float height = 28f * scale;
                _windowRect.width = width;
                _windowRect.height = height;

                KeepOnScreen();

                if (TimeScaleManager.IsPaused)
                {
                    GUI.backgroundColor = new Color(1f, 0.4f, 0.2f, 0.9f);
                }
                else if (!Mathf.Approximately(Time.timeScale, 1.0f))
                {
                    GUI.backgroundColor = new Color(0.2f, 0.8f, 1f, 0.9f);
                }
                else
                {
                    GUI.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.75f);
                }

                string label = TimeScaleManager.IsPaused ? "⏸ 0x" : $"⚡ {Time.timeScale:0.##}x";
                if (GUI.Button(_windowRect, label, _pillStyle))
                {
                    _isExpanded = true;
                }

                HandleDrag();
            }
            else
            {
                // Expanded Overlay Bar
                int count = DefaultPresets.Length + 1; // presets + close button
                float btnWidth = 44f * scale;
                float btnHeight = 32f * scale;
                float totalWidth = (btnWidth * count) + (8f * scale);
                float totalHeight = btnHeight + (8f * scale);

                _windowRect.width = totalWidth;
                _windowRect.height = totalHeight;

                KeepOnScreen();

                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
                GUI.Box(_windowRect, GUIContent.none, _boxStyle);

                float startX = _windowRect.x + (4f * scale);
                float currentY = _windowRect.y + (4f * scale);

                for (int i = 0; i < DefaultPresets.Length; i++)
                {
                    float preset = DefaultPresets[i];
                    Rect btnRect = new Rect(startX + (i * btnWidth), currentY, btnWidth - (2f * scale), btnHeight);

                    bool isActive = Mathf.Approximately(Time.timeScale, preset);
                    if (isActive)
                    {
                        GUI.backgroundColor = preset == 0 ? new Color(1f, 0.4f, 0.2f, 1f) : new Color(0.2f, 0.8f, 1f, 1f);
                    }
                    else
                    {
                        GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.85f);
                    }

                    string btnText = preset == 0 ? "⏸" : $"{preset:0.##}x";
                    if (GUI.Button(btnRect, btnText, isActive ? _activeBtnStyle : _btnStyle))
                    {
                        if (preset == 0)
                        {
                            TimeScaleManager.TogglePause();
                        }
                        else
                        {
                            TimeScaleManager.SetTimeScale(preset);
                        }
                    }
                }

                // Close button [✕]
                Rect closeRect = new Rect(startX + (DefaultPresets.Length * btnWidth), currentY, btnWidth - (2f * scale), btnHeight);
                GUI.backgroundColor = new Color(0.6f, 0.2f, 0.2f, 0.9f);
                if (GUI.Button(closeRect, "✕", _btnStyle))
                {
                    _isExpanded = false;
                }

                HandleDrag();
            }

            GUI.backgroundColor = originalBg;
            GUI.color = originalColor;
        }

        private void HandleDrag()
        {
            var e = Event.current;
            if (e == null) return;

            if (e.type == EventType.MouseDown && _windowRect.Contains(e.mousePosition))
            {
                _isDragging = true;
                _dragOffset = e.mousePosition - _windowRect.position;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && _isDragging)
            {
                _windowRect.position = e.mousePosition - _dragOffset;
                KeepOnScreen();
                e.Use();
            }
            else if (e.type == EventType.MouseUp && _isDragging)
            {
                _isDragging = false;
                e.Use();
            }
        }

        private void KeepOnScreen()
        {
            _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Screen.width - _windowRect.width);
            _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Screen.height - _windowRect.height);
        }
    }
}
#endif
