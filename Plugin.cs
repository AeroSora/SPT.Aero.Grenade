using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace Aero.Grenade
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(FikaCompat.FIKA_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "aero.grenade";
        public const string NAME = "Aero.Grenade";
        public const string VERSION = "0.1.0";

        public static Plugin Instance { get; private set; }
        public static ManualLogSource Log { get; private set; }
        public static GrenadeConfig Cfg { get; private set; }

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            Cfg = new GrenadeConfig(Config);

            new Harmony(GUID).PatchAll();

            FikaCompat.Active = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(FikaCompat.FIKA_GUID)
                && FikaLoader.Load(System.IO.Path.GetDirectoryName(Info.Location), FikaCompat.SYNC_ASSEMBLY, FikaCompat.SYNC_ENTRY, Log);

            Log.LogInfo($"{NAME} {VERSION} loaded" + (FikaCompat.Active ? " with Fika cook sync" : ""));
        }

        private void Update()
        {
            GrenadeMenu.Prewarm();
            GrenadeInputManager.Tick();
            DeployPinCutIn.Tick();
            GrenadeThrowDriver.Tick();
            GrenadeMenu.TickFade();
            CookTwitch.Tick();
            CookFlash.Tick();
        }

        private void OnGUI()
        {
            Diagnostics.DrawHud();
        }
    }
}
