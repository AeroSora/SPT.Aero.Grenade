using System.Linq;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

namespace Aero.Grenade
{
    public static class GrenadeInputManager
    {
        public static bool  KeyIsHeld     { get; private set; }
        public static float PressTime     { get; private set; } = -1f;
        public static bool  EnhancedArmed { get; private set; }

        private static EFT.InventoryLogic.ThrowWeap _lastSelected;
        private static float _blockVanillaEquipUntil = -1f;
        private static float _blockAllEquipUntil = -1f;

        private static bool _menuOpenedByUs;

        internal static bool MenuOpenedByUs => _menuOpenedByUs;

        private const float ARMED_TIMEOUT = 5f;
        private static float _armedAt = -1f;
        private static bool  _sawGrenade;

        private static bool _holdConsumed;

        public static void Tick()
        {
            if (!Plugin.Cfg.EnhancedEnabled.Value)
            {
                if (KeyIsHeld) KeyIsHeld = false;
                if (EnhancedArmed) ClearArm();
                return;
            }

            if (_menuOpenedByUs)
            {
                if (!GrenadeMenu.IsShown)
                {
                    _menuOpenedByUs = false;
                }
                else
                {
                    _blockAllEquipUntil = Time.unscaledTime + 0.25f;

                    if (Plugin.Cfg.NumberKeysPick.Value && !EnhancedArmed)
                    {
                        int idx = ReadNumberKey();
                        if (idx >= 0) PickIndexAndArm(idx);
                    }
                }
            }

            var key = Plugin.Cfg.GrenadeKey.Value;
            bool down = key.MainKey != KeyCode.None && Input.GetKey(key.MainKey);

            if (down && !KeyIsHeld)
            {
                KeyIsHeld = true;
                PressTime = Time.unscaledTime;
                _holdConsumed = false;

                if (Plugin.Cfg.TapOpensMenu.Value)
                    _blockVanillaEquipUntil = Time.unscaledTime + (Plugin.Cfg.HoldThresholdMs.Value / 1000f) + 0.3f;

                Diagnostics.Verbose("Grenade key pressed.");
            }

            if (down && KeyIsHeld && !_holdConsumed)
            {
                float heldMs = (Time.unscaledTime - PressTime) * 1000f;
                if (heldMs >= Plugin.Cfg.HoldThresholdMs.Value)
                {
                    _holdConsumed = true;
                    if (GrenadeMenu.IsShown)
                        PickFromMenuAndArm(heldMs);
                    else if (!EnhancedArmed)
                        TryArmEnhanced(heldMs);
                }
            }

            if (!down && KeyIsHeld)
            {
                KeyIsHeld = false;
                bool wasTap = !_holdConsumed;
                _holdConsumed = false;
                Diagnostics.Verbose("Grenade key released.");

                if (wasTap && !EnhancedArmed && Plugin.Cfg.TapOpensMenu.Value)
                {
                    if (GrenadeMenu.IsShown)
                    {
                        GrenadeMenu.CycleNext();
                        Diagnostics.Verbose("Tap → cycle grenade menu.");
                    }
                    else
                    {
                        OpenMenu();
                    }
                }
            }

            ExpireStaleArm();
        }

        private static void OpenMenu()
        {
            var player = Singleton<GameWorld>.Instance?.MainPlayer;
            if (player == null || !player.HealthController.IsAlive) return;
            if (player.HandsController is Player.BaseGrenadeHandsController) return;

            if (GrenadeMenu.Open(player))
            {
                _menuOpenedByUs = true;
                Diagnostics.Verbose("Tap → opened grenade selection menu.");
            }
        }

        private static void PickIndexAndArm(int index)
        {
            int count = GrenadeMenu.EntryCount;
            if (index > count) return;

            if (index == count)
            {
                GrenadeMenu.CloseIfShown();
                _menuOpenedByUs = false;
                Diagnostics.Verbose($"Number {index + 1} → Cancel slot, menu closed.");
                return;
            }

            var picked = GrenadeMenu.SelectIndexAndClose(index);
            _menuOpenedByUs = false;
            if (picked == null)
            {
                Diagnostics.Verbose($"Number {index + 1}: no grenade at that position.");
                return;
            }

            _blockAllEquipUntil = Time.unscaledTime + 0.3f;

            _lastSelected = picked;
            _holdConsumed = true;
            Diagnostics.Verbose($"Number {index + 1} → '{picked.ShortName}' → enhanced sequence.");
            TryArmEnhanced(Plugin.Cfg.HoldThresholdMs.Value);
        }

        private static int ReadNumberKey()
        {
            for (int i = 0; i < 9; i++)
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                    return i;
            return -1;
        }

        private static void PickFromMenuAndArm(float heldMs)
        {
            var picked = GrenadeMenu.PickAndClose();
            _menuOpenedByUs = false;
            if (picked == null)
            {
                Diagnostics.Verbose("Menu hold on Cancel → closed, no throw.");
                return;
            }

            _lastSelected = picked;
            Diagnostics.Verbose($"Menu pick '{picked.ShortName}' → enhanced sequence.");
            TryArmEnhanced(heldMs);
        }

        private static void TryArmEnhanced(float heldMs)
        {
            var player = Singleton<GameWorld>.Instance?.MainPlayer;
            if (player == null || !player.HealthController.IsAlive)
                return;

            if (player.HandsController is Player.BaseGrenadeHandsController)
            {
                EnhancedArmed = true;
                _armedAt = Time.unscaledTime;
                _sawGrenade = true;
                Diagnostics.Verbose("Hold threshold met but grenade already in hands.");
                return;
            }

            var grenade = ResolveThrowable(player);
            if (grenade == null)
            {
                Diagnostics.Verbose("Hold threshold met but no throwable resolved — skipping.");
                return;
            }

            EnhancedArmed = true;
            _armedAt = Time.unscaledTime;
            _sawGrenade = false;
            Diagnostics.Verbose($"Enhanced ARMED at {heldMs:F0}ms — equipping '{grenade.ShortName}' directly (no release).");

            EquipDirect(player, grenade);
        }

        private static void EquipDirect(Player player, EFT.InventoryLogic.ThrowWeap grenade)
        {
            _blockVanillaEquipUntil = Time.unscaledTime + 0.6f;
            try
            {
                player.SetInHands(grenade, new Callback<EFT.IGrenadeController>(OnEquipResult));
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError($"[Aero.Grenade] Direct SetInHands threw: {e}");
            }
        }

        private static void OnEquipResult(Result<EFT.IGrenadeController> result)
        {
            if (string.IsNullOrEmpty(result.Error))
                Diagnostics.Verbose("Direct equip succeeded — grenade in hands while key still held.");
            else
                Plugin.Log.LogWarning($"[Aero.Grenade] Direct equip failed: {result.Error}");
        }

        private static EFT.InventoryLogic.ThrowWeap ResolveThrowable(Player player)
        {
            var invController = player.InventoryController;
            var equipment = invController?.Inventory?.Equipment;
            if (equipment == null) return null;

            var throwables = EFT.InventoryLogic.ItemExtensions.GetAllItems(equipment).OfType<EFT.InventoryLogic.ThrowWeap>().ToList();

            if (_lastSelected != null && !throwables.Contains(_lastSelected))
                _lastSelected = null;

            if (_lastSelected != null && SafeCanThrow(player, _lastSelected))
                return _lastSelected;

            return throwables.FirstOrDefault(g => SafeCanThrow(player, g))
                ?? throwables.FirstOrDefault();
        }

        private static bool SafeCanThrow(Player player, EFT.InventoryLogic.ThrowWeap item)
        {
            try { return player.InventoryController.CanThrow(item); }
            catch { return false; }
        }

        public static void RememberSelected(EFT.InventoryLogic.ThrowWeap throwWeap)
        {
            if (throwWeap != null) _lastSelected = throwWeap;
        }

        public static bool GrenadeKeyDownNow()
        {
            var key = Plugin.Cfg.GrenadeKey.Value;
            return key.MainKey != KeyCode.None && Input.GetKey(key.MainKey);
        }

        public static bool ShouldBlockAllEquip() => Time.unscaledTime < _blockAllEquipUntil;

        public static bool ShouldBlockGrenadeEquip()
        {
            if (Time.unscaledTime < _blockVanillaEquipUntil) return true;
            if (Plugin.Cfg.TapOpensMenu.Value)
            {
                var key = Plugin.Cfg.GrenadeKey.Value;
                if (key.MainKey != KeyCode.None && Input.GetKey(key.MainKey)) return true;
            }
            return false;
        }

        public static void ClearArm()
        {
            EnhancedArmed = false;
            _armedAt = -1f;
            _sawGrenade = false;
        }

        private static void ExpireStaleArm()
        {
            if (!EnhancedArmed) return;

            var player = Singleton<GameWorld>.Instance?.MainPlayer;
            bool grenadeOut = player?.HandsController is Player.BaseGrenadeHandsController;

            if (grenadeOut) { _sawGrenade = true; return; }

            if (_sawGrenade)
            {
                Diagnostics.Verbose("Grenade left hands — clearing enhanced arm.");
                ClearArm();
                return;
            }

            if (!KeyIsHeld && Time.unscaledTime - _armedAt > ARMED_TIMEOUT)
            {
                Diagnostics.Verbose("Enhanced arm expired (no grenade appeared).");
                ClearArm();
            }
        }
    }
}
