using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace com.ktgame.utils.time_scale_on_toolbar.editor
{
    public enum ToolbarDisplayMode
    {
        [InspectorName("Compact (Space Saver ~75px)")]
        Compact,

        [InspectorName("Preset Buttons Bar")]
        PresetsBar,

        [InspectorName("Slider & Value")]
        Slider
    }

    public enum ToolbarZoneAlignment
    {
        Left,
        Right
    }

    [FilePath("UserSettings/Ktgame/TimeScaleSliderOnToolbarSetting.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class TimeScaleSliderOnToolbarSetting : ScriptableSingleton<TimeScaleSliderOnToolbarSetting>
    {
        private const string PREF_KEY_OVERLAY = "Ktgame_TimeScale_InGameOverlay";

        [SerializeField] private bool isEnable = true;
        [SerializeField] private ToolbarDisplayMode displayMode = ToolbarDisplayMode.Compact;
        [SerializeField] private ToolbarZoneAlignment toolbarZone = ToolbarZoneAlignment.Left;
        [SerializeField] private float[] timeScaleArray = { 0f, 0.25f, 0.5f, 1f, 1.5f, 2f, 4f, 8f };
        [SerializeField] private bool muteAudioOnFastForward = true;
        [SerializeField] private bool autoResetOnPlayModeExit = true;
        [SerializeField] private bool enableScrollWheel = true;
        [SerializeField] private bool showConsoleLog = false;
        [SerializeField] private bool enableInGameOverlay = true;

        public static event Action OnSettingsChanged;

        public bool IsEnable
        {
            get => isEnable;
            set { if (isEnable != value) { isEnable = value; SaveAndNotify(); } }
        }

        public ToolbarDisplayMode DisplayMode
        {
            get => displayMode;
            set { if (displayMode != value) { displayMode = value; SaveAndNotify(); } }
        }

        public ToolbarZoneAlignment ToolbarZone
        {
            get => toolbarZone;
            set { if (toolbarZone != value) { toolbarZone = value; SaveAndNotify(); } }
        }

        public IReadOnlyList<float> TimeScaleArray => timeScaleArray;

        public float[] EditableTimeScaleArray
        {
            get => timeScaleArray;
            set { timeScaleArray = value; SaveAndNotify(); }
        }

        public bool MuteAudioOnFastForward
        {
            get => muteAudioOnFastForward;
            set { if (muteAudioOnFastForward != value) { muteAudioOnFastForward = value; SaveAndNotify(); } }
        }

        public bool AutoResetOnPlayModeExit
        {
            get => autoResetOnPlayModeExit;
            set { if (autoResetOnPlayModeExit != value) { autoResetOnPlayModeExit = value; SaveAndNotify(); } }
        }

        public bool EnableScrollWheel
        {
            get => enableScrollWheel;
            set { if (enableScrollWheel != value) { enableScrollWheel = value; SaveAndNotify(); } }
        }

        public bool ShowConsoleLog
        {
            get => showConsoleLog;
            set { if (showConsoleLog != value) { showConsoleLog = value; SaveAndNotify(); } }
        }

        public bool EnableInGameOverlay
        {
            get => enableInGameOverlay;
            set
            {
                if (enableInGameOverlay != value)
                {
                    enableInGameOverlay = value;
                    PlayerPrefs.SetInt(PREF_KEY_OVERLAY, value ? 1 : 0);
                    PlayerPrefs.Save();
                    SaveAndNotify();
                }
            }
        }

        public void SaveAndNotify()
        {
            Save(true);
            OnSettingsChanged?.Invoke();
        }

        public void ResetToDefaults()
        {
            isEnable = true;
            displayMode = ToolbarDisplayMode.Compact;
            toolbarZone = ToolbarZoneAlignment.Left;
            timeScaleArray = new[] { 0f, 0.25f, 0.5f, 1f, 1.5f, 2f, 4f, 8f };
            muteAudioOnFastForward = true;
            autoResetOnPlayModeExit = true;
            enableScrollWheel = true;
            showConsoleLog = false;
            enableInGameOverlay = true;
            PlayerPrefs.SetInt(PREF_KEY_OVERLAY, 1);
            PlayerPrefs.Save();
            SaveAndNotify();
        }
    }
}