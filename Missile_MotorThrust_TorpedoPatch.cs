using HarmonyLib;

namespace Torpedo
{
    [HarmonyPatch(typeof(Missile), "MotorThrust")]
    public static class Missile_MotorThrust_TorpedoPatch
    {
        public static bool Prefix(Missile __instance)
        {
            if (__instance.definition == null) return true;
            if (!TorpedoMounts_Patch.HoverAltitudeByName.ContainsKey(__instance.definition.jsonKey)) return true;
            if (__instance.GlobalPosition().y <= 0f) return true;

            Traverse.Create(__instance).Field("engineCurrentThrust").SetValue(0f);
            return false;
        }
    }
}
