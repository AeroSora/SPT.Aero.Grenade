using System;
using EFT;
using HarmonyLib;

namespace Aero.Grenade.Patches
{
    using Ctrl  = Player.GrenadeHandsController;

    [HarmonyPatch(typeof(Ctrl), nameof(Ctrl.Spawn), new[] { typeof(float), typeof(Action) })]
    public static class DeploySpeedPatch
    {
        [HarmonyPrefix]
        public static void Prefix(Ctrl __instance, ref float animationSpeed)
        {
            if (!GrenadeAnim.IsLocal(__instance)) return;

            GrenadeAnim.ResetWeaponSpeed(__instance);

            float mult = Plugin.Cfg.Speed(AnimPart.Deploy);
            animationSpeed *= mult;
            GrenadeAnim.LastPhase = $"Deploy ×{mult:F2}";
            Diagnostics.Verbose($"Deploy animationSpeed ×{mult:F2} ({(GrenadeInputManager.EnhancedArmed ? "enhanced" : "vanilla")}).");
        }
    }

    [HarmonyPatch(typeof(Ctrl.HighThrowOperation), "Start", new Type[0])]
    public static class PinPullSpeedPatch
    {
        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            var controller = GrenadeAnim.GetController(__instance);
            if (controller == null || !GrenadeAnim.IsLocal(controller)) return;

            var part = (__instance is Ctrl.LowThrowOperation) ? AnimPart.UnderhandRepin : AnimPart.PinPull;
            GrenadeAnim.ApplyWeaponSpeed(controller, part);
        }
    }

    [HarmonyPatch(typeof(Ctrl.HighThrowOperation), nameof(Ctrl.HighThrowOperation.HighThrow))]
    public static class HighThrowSpeedPatch
    {
        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            var controller = GrenadeAnim.GetController(__instance);
            if (controller == null || !GrenadeAnim.IsLocal(controller)) return;
            GrenadeAnim.ApplyWeaponSpeed(controller, AnimPart.HighThrow);
        }
    }

    [HarmonyPatch(typeof(Ctrl.LowThrowOperation), nameof(Ctrl.LowThrowOperation.LowThrow))]
    public static class LowThrowSpeedPatch
    {
        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            var controller = GrenadeAnim.GetController(__instance);
            if (controller == null || !GrenadeAnim.IsLocal(controller)) return;
            GrenadeAnim.ApplyWeaponSpeed(controller, AnimPart.LowThrow);
        }
    }

    [HarmonyPatch(typeof(Ctrl.Remove), "Start", new[] { typeof(Action) })]
    public static class HolsterSpeedPatch
    {
        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            var controller = GrenadeAnim.GetController(__instance);
            if (controller == null || !GrenadeAnim.IsLocal(controller)) return;
            GrenadeAnim.ApplyWeaponSpeed(controller, AnimPart.Holster);
        }
    }
}
