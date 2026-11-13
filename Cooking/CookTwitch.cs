using EFT.Animations;
using UnityEngine;

namespace Aero.Grenade
{
    public static class CookTwitch
    {
        public static bool Active { get; private set; }
        public static ProceduralWeaponAnimation Pwa { get; private set; }
        public static Vector3 PosOffset { get; private set; }
        public static Quaternion RotOffset { get; private set; } = Quaternion.identity;

        private static float _d;
        private static bool _rising;

        public static void Trigger(ProceduralWeaponAnimation pwa)
        {
            if (!Plugin.Cfg.CookTwitchEnabled.Value || pwa == null) return;
            Pwa = pwa;
            _d = 0f;
            _rising = true;
            Active = true;
        }

        public static void Reset()
        {
            Active = false;
            Pwa = null;
            _d = 0f;
            PosOffset = Vector3.zero;
            RotOffset = Quaternion.identity;
        }

        public static void Tick()
        {
            if (!Active) return;

            float dt = Time.unscaledDeltaTime;
            if (_rising)
            {
                _d = Mathf.MoveTowards(_d, 1f, GrenadeTuning.TwitchSpeed * dt);
                if (_d >= 1f) _rising = false;
            }
            else
            {
                _d = Mathf.MoveTowards(_d, 0f, GrenadeTuning.TwitchDamping * dt);
                if (_d <= 0f) { Reset(); return; }
            }

            float e = Mathf.SmoothStep(0f, 1f, _d);
            var c = Plugin.Cfg;
            PosOffset = GrenadeTuning.TwitchPosition * e;
            RotOffset = Quaternion.Euler(
                GrenadeTuning.TwitchRotation.x * e, GrenadeTuning.TwitchRotation.y * e, GrenadeTuning.TwitchRotation.z * e);
        }
    }
}
