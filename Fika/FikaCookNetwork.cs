using System.Collections.Generic;
using EFT;
using Fika.Core.Main.Players;
using Fika.Core.Main.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using HarmonyLib;
using UnityEngine;

namespace Aero.Grenade.Fika
{
    public static class FikaCookNetwork
    {
        private const string OBSERVED_CTRL = "Fika.Core.Main.ObservedClasses.HandsControllers.ObservedGrenadeController";
        private const float COOK_TTL = 10f;

        private static IFikaNetworkManager _net;
        private static readonly Dictionary<int, float> _cook  = new Dictionary<int, float>();
        private static readonly Dictionary<int, float> _stamp = new Dictionary<int, float>();

        public static void Init()
        {
            FikaCompat.SendCook = SendCook;
            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetCreated);
            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerDestroyedEvent>(OnNetDestroyed);

            var harmony = new Harmony("aero.grenade.fika");
            var target = AccessTools.Method(AccessTools.TypeByName(OBSERVED_CTRL), "SpawnGrenade");
            if (target != null)
                harmony.Patch(target, prefix: new HarmonyMethod(typeof(FikaCookNetwork), nameof(SpawnGrenadePrefix)));
            else
                Plugin.Log.LogWarning("[Aero.Grenade] Fika: ObservedGrenadeController.SpawnGrenade not found — headless cook sync inactive.");

            Diagnostics.Verbose("Fika cook-sync initialized.");
        }

        private static void OnNetCreated(FikaNetworkManagerCreatedEvent e)
        {
            _net = e.Manager;
            e.Manager.RegisterPacket<CookPrewarmPacket, NetPeer>(CookPrewarmPacket.OnReceived);
            _cook.Clear();
            _stamp.Clear();
            Diagnostics.Verbose($"Fika cook-sync: network up (isServer={FikaBackendUtils.IsServer}, isHeadless={FikaBackendUtils.IsHeadless}).");
        }

        private static void OnNetDestroyed(FikaNetworkManagerDestroyedEvent e)
        {
            _net = null;
            _cook.Clear();
            _stamp.Clear();
        }

        public static void SendCook(Player.BaseGrenadeHandsController ctrl, float elapsed)
        {
            if (_net == null || FikaBackendUtils.IsServer) return;

            var fp = Traverse.Create(ctrl).Field("_player").GetValue<Player>() as FikaPlayer;
            if (fp == null) return;

            var pkt = new CookPrewarmPacket { NetId = fp.NetId, CookTime = elapsed };
            _net.SendData(ref pkt, DeliveryMethod.ReliableOrdered, true);
            Diagnostics.Verbose($"Fika cook-sync: sent {elapsed:F2}s for netId {fp.NetId}.");
        }

        public static void StoreRemoteCook(int netId, float time)
        {
            _cook[netId] = time;
            _stamp[netId] = Time.realtimeSinceStartup;
        }

        private static void SpawnGrenadePrefix(Player.GrenadeHandsController __instance, ref float timeSinceSafetyLevelRemoved)
        {
            var fp = Traverse.Create(__instance).Field("_player").GetValue<Player>() as FikaPlayer;
            if (fp == null) return;

            if (!_cook.TryGetValue(fp.NetId, out float t)) return;
            _cook.Remove(fp.NetId);
            float stamp = _stamp.TryGetValue(fp.NetId, out var s) ? s : 0f;
            _stamp.Remove(fp.NetId);

            if (Time.realtimeSinceStartup - stamp > COOK_TTL) return;

            timeSinceSafetyLevelRemoved = t;
            Diagnostics.Verbose($"Fika cook-sync: applied {t:F2}s prewarm to netId {fp.NetId}'s grenade.");
        }
    }
}
