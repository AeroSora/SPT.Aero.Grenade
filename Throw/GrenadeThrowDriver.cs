using Comfort.Common;
using EFT;
using UnityEngine;

namespace Aero.Grenade
{
    public static class GrenadeThrowDriver
    {
        private enum Phase { Idle, PinningHigh, PinningLow }
        private static Phase _phase = Phase.Idle;
        private static bool _prevLmb, _prevRmb;
        private static float _lowPinTime;

        public static bool ReadyToThrow;

        public static bool AutoThrowPending;
        public static string Status = "—";

        public static void Tick()
        {
            bool lmb = Input.GetMouseButton(0);
            bool rmb = Input.GetMouseButton(1);
            bool lmbDown = lmb && !_prevLmb;
            bool rmbDown = rmb && !_prevRmb;
            _prevLmb = lmb;
            _prevRmb = rmb;

            var player = Singleton<GameWorld>.Instance?.MainPlayer;
            var ctrl = player?.HandsController as Player.GrenadeHandsController;

            if (ctrl == null || !GrenadeInputManager.EnhancedArmed)
            {
                _phase = Phase.Idle;
                ReadyToThrow = false;
                AutoThrowPending = false;
                Status = "—";
                GrenadeCook.Reset();
                return;
            }

            GrenadeCook.Tick(ctrl);

            if (_phase == Phase.PinningHigh)
            {
                if (ctrl.WaitingForHighThrow)
                {
                    ctrl.HighThrow();
                    _phase = Phase.Idle; Status = "threw high"; Diagnostics.Verbose("Manual HighThrow.");
                }
                return;
            }
            if (_phase == Phase.PinningLow)
            {
                var lowAnim = ctrl.FirearmsAnimator?.Animator;
                if (lowAnim != null) lowAnim.speed = GrenadeTuning.UnderhandRepinSpeed;

                bool repinDone = Time.unscaledTime - _lowPinTime >= GrenadeTuning.UnderhandRepinHold;
                if (repinDone && ctrl.WaitingForLowThrow)
                {
                    ctrl.LowThrow(); _phase = Phase.Idle; Status = "threw low"; Diagnostics.Verbose("Manual LowThrow.");
                }
                return;
            }

            if (!ReadyToThrow) { Status = "deploying"; return; }

            if (AutoThrowPending)
            {
                AutoThrowPending = false;
                ctrl.PullRingForHighThrow();
                _phase = Phase.PinningHigh;
                Status = "auto-throw";
                Diagnostics.Verbose("Auto-throw → PullRingForHighThrow.");
                return;
            }

            Status = GrenadeCook.Cooking ? $"cooking {GrenadeCook.Elapsed:F1}s" : "ready";

            bool interactiveMode = Plugin.Cfg.Mode.Value != ThrowMode.QuickThrow;

            if (interactiveMode && Plugin.Cfg.CookingEnabled.Value && !GrenadeAnim.NoPin(ctrl)
                && CookButtonPressed() && !GrenadeCook.Cooking)
            {
                GrenadeCook.Start(ctrl);
                return;
            }

            if (lmbDown)
            {
                ctrl.PullRingForHighThrow();
                _phase = Phase.PinningHigh;
                Status = "pinning high";
                Diagnostics.Verbose("LMB → PullRingForHighThrow.");
            }
            else if (rmbDown && interactiveMode && Plugin.Cfg.EnhancedUnderhandThrow.Value)
            {
                if (GrenadeAnim.NoPin(ctrl))
                {
                    ctrl.PullRingForHighThrow();
                    _phase = Phase.PinningHigh;
                    Status = "no-pin → high";
                    Diagnostics.Verbose("RMB on no-pin grenade → overhand (re-pin disabled).");
                }
                else
                {
                    ctrl.FirearmsAnimator?.SetGrenadeFire(FirearmsAnimator.EGrenadeFire.Idle);
                    ctrl.PullRingForLowThrow();
                    _phase = Phase.PinningLow;
                    _lowPinTime = Time.unscaledTime;
                    Status = "pinning low";
                    Diagnostics.Verbose("RMB → PullRingForLowThrow.");
                }
            }
        }

        private static bool CookButtonPressed()
        {
            var sc = Plugin.Cfg.CookButton.Value;
            if (sc.MainKey != KeyCode.None)
                return Input.GetKeyDown(sc.MainKey);
            return sc.IsDown();
        }
    }
}
