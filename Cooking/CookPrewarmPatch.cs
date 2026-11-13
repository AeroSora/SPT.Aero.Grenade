using EFT;
using HarmonyLib;

namespace Aero.Grenade.Patches
{
    [HarmonyPatch(typeof(Player.BaseGrenadeHandsController), "Throw")]
    public static class CookPrewarmPatch
    {
        [HarmonyPrefix]
        public static void Prefix(Player.BaseGrenadeHandsController __instance, ref float timeSinceSafetyLevelRemoved)
        {
            if (!GrenadeCook.Cooking) return;
            if (!GrenadeAnim.IsLocal(__instance)) return;

            timeSinceSafetyLevelRemoved = GrenadeCook.Elapsed;
            Diagnostics.Verbose($"Cook prewarm injected: {timeSinceSafetyLevelRemoved:F2}s.");

            FikaCompat.SendCook?.Invoke(__instance, GrenadeCook.Elapsed);

            GrenadeCook.Reset();
        }
    }
}
