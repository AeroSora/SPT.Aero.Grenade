using EFT;
using EFT.Animations;
using EFT.Communications;
using HarmonyLib;
using UnityEngine;

namespace Aero.Grenade
{
    public static class GrenadeCook
    {
        public static bool Cooking { get; private set; }
        private static float _startTime;
        private static int _lastCountdown = -1;
        private static bool _detonating;

        public static float Elapsed => Cooking ? Time.unscaledTime - _startTime : 0f;

        public static void Start(Player.GrenadeHandsController ctrl)
        {
            if (Cooking || !Plugin.Cfg.CookingEnabled.Value) return;
            if (GrenadeAnim.NoPin(ctrl))
            {
                Diagnostics.Verbose("Cook skipped — no-pin grenade (can't reinsert/hold a strip igniter).");
                return;
            }
            Cooking = true;
            _startTime = Time.unscaledTime;
            _lastCountdown = -1;
            Diagnostics.Verbose($"Cooking started. Fuse (GetExplDelay) = {SafeFuse(ctrl):F3}s.");
            CookCue.Play(ctrl);
            CookTwitch.Trigger(GetPwa(ctrl));
            if (Plugin.Cfg.Indicator.Value == CookIndicator.Text) Notify("Grenade cooked", Color.white);
        }

        public static void Reset() { Cooking = false; _lastCountdown = -1; _detonating = false; CookFlash.Reset(); }

        public static void Tick(Player.GrenadeHandsController ctrl)
        {
            if (!Cooking) return;
            if (ctrl == null) { Reset(); return; }

            float fuse = SafeFuse(ctrl);
            if (fuse <= 0f) return;

            float remaining = fuse - Elapsed;

            int top = Mathf.FloorToInt(fuse);
            int c = Mathf.CeilToInt(remaining);
            if (c != _lastCountdown)
            {
                if (c >= 1 && c <= top)
                {
                    if (Plugin.Cfg.Indicator.Value == CookIndicator.Text)
                    {
                        Color col = (remaining <= fuse * 0.5f) ? Color.red : Color.white;
                        Notify($"{c}...", col);
                    }

                    if (Plugin.Cfg.Indicator.Value == CookIndicator.Flash)
                        CookFlash.Flash();
                }
                _lastCountdown = c;
            }

            if (Plugin.Cfg.OverCookDetonates.Value && remaining <= 0f && !_detonating)
            {
                if (ForceDetonate(ctrl))
                {
                    _detonating = true;
                    Diagnostics.Verbose($"Over-cook (fuse {fuse:F2}s elapsed) — detonating.");
                }
            }
        }

        private static void Notify(string msg, Color color)
        {
            try
            {
                EFT.Communications.NotificationManager.DisplayMessageNotification(
                    msg, ENotificationDurationType.Default, ENotificationIconType.Alert, color);
            }
            catch (System.Exception e) { Diagnostics.Verbose($"Notify failed: {e.Message}"); }
        }

        private static bool ForceDetonate(Player.GrenadeHandsController ctrl)
        {
            if (ctrl.WaitingForHighThrow) { ctrl.HighThrow(); return true; }
            if (ctrl.WaitingForLowThrow)  { ctrl.LowThrow();  return true; }

            if (ctrl.CurrentOperation is Player.GrenadeHandsController.Idling)
            {
                ctrl.PullRingForHighThrow();
                return false;
            }
            return false;
        }

        private static float SafeFuse(Player.GrenadeHandsController ctrl)
        {
            try { return ctrl.Item?.GetExplDelay ?? 0f; }
            catch { return 0f; }
        }

        private static ProceduralWeaponAnimation GetPwa(Player.GrenadeHandsController ctrl)
        {
            try
            {
                var player = Traverse.Create(ctrl).Field("_player").GetValue<Player>();
                return player?.ProceduralWeaponAnimation;
            }
            catch { return null; }
        }
    }
}
