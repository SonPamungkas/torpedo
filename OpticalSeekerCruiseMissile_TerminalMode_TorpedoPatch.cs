using HarmonyLib;
using UnityEngine;

namespace Torpedo
{
    [HarmonyPatch(typeof(OpticalSeekerCruiseMissile), "TerminalMode")]
    public static class OpticalSeekerCruiseMissile_TerminalMode_TorpedoPatch
    {
        private static readonly AccessTools.FieldRef<MissileSeeker, Missile> missileRef =
            AccessTools.FieldRefAccess<MissileSeeker, Missile>("missile");
        private static readonly AccessTools.FieldRef<MissileSeeker, Unit> targetUnitRef =
            AccessTools.FieldRefAccess<MissileSeeker, Unit>("targetUnit");

        public static bool Prefix(OpticalSeekerCruiseMissile __instance)
        {
            Missile missile = missileRef(__instance);
            if (missile == null || missile.definition == null) return true;
            if (!TorpedoMounts_Patch.HoverAltitudeByName.ContainsKey(missile.definition.jsonKey)) return true;
            if (targetUnitRef(__instance) != null) return true;

            GlobalPosition aimPoint = missile.GlobalPosition() + missile.transform.forward * 100000f;
            missile.SetAimpoint(aimPoint, Vector3.zero);
            return false;
        }
    }
}
