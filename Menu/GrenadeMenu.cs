using System;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using UnityEngine;

namespace Aero.Grenade
{
    public static class GrenadeMenu
    {
        private static GrenadeSelector _selector;
        private static CanvasGroup _fadeGroup;
        private static float _fadeStart = -1f;
        private static bool _warmed;

        private static GrenadeSelector Selector
        {
            get
            {
                if (_selector == null) _selector = FindSelector();
                return _selector;
            }
        }

        private static GrenadeSelector FindSelector()
        {
            var all = Resources.FindObjectsOfTypeAll<GrenadeSelector>();
            if (all == null || all.Length == 0) return null;
            foreach (var s in all)
                if (s != null && s.gameObject.scene.IsValid())
                    return s;
            return all[0];
        }

        public static bool IsShown
        {
            get
            {
                var s = Selector;
                return s != null && s.IsShown;
            }
        }

        public static void Prewarm()
        {
            if (Singleton<GameWorld>.Instance?.MainPlayer == null)
            {
                _warmed = false;
                _selector = null;
                return;
            }
            if (_warmed) return;

            var sel = Selector;
            if (sel == null) return;
            try
            {
                sel.GetSortedGrenadeTemplateIds();
                _warmed = true;
                Diagnostics.Verbose("Grenade menu pre-warmed.");
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning($"[Aero.Grenade] Menu prewarm deferred: {e.Message}");
            }
        }

        public static bool Open(Player player)
        {
            var sel = Selector;
            if (sel == null) { Plugin.Log.LogWarning("[Aero.Grenade] GrenadeSelector not found — cannot open menu."); return false; }

            var inv = player.InventoryController;
            if (inv == null) return false;

            var pos = new Vector2(
                Screen.width * 0.5f + Plugin.Cfg.MenuOffsetX.Value,
                Screen.height * 0.5f - Plugin.Cfg.MenuOffsetY.Value);
            try
            {
                sel.ShowGrenades(null, inv, pos, null);
                sel.transform.localScale = Vector3.one * Plugin.Cfg.MenuScale.Value;
                StartFadeIn(sel);
                Diagnostics.Verbose("Grenade menu opened.");
                return true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"[Aero.Grenade] ShowGrenades failed: {e}");
                return false;
            }
        }

        public static void CycleNext()
        {
            var sel = Selector;
            if (sel != null && sel.IsShown) sel.SelectPrev();
        }

        public static EFT.InventoryLogic.ThrowWeap PickAndClose()
        {
            var sel = Selector;
            if (sel == null || !sel.IsShown) return null;
            var g = sel.GetGrenade();
            sel.Close();
            return g;
        }

        public static int EntryCount
        {
            get
            {
                var sel = Selector;
                if (sel == null) return 0;
                var list = Traverse.Create(sel).Field("_grenadeViews").GetValue() as System.Collections.ICollection;
                return list?.Count ?? 0;
            }
        }

        public static EFT.InventoryLogic.ThrowWeap SelectIndexAndClose(int index)
        {
            var sel = Selector;
            if (sel == null || !sel.IsShown) return null;
            if (index < 0 || index >= EntryCount) return null;

            sel.SelectedGrenadeIndex = index;
            var g = sel.GetGrenade();
            sel.Close();
            return g;
        }

        public static void CloseIfShown()
        {
            var sel = Selector;
            if (sel != null && sel.IsShown) sel.Close();
        }

        private static void StartFadeIn(GrenadeSelector sel)
        {
            _fadeGroup = sel.GetComponent<CanvasGroup>() ?? sel.gameObject.AddComponent<CanvasGroup>();
            _fadeGroup.alpha = 0f;
            _fadeStart = Time.unscaledTime;
        }

        public static void TickFade()
        {
            if (_fadeStart < 0f || _fadeGroup == null) return;
            if (!IsShown) { _fadeStart = -1f; return; }

            float dur = GrenadeTuning.MenuFadeIn;
            float a = dur <= 0f ? 1f : Mathf.Clamp01((Time.unscaledTime - _fadeStart) / dur);
            _fadeGroup.alpha = a;
            if (a >= 1f) _fadeStart = -1f;
        }
    }
}
