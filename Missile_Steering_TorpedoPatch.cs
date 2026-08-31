using HarmonyLib;
using UnityEngine;
namespace Torpedo
{
    [HarmonyPatch(typeof(Missile), "Steering")]
    public static class Missile_Steering_TorpedoPatch
    {
        private static readonly AccessTools.FieldRef<Missile, Vector3> inputsRef =
            AccessTools.FieldRefAccess<Missile, Vector3>("inputs");
        private static readonly AccessTools.FieldRef<Missile, Vector3> localAngularVelRef =
            AccessTools.FieldRefAccess<Missile, Vector3>("localAngularVel");
        public static bool Prefix(Missile __instance)
        {
            if (__instance.definition == null) return true;
            if (!TorpedoMounts_Patch.HoverAltitudeByName.ContainsKey(__instance.definition.jsonKey)) return true;
            if (__instance.GlobalPosition().y <= 0f) return true;
            inputsRef(__instance) = Vector3.zero;
            localAngularVelRef(__instance) = __instance.rb != null
                ? __instance.transform.InverseTransformVector(__instance.rb.angularVelocity)
                : Vector3.zero;
            return false;
        }
    }
}