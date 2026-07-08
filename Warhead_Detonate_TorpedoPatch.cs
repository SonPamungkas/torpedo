using HarmonyLib;

namespace Torpedo
{


    [HarmonyPatch]
    public static class Warhead_Detonate_TorpedoPatch
    {
        public static System.Reflection.MethodBase TargetMethod()
        {
            return AccessTools.Method(AccessTools.Inner(typeof(Missile), "Warhead"), "Detonate");
        }

        public static void Prefix(ref bool armed)
        {
            armed = true;
        }
    }
}

