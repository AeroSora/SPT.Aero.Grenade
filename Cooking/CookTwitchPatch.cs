using EFT.Animations;
using HarmonyLib;

namespace Aero.Grenade.Patches
{
    [HarmonyPatch(typeof(ProceduralWeaponAnimation), "CalculateCameraPosition")]
    internal static class CookTwitchPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ProceduralWeaponAnimation __instance)
        {
            if (!CookTwitch.Active || __instance == null || __instance != CookTwitch.Pwa) return;

            var hc = __instance.HandsContainer;
            var wra = hc != null ? hc.WeaponRootAnim : null;
            if (wra == null) return;

            wra.localPosition += CookTwitch.PosOffset;
            wra.localRotation *= CookTwitch.RotOffset;
        }
    }
}
