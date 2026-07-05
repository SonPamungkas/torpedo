using System.Collections.Generic;
using HarmonyLib;

namespace Torpedo
{
    [HarmonyPatch(typeof(Missile), "TakeDamage")]
    public static class Missile_TakeDamage_TorpedoPatch
    {
        internal static readonly HashSet<Missile> InProgress = new HashSet<Missile>();

        [HarmonyPrefix]
        public static void Prefix(Missile __instance) => InProgress.Add(__instance);

        [HarmonyPostfix]
        public static void Postfix(Missile __instance) => InProgress.Remove(__instance);
    }
}
