using EFT;
using HarmonyLib;
using UnityEngine;

namespace Aero.Grenade
{
    public static class GrenadeAnim
    {
        public static string LastPhase = "—";

        public static Player.GrenadeHandsController GetController(object opInstance)
        {
            if (opInstance == null) return null;
            return Traverse.Create(opInstance).Field("Controller") .GetValue<Player.GrenadeHandsController>();
        }

        public static bool NoPin(Player.GrenadeHandsController ctrl)
        {
            try { return ctrl?.Item != null && !ctrl.Item.CanBeHiddenDuringThrow; }
            catch { return false; }
        }

        public static bool IsLocal(Player.BaseGrenadeHandsController controller)
        {
            var player = Traverse.Create(controller).Field("_player").GetValue<Player>();
            return player != null && player.IsYourPlayer;
        }

        public static Animator GetUnityAnimator(Player.BaseGrenadeHandsController controller)
        {
            var anim = controller?.FirearmsAnimator?.Animator;
            if (anim == null) return null;
            try
            {
                var t = Traverse.Create(anim);
                var ua = t.Property("Animator").GetValue<Animator>();
                if (ua == null) ua = t.Field("Animator_0").GetValue<Animator>();
                return ua;
            }
            catch { return null; }
        }

        public static void ResetWeaponSpeed(Player.BaseGrenadeHandsController controller)
        {
            var anim = controller?.FirearmsAnimator?.Animator;
            if (anim != null) anim.speed = 1f;
        }

        public static void ApplyWeaponSpeed(Player.BaseGrenadeHandsController controller, AnimPart part)
        {
            var anim = controller?.FirearmsAnimator?.Animator;
            if (anim == null) return;

            float mult = Plugin.Cfg.Speed(part);
            anim.speed = mult;
            LastPhase = $"{part} ×{mult:F2}";
            Diagnostics.Verbose($"Anim speed: {LastPhase} ({(GrenadeInputManager.EnhancedArmed ? "enhanced" : "vanilla")}).");
        }
    }
}
