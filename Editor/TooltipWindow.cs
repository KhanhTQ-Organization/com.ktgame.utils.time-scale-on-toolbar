using UnityEditor;
using UnityEngine;

namespace com.ktgame.utils.time_scale_on_toolbar.editor
{
    public static class TooltipWindow
    {
        public static void Open
        (
            string text,
            float time = 2,
            int fontSize = 16
        )
        {
            var pos = Event.current != null ? Event.current.mousePosition : new Vector2(Screen.width / 2f, 100f);
            var activatorRect = new Rect(pos, Vector2.one);
            var content = new TooltipWindowContent(text, time, fontSize);

            PopupWindow.Show(activatorRect, content);
        }
    }
}