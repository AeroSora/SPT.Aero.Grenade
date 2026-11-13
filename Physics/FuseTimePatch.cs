using EFT;
using HarmonyLib;
using UnityEngine;

namespace Aero.Grenade.Patches
{
    [HarmonyPatch(typeof(EFT.InventoryLogic.ThrowWeap), nameof(EFT.InventoryLogic.ThrowWeap.GetExplDelay), MethodType.Getter)]
    public static class FuseTimePatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result)
        {
            float mult = Plugin.Cfg.FuseTimeMult.Value;
            if (mult != 1f)
                __result *= mult;
        }
    }
}
