using HarmonyLib;
using UnityEngine;

namespace Torpedo
{
    [HarmonyPatch(typeof(Missile), "Detonate")]
    public static class Missile_Detonate_TorpedoPatch
    {
        public static bool Prefix(Missile __instance, Vector3 normal, bool hitArmor, bool hitTerrain)
        {
            if (__instance.definition == null) return true;
            if (!TorpedoMounts_Patch.HoverAltitudeByName.ContainsKey(__instance.definition.jsonKey)) return true;

            if (!hitArmor && !hitTerrain && !Missile_TakeDamage_TorpedoPatch.InProgress.Contains(__instance))
            {
                return false;
            }

            TorpedoWake.RemoveWake(__instance);

            return true;
        }
    }
}
