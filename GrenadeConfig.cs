using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace Aero.Grenade
{
    public enum AnimPart { Deploy, PinPull, HighThrow, LowThrow, Holster, UnderhandRepin }

    public enum CookFlashColor { Red, Green }

    public enum CookIndicator
    {
        Flash,

        Text,

        Timer,
        None,
    }

    public enum ThrowMode
    {
        Normal,

        QuickThrow,

        Hybrid,
    }

    public class GrenadeConfig
    {
        private const string S_THROW = "1. Throwing";
        private const string S_ANIM = "2. Animation";
        private const string S_COOK = "3. Cooking";
        private const string S_MENU = "4. Grenade Menu";
        private const string S_PHYS = "5. Physics";

        public ConfigEntry<bool> EnhancedEnabled { get; }
        public ConfigEntry<KeyboardShortcut> GrenadeKey { get; }
        public ConfigEntry<int> HoldThresholdMs { get; }
        public ConfigEntry<ThrowMode> Mode { get; }
        public ConfigEntry<bool> AutoPullPin { get; }
        public ConfigEntry<bool> EnhancedUnderhandThrow { get; }
        public ConfigEntry<bool> TapOpensMenu { get; }

        public ConfigEntry<float> HoldSpeed { get; }
        public ConfigEntry<float> TapSpeed { get; }
        public ConfigEntry<float> TapHolsterSpeed { get; }

        public ConfigEntry<bool> CookingEnabled { get; }
        public ConfigEntry<KeyboardShortcut> CookButton { get; }
        public ConfigEntry<bool> OverCookDetonates { get; }
        public ConfigEntry<CookIndicator> Indicator { get; }
        public ConfigEntry<CookFlashColor> CookFlashTone { get; }
        public ConfigEntry<float> CookCueVolume { get; }
        public ConfigEntry<bool> CookTwitchEnabled { get; }

        public ConfigEntry<float> MenuScale { get; }
        public ConfigEntry<float> MenuOffsetX { get; }
        public ConfigEntry<float> MenuOffsetY { get; }
        public ConfigEntry<bool> NumberKeysPick { get; }

        public ConfigEntry<float> ThrowForceMult { get; }
        public ConfigEntry<float> FuseTimeMult { get; }

        public GrenadeConfig(ConfigFile cfg)
        {
            EnhancedEnabled = cfg.Bind(S_THROW, "Enhanced throw", true, Desc(
                "Holding the grenade key draws a grenade, pulls the pin and readies it in one motion; a tap still works as in the game.", 7));
            GrenadeKey = cfg.Bind(S_THROW, "Grenade key", new KeyboardShortcut(KeyCode.G), Desc(
                "Tap for the game's grenade handling, hold for the enhanced throw.", 6));
            HoldThresholdMs = cfg.Bind(S_THROW, "Hold time to enhance (ms)", 250, Desc(
                "How long the grenade key must be held before the enhanced throw starts, in milliseconds. (50 to 1000)", 5, new AcceptableValueRange<int>(50, 1000)));
            Mode = cfg.Bind(S_THROW, "Throw mode", ThrowMode.Hybrid, Desc(
                "Normal: the grenade waits at ready for a left or right click or a cook. QuickThrow: it is thrown overhand the moment it is ready. Hybrid: keep holding the key to throw at ready, release it early to wait as in Normal.", 4));
            AutoPullPin = cfg.Bind(S_THROW, "Auto pull pin", true, Desc(
                "The enhanced throw pulls the pin and holds the grenade overhead, ready to throw.", 3));
            EnhancedUnderhandThrow = cfg.Bind(S_THROW, "Underhand throw on right click", true, Desc(
                "Right click on a readied grenade throws it underhand.", 2));
            TapOpensMenu = cfg.Bind(S_THROW, "Tap opens the grenade menu", true, Desc(
                "A quick tap of the grenade key opens the grenade selection menu.", 1));

            HoldSpeed = cfg.Bind(S_ANIM, "Animation speed, hold", 1.4f, Desc(
                "Speed of the draw, pin pull, throws and put-away during the enhanced throw. 1 is the game's speed. (0.5 to 3)", 3, new AcceptableValueRange<float>(0.5f, 3f)));
            TapSpeed = cfg.Bind(S_ANIM, "Animation speed, tap", 1.2f, Desc(
                "Speed of the draw and pin pull after a tap; the throws keep their proportion to it (1.0 at 1.2). 1 is the game's speed. (0.5 to 3)", 2, new AcceptableValueRange<float>(0.5f, 3f)));
            TapHolsterSpeed = cfg.Bind(S_ANIM, "Put-away speed, tap", 1f, Desc(
                "Speed of putting the grenade away after a tap. 1 is the game's speed. (0.5 to 3)", 1, new AcceptableValueRange<float>(0.5f, 3f)));

            CookingEnabled = cfg.Bind(S_COOK, "Cooking", true, Desc(
                "Press the cook key while holding a readied grenade to start its fuse before throwing.", 7));
            CookButton = cfg.Bind(S_COOK, "Cook key", new KeyboardShortcut(KeyCode.F), Desc(
                "Starts cooking the readied grenade.", 6));
            OverCookDetonates = cfg.Bind(S_COOK, "Over-cooked grenades explode in hand", true, Desc(
                "A grenade cooked past its fuse time explodes in your hand.", 5));
            Indicator = cfg.Bind(S_COOK, "Cook indicator", CookIndicator.Flash, Desc(
                "Flash: the screen's malfunction glow flashes every second of the cook. Text: a message and countdown. Timer: a timer at the bottom of the screen. None: no indicator.", 4));
            CookFlashTone = cfg.Bind(S_COOK, "Cook flash color", CookFlashColor.Red, Desc(
                "Which malfunction glow the flash uses: Red (malfunction found) or Green (malfunction cleared).", 3));
            CookCueVolume = cfg.Bind(S_COOK, "Cook sound volume", 1f, Desc(
                "Volume of the spoon and fuse sounds that play when cooking starts. (0 to 2)", 2, new AcceptableValueRange<float>(0f, 2f)));
            CookTwitchEnabled = cfg.Bind(S_COOK, "Cook twitch", true, Desc(
                "The hand twitches briefly when the spoon releases.", 1));

            MenuScale = cfg.Bind(S_MENU, "Menu size", 1f, Desc(
                "Size of the grenade selection menu. 1 is the game's size. (0.3 to 3)", 4, new AcceptableValueRange<float>(0.3f, 3f)));
            MenuOffsetX = cfg.Bind(S_MENU, "Menu position X", 150f, Desc(
                "Horizontal offset of the menu from the screen centre, in pixels; positive is right. (-960 to 960)", 3, new AcceptableValueRange<float>(-960f, 960f)));
            MenuOffsetY = cfg.Bind(S_MENU, "Menu position Y", -150f, Desc(
                "Vertical offset of the menu from the screen centre, in pixels; positive is up. (-540 to 540)", 2, new AcceptableValueRange<float>(-540f, 540f)));
            NumberKeysPick = cfg.Bind(S_MENU, "Number keys pick a grenade", true, Desc(
                "While the menu is open, 1 to 9 pick the grenade at that position and ready it.", 1));

            ThrowForceMult = cfg.Bind(S_PHYS, "Throw strength", 1.1f, Desc(
                "How far your grenades fly, as a multiple of the game's throw force. (0.5 to 3)", 2, new AcceptableValueRange<float>(0.5f, 3f)));
            FuseTimeMult = cfg.Bind(S_PHYS, "Fuse time", 1f, Desc(
                "Every grenade's fuse time, as a multiple of the game's; the cook countdown follows. (0.5 to 3)", 1, new AcceptableValueRange<float>(0.5f, 3f)));

            GrenadeConfigMigration.Run(cfg, this);
        }

        private static ConfigDescription Desc(string text, int order, AcceptableValueBase range = null)
        {
            return new ConfigDescription(text, range, new ConfigurationManagerAttributes { Order = order });
        }

        public float Speed(AnimPart part)
        {
            bool enhanced = GrenadeInputManager.EnhancedArmed;
            switch (part)
            {
                case AnimPart.Deploy:
                case AnimPart.PinPull:
                    return enhanced ? HoldSpeed.Value : TapSpeed.Value;
                case AnimPart.HighThrow:
                case AnimPart.LowThrow:
                    return enhanced ? HoldSpeed.Value : TapSpeed.Value * GrenadeTuning.TapThrowRatio;
                case AnimPart.Holster:
                    return enhanced ? HoldSpeed.Value : TapHolsterSpeed.Value;
                case AnimPart.UnderhandRepin:
                    return GrenadeTuning.UnderhandRepinSpeed;
                default:
                    return 1f;
            }
        }
    }

    internal static class GrenadeTuning
    {
        internal const float TapThrowRatio = 1f / 1.2f;
        internal const float UnderhandRepinSpeed = 100f;
        internal const float UnderhandRepinHold = 0.2f;
        internal const float DeployEndPoint = 0.62f;
        internal const float PinPullStartPoint = 0.4f;
        internal const float CutInBlend = 0.35f;
        internal const float CookFlashIntensity = 1f;
        internal const float CookFlashDuration = 0.3f;
        internal const string CookCueSounds = "Lever, Safety, Fuse";
        internal static readonly Vector3 TwitchPosition = new Vector3(0f, 0.005f, -0.01f);
        internal static readonly Vector3 TwitchRotation = new Vector3(1.3f, 0f, -1f);
        internal const float TwitchSpeed = 5f;
        internal const float TwitchDamping = 3f;
        internal const float MenuFadeIn = 0.15f;
    }

    internal static class GrenadeConfigMigration
    {
        private static readonly string[] OldSections =
        {
            "1 - Trigger", "2 - Animation (Enhanced / Hold)", "3 - Animation (Vanilla / Tap)", "4 - Cooking",
            "5 - Grenade Menu", "6 - Physics", "7 - Trajectory Arc", "8 - Diagnostics (Morph R&D)", "9 - Debug"
        };

        internal static void Run(ConfigFile config, GrenadeConfig c)
        {
            var orphans = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries")?.GetValue(config) as Dictionary<ConfigDefinition, string>;
            if (orphans == null || orphans.Count == 0) return;

            int carried = 0;
            void Carry(string section, string key, ConfigEntryBase target)
            {
                if (orphans.TryGetValue(new ConfigDefinition(section, key), out string value))
                {
                    target.SetSerializedValue(value);
                    carried++;
                }
            }
            bool Flag(string section, string key) =>
                orphans.TryGetValue(new ConfigDefinition(section, key), out string v) && string.Equals(v, "true", System.StringComparison.OrdinalIgnoreCase);

            Carry("1 - Trigger", "Enhanced Throw Enabled", c.EnhancedEnabled);
            Carry("1 - Trigger", "Grenade Key", c.GrenadeKey);
            Carry("1 - Trigger", "Hold Threshold (ms)", c.HoldThresholdMs);
            Carry("1 - Trigger", "Throw Mode", c.Mode);
            Carry("1 - Trigger", "Auto Pull-Pin", c.AutoPullPin);
            Carry("1 - Trigger", "Enhanced Underhand Throw", c.EnhancedUnderhandThrow);
            Carry("1 - Trigger", "Tap Opens Grenade Menu", c.TapOpensMenu);
            Carry("2 - Animation (Enhanced / Hold)", "Deploy Speed", c.HoldSpeed);
            Carry("3 - Animation (Vanilla / Tap)", "Deploy Speed", c.TapSpeed);
            Carry("3 - Animation (Vanilla / Tap)", "Holster Speed", c.TapHolsterSpeed);
            Carry("4 - Cooking", "Cooking Enabled", c.CookingEnabled);
            Carry("4 - Cooking", "Cook Button", c.CookButton);
            Carry("4 - Cooking", "Detonate On Over-cook", c.OverCookDetonates);
            Carry("4 - Cooking", "Cook Flash Color", c.CookFlashTone);
            Carry("4 - Cooking", "Cook Cue Volume", c.CookCueVolume);
            Carry("4 - Cooking", "Cook Twitch Enabled", c.CookTwitchEnabled);
            Carry("5 - Grenade Menu", "Menu Scale", c.MenuScale);
            Carry("5 - Grenade Menu", "Menu Position X", c.MenuOffsetX);
            Carry("5 - Grenade Menu", "Menu Position Y", c.MenuOffsetY);
            Carry("5 - Grenade Menu", "Number Keys Pick Grenade", c.NumberKeysPick);
            Carry("6 - Physics", "Throw Force Multiplier", c.ThrowForceMult);
            Carry("6 - Physics", "Fuse Time Multiplier", c.FuseTimeMult);
            if (orphans.ContainsKey(new ConfigDefinition("4 - Cooking", "Cook Flash")))
            {
                c.Indicator.Value = Flag("4 - Cooking", "Cook Flash") ? CookIndicator.Flash
                    : Flag("4 - Cooking", "Cook Notifier") ? CookIndicator.Text
                    : Flag("4 - Cooking", "Show Cook Timer (HUD)") ? CookIndicator.Timer
                    : CookIndicator.None;
                carried++;
            }

            int removed = 0;
            foreach (ConfigDefinition definition in new List<ConfigDefinition>(orphans.Keys))
            {
                if (System.Array.IndexOf(OldSections, definition.Section) >= 0)
                {
                    orphans.Remove(definition);
                    removed++;
                }
            }
            if (carried == 0 && removed == 0) return;
            config.Save();
            Plugin.Log.LogInfo($"Settings migrated from Aero.Grenade 0.15: {carried} value(s) carried over, {removed} old entr{(removed == 1 ? "y" : "ies")} removed");
        }
    }
}
