using EFT;
using HarmonyLib;

namespace Aero.Grenade.Patches
{
    [HarmonyPatch(typeof(Player.BaseGrenadeHandsController), "ThrowGrenadeInner")]
    public static class ThrowStrengthPatch
    {
        [HarmonyPrefix]
        public static void Prefix(Player ____player, bool lowThrow, float timeSinceSafetyLevelRemoved, ref float forcePower)
        {
            if (____player == null || !____player.IsYourPlayer) return;
            float multiplier = Plugin.Cfg.ThrowForceMult.Value;
            if (multiplier != 1f) forcePower *= multiplier;
            Diagnostics.Verbose($"Throw ({(lowThrow ? "low" : "high")}): force {forcePower:F2}, fuse elapsed {timeSinceSafetyLevelRemoved:F2} s.");
        }
    }
}
