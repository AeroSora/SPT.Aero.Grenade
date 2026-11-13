using EFT.InputSystem;
using EFT.UI;
using HarmonyLib;

namespace Aero.Grenade.Patches
{
    [HarmonyPatch(typeof(GrenadeSelector), nameof(GrenadeSelector.TranslateCommand))]
    public static class SelectorInputPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ECommand command, ref InputNode.ETranslateResult __result)
        {
            if (!GrenadeInputManager.MenuOpenedByUs) return true;
            if (command != ECommand.ThrowGrenade && command != ECommand.PressThrowGrenade) return true;

            __result = InputNode.ETranslateResult.Block;
            Diagnostics.Verbose($"Grenade menu: EFT {command} swallowed while our menu is open.");
            return false;
        }
    }
}
