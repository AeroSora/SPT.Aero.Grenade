using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;

namespace Aero.Grenade.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.TryProceed),
        new[] { typeof(Item), typeof(Callback<IHandsController>), typeof(bool) })]
    public static class GrenadeEquipPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(Player __instance, Item item)
        {
            if (!__instance.IsYourPlayer) return true;

            if (GrenadeInputManager.ShouldBlockAllEquip())
            {
                Diagnostics.Verbose($"Swallowed equip '{item?.ShortName}' (number-key grenade pick).");
                return false;
            }

            if (!(item is EFT.InventoryLogic.ThrowWeap throwWeap)) return true;

            GrenadeInputManager.RememberSelected(throwWeap);

            if (GrenadeInputManager.ShouldBlockGrenadeEquip())
            {
                Diagnostics.Verbose("Swallowed vanilla grenade equip (menu mode owns the key).");
                return false;
            }
            return true;
        }
    }
}
