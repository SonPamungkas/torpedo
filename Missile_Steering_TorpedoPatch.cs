using HarmonyLib;
using UnityEngine;

namespace Torpedo
{





    [HarmonyPatch(typeof(Missile), "Steering")]
    public static class Missile_Steering_TorpedoPatch
    {
        public static bool Prefix(Missile __instance)
        {
            if (__instance.definition == null) return true;
            if (!TorpedoMounts_Patch.HoverAltitudeByName.ContainsKey(__instance.definition.jsonKey)) return true;
            if (__instance.GlobalPosition().y <= 0f) return true;

            Traverse.Create(__instance).Field("inputs").SetValue(Vector3.zero);
            return false;
        }
    }
}

