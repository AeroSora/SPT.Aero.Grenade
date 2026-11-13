using EFT;
using HarmonyLib;

namespace Aero.Grenade.Patches
{
    using Ctrl = Player.GrenadeHandsController;

    [HarmonyPatch(typeof(Ctrl.Idling), "Start", new System.Type[0])]
    public static class AutoPinPatch
    {
        [HarmonyPostfix]
        public static void Postfix(object __instance)
        {
            if (!GrenadeInputManager.EnhancedArmed) return;

            var controller = GrenadeAnim.GetController(__instance);
            if (controller == null || !GrenadeAnim.IsLocal(controller)) return;

            GrenadeThrowDriver.ReadyToThrow = true;

            var mode = Plugin.Cfg.Mode.Value;
            GrenadeThrowDriver.AutoThrowPending =
                mode == ThrowMode.QuickThrow ||
                (mode == ThrowMode.Hybrid && GrenadeInputManager.KeyIsHeld);

            Diagnostics.Verbose($"Ready reached. Mode={mode}, KeyHeld={GrenadeInputManager.KeyIsHeld}, autoThrow={GrenadeThrowDriver.AutoThrowPending}.");

            if (Plugin.Cfg.AutoPullPin.Value && !GrenadeAnim.NoPin(controller))
            {
                controller.FirearmsAnimator?.SetGrenadeFire(FirearmsAnimator.EGrenadeFire.Hold);
                Diagnostics.Verbose("Auto pull-pin: holding overhead (operation stays selectable).");
            }
        }
    }
}
