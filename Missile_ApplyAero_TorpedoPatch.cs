using HarmonyLib;
namespace Torpedo
{
    [HarmonyPatch(typeof(Missile), "ApplyAero")]
    public static class Missile_ApplyAero_TorpedoPatch
    {
        private static readonly AccessTools.FieldRef<Missile, float> currentFinAreaRef =
            AccessTools.FieldRefAccess<Missile, float>("currentFinArea");
        private static readonly AccessTools.FieldRef<Missile, float> finAreaRef =
            AccessTools.FieldRefAccess<Missile, float>("finArea");
        public static void Prefix(Missile __instance)
        {
            if (__instance.definition == null) return;
            if (!TorpedoMounts_Patch.HoverAltitudeByName.ContainsKey(__instance.definition.jsonKey)) return;
            if (__instance.GlobalPosition().y > 0f)
            {
                currentFinAreaRef(__instance) = 0f;
            }
            else if (currentFinAreaRef(__instance) <= 0f)
            {
                currentFinAreaRef(__instance) = finAreaRef(__instance);
            }
        }
    }
}