using System.Diagnostics;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace Aero.Grenade
{
    public static class Diagnostics
    {
        private static GUIStyle _cookStyle;

        internal static bool VerboseOn => false;

        [Conditional("AERO_DEV")]
        public static void Verbose(string msg)
        {
        }

        public static void DrawHud()
        {
            DrawCookTimer();
        }

        private static void DrawCookTimer()
        {
            if (Plugin.Cfg?.Indicator?.Value != CookIndicator.Timer || !GrenadeCook.Cooking) return;

            _cookStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.red },
            };

            float w = 260f, h = 48f;
            GUI.Label(new Rect((Screen.width - w) * 0.5f, Screen.height - 140f, w, h),
                $"COOKING {GrenadeCook.Elapsed:F1}s", _cookStyle);
        }
    }
}
