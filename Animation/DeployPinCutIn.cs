using Comfort.Common;
using EFT;
using UnityEngine;

namespace Aero.Grenade
{
    public static class DeployPinCutIn
    {
        private const int HandsLayer = 1;

        private const int DeployLastHash = -965243548;
        private const int PinPullHash    = 220670101;

        private static bool _done;

        public static void Tick()
        {
            if (Plugin.Cfg == null) { _done = false; return; }
            if (!GrenadeInputManager.EnhancedArmed) { _done = false; return; }

            var player = Singleton<GameWorld>.Instance?.MainPlayer;
            var ctrl = player?.HandsController as Player.GrenadeHandsController;
            var anim = ctrl?.FirearmsAnimator?.Animator;
            if (anim == null) { _done = false; return; }

            if (GrenadeAnim.NoPin(ctrl)) { _done = false; return; }

            if (GrenadeThrowDriver.ReadyToThrow) return;
            if (_done) return;

            var info = anim.GetCurrentAnimatorStateInfo(HandsLayer);
            if (info.fullPathHash != DeployLastHash) return;
            if (info.normalizedTime < GrenadeTuning.DeployEndPoint) return;

            float start = GrenadeTuning.PinPullStartPoint;
            float blend = GrenadeTuning.CutInBlend;
            var unity = GrenadeAnim.GetUnityAnimator(ctrl);
            if (unity != null && blend > 0f)
            {
                unity.CrossFade(PinPullHash, blend, HandsLayer, start);
                Diagnostics.Verbose($"Deploy→pin cut-in: crossfade pull-out (t={info.normalizedTime:F2}) → pin-pull@{start:F2}, blend {blend:F2}.");
            }
            else
            {
                anim.Play(PinPullHash, HandsLayer, start);
                Diagnostics.Verbose($"Deploy→pin cut-in: snap pull-out (t={info.normalizedTime:F2}) → pin-pull@{start:F2}.");
            }
            _done = true;
        }
    }
}
