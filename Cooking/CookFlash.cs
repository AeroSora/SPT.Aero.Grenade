using System;
using System.Linq;
using EFT.UI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Aero.Grenade
{
    public static class CookFlash
    {
        private static BattleUIMalfunctionGlow _comp;
        private static Image _img;
        private static Sprite _redSprite, _greenSprite;

        private static float _t;
        private static bool _active;

        public static void Flash()
        {
            if (!Resolve()) { Diagnostics.Verbose("Cook flash: glow image not found."); return; }

            var sprite = Plugin.Cfg.CookFlashTone.Value == CookFlashColor.Green ? _greenSprite : _redSprite;
            if (sprite == null) sprite = _redSprite ?? _greenSprite;
            if (sprite != null) _img.sprite = sprite;

            _img.enabled = true;
            _t = 0f;
            _active = true;
        }

        public static void Tick()
        {
            if (!_active) return;
            if (_img == null) { _active = false; return; }

            _t += Time.unscaledDeltaTime;
            float dur = GrenadeTuning.CookFlashDuration;
            if (_t >= dur) { SetAlpha(0f); _img.enabled = false; _active = false; return; }

            float e = Mathf.Sin(Mathf.PI * (_t / dur)) * GrenadeTuning.CookFlashIntensity;
            SetAlpha(Mathf.Clamp01(e));
        }

        public static void Reset()
        {
            if (_img != null) { SetAlpha(0f); _img.enabled = false; }
            _active = false;
            _t = 0f;
        }

        private static void SetAlpha(float a)
        {
            _img.color = new Color(1f, 1f, 1f, a);
        }

        private static bool Resolve()
        {
            if (_comp != null && _img != null) return true;

            var all = Resources.FindObjectsOfTypeAll<BattleUIMalfunctionGlow>();
            if (all == null || all.Length == 0) return false;

            _comp = all.FirstOrDefault(g => g != null && g.gameObject.scene.IsValid()) ?? all[0];
            if (_comp == null) return false;

            _img = AccessTools.Field(typeof(BattleUIMalfunctionGlow), "_glow").GetValue(_comp) as Image;
            ExtractSprites();
            return _img != null;
        }

        private static void ExtractSprites()
        {
            try
            {
                var glows = AccessTools.Field(typeof(BattleUIMalfunctionGlow), "_glows").GetValue(_comp) as Array;
                if (glows == null) return;

                var gp = glows.GetType().GetElementType();
                var typeF = AccessTools.Field(gp, "Type");
                var glowF = AccessTools.Field(gp, "Glow");

                foreach (var elem in glows)
                {
                    var t = (BattleUIMalfunctionGlow.EGlowType)typeF.GetValue(elem);
                    var sprite = glowF.GetValue(elem) as Sprite;
                    if (t == BattleUIMalfunctionGlow.EGlowType.Examined) _redSprite = sprite;
                    else if (t == BattleUIMalfunctionGlow.EGlowType.Repaired) _greenSprite = sprite;
                }
            }
            catch (Exception e) { Diagnostics.Verbose($"Cook flash sprite extract failed: {e.Message}"); }
        }
    }
}
