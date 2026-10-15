using System;
using UnityEngine;

namespace com.ktgame.utils.time_scale_on_toolbar
{
    /// <summary>
    /// Core manager for controlling game time scale safely at runtime and in editor.
    /// </summary>
    public static class TimeScaleManager
    {
        private static float _previousTimeScale = 1.0f;

        /// <summary>
        /// Invoked whenever the time scale is changed through TimeScaleManager.
        /// </summary>
        public static event Action<float> OnTimeScaleChanged;

        /// <summary>
        /// Gets or sets the current Time.timeScale.
        /// </summary>
        public static float Current
        {
            get => Time.timeScale;
            set => SetTimeScale(value);
        }

        /// <summary>
        /// Whether the game time is currently paused (timeScale == 0).
        /// </summary>
        public static bool IsPaused => Mathf.Approximately(Time.timeScale, 0f);

        /// <summary>
        /// The previous non-zero time scale before pausing (defaults to 1.0).
        /// </summary>
        public static float PreviousTimeScale => _previousTimeScale > 0.001f ? _previousTimeScale : 1.0f;

        /// <summary>
        /// Sets a new time scale value safely (clamped to >= 0).
        /// </summary>
        public static void SetTimeScale(float value)
        {
            value = Mathf.Max(0f, value);

            if (!Mathf.Approximately(Time.timeScale, 0f))
            {
                _previousTimeScale = Time.timeScale;
            }

            Time.timeScale = value;
            OnTimeScaleChanged?.Invoke(value);
        }

        /// <summary>
        /// Pauses the game (timeScale = 0) while remembering the previous speed.
        /// </summary>
        public static void Pause()
        {
            if (!IsPaused)
            {
                _previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                OnTimeScaleChanged?.Invoke(0f);
            }
        }

        /// <summary>
        /// Resumes the game to the previous time scale speed.
        /// </summary>
        public static void Resume()
        {
            if (IsPaused)
            {
                var target = _previousTimeScale > 0.001f ? _previousTimeScale : 1.0f;
                Time.timeScale = target;
                OnTimeScaleChanged?.Invoke(target);
            }
        }

        /// <summary>
        /// Toggles between Pause and Resume.
        /// </summary>
        public static void TogglePause()
        {
            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        /// <summary>
        /// Resets the time scale back to 1.0x normal speed.
        /// </summary>
        public static void ResetTimeScale()
        {
            SetTimeScale(1.0f);
        }
    }
}
