using HarmonyLib;
namespace Torpedo
{
    [HarmonyPatch(typeof(Missile), "MotorThrust")]
    public static class Missile_MotorThrust_TorpedoPatch
    {
        private static readonly AccessTools.FieldRef<Missile, float> engineCurrentThrustRef =
            AccessTools.FieldRefAccess<Missile, float>("engineCurrentThrust");
        public static bool Prefix(Missile __instance)
        {
            if (__instance.definition == null) return true;
            if (!TorpedoMounts_Patch.HoverAltitudeByName.ContainsKey(__instance.definition.jsonKey)) return true;
            if (__instance.GlobalPosition().y <= 0f) return true;
            engineCurrentThrustRef(__instance) = 0f;
            return false;
        }
    }
}