using HarmonyLib;

namespace Torpedo
{
    [HarmonyPatch(typeof(OpticalSeekerCruiseMissile), "TerrainWaypoint")]
    public static class OpticalSeekerCruiseMissile_TerrainWaypoint_TorpedoPatch
    {
        private static readonly AccessTools.FieldRef<MissileSeeker, Missile> missileRef =
            AccessTools.FieldRefAccess<MissileSeeker, Missile>("missile");

        public static bool Prefix(OpticalSeekerCruiseMissile __instance, GlobalPosition destination, ref GlobalPosition __result)
        {
            Missile missile = missileRef(__instance);
            if (missile == null || missile.definition == null) return true;
            if (!TorpedoMounts_Patch.HoverAltitudeByName.TryGetValue(missile.definition.jsonKey, out float hoverAltitude))
                return true;
            if (!TorpedoPhysics.InCruisePhase(missile)) return true;

            destination.y = hoverAltitude;
            __result = destination;
            return false;
        }
    }
}
