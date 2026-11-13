using System;
using EFT;

namespace Aero.Grenade
{
    public static class FikaCompat
    {
        public const string FIKA_GUID = "com.fika.core";
        public const string SYNC_ASSEMBLY = "Aero.Grenade.Fika.dll";
        public const string SYNC_ENTRY = "Aero.Grenade.Fika.FikaCookNetwork";

        public static bool Active;

        public static Action<Player.BaseGrenadeHandsController, float> SendCook;
    }
}
