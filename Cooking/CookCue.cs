using System.Linq;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace Aero.Grenade
{
    public static class CookCue
    {
        private const int ROLLOFF = 30;
        private static GameObject _loggedFor;

        public static void Play(Player.GrenadeHandsController ctrl)
        {
            if (ctrl == null) return;

            string raw = GrenadeTuning.CookCueSounds;
            if (string.IsNullOrEmpty(raw)) return;

            var names = raw.Split(',')
                .Select(s => s.Trim())
                .Where(s => s.Length > 0 && !s.Equals("None", System.StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (names.Length == 0) return;

            var sp = GetSoundPlayer(ctrl);
            if (sp == null) { Diagnostics.Verbose("Cook cue: no BaseSoundPlayer on grenade prefab."); return; }

            MaybeLogAvailable(ctrl, sp);

            if (!Singleton<BetterAudio>.Instantiated)
            {
                Diagnostics.Verbose("Cook cue: BetterAudio not instantiated.");
                return;
            }

            Vector3 pos = ctrl.ControllerGameObject != null
                ? ctrl.ControllerGameObject.transform.position
                : Vector3.zero;
            float volMult = Plugin.Cfg.CookCueVolume.Value;

            int played = 0;
            foreach (var name in names)
            {
                var elem = FindElement(sp, name);
                var clip = elem?.RandomSoundClip;
                if (clip == null) { Diagnostics.Verbose($"Cook cue: '{name}' not found / no clip."); continue; }

                try
                {
                    Singleton<BetterAudio>.Instance.PlayAtPoint(
                        pos, clip, BetterAudio.AudioSourceGroupType.Grenades, ROLLOFF,
                        elem.Volume * volMult, EOcclusionTest.None, null,
                        spatialize: false, oneShot: true, autoReleaseSource: true);
                    played++;
                }
                catch (System.Exception e) { Diagnostics.Verbose($"Cook cue play '{name}' failed: {e.Message}"); }
            }

            if (played > 0) Diagnostics.Verbose($"Cook cue: played {played} sound(s) [{string.Join(", ", names)}].");
        }

        private static BaseSoundPlayer.SoundElement FindElement(BaseSoundPlayer sp, string name)
        {
            return sp.AdditionalSounds?.Find(e =>
                e != null && (e.EventName == name || e.EventName == "Snd" + name));
        }

        private static BaseSoundPlayer GetSoundPlayer(Player.GrenadeHandsController ctrl)
        {
            var go = ctrl.ControllerGameObject;
            return go != null ? go.GetComponent<BaseSoundPlayer>() : null;
        }

        private static void MaybeLogAvailable(Player.GrenadeHandsController ctrl, BaseSoundPlayer sp)
        {
            if (!Diagnostics.VerboseOn) return;
            var go = ctrl.ControllerGameObject;
            if (go == _loggedFor) return;
            _loggedFor = go;

            try
            {
                var available = sp.AdditionalSounds?
                    .Where(e => e != null && !string.IsNullOrEmpty(e.EventName))
                    .Select(e => e.EventName);
                Diagnostics.Verbose($"Grenade sound events: {(available != null ? string.Join(", ", available) : "(none)")}");
            }
            catch (System.Exception e) { Diagnostics.Verbose($"Cook cue enumerate failed: {e.Message}"); }
        }
    }
}
