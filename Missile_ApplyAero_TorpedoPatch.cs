using HarmonyLib;

namespace Torpedo
{
    [HarmonyPatch(typeof(Missile), "ApplyAero")]
    public static class Missile_ApplyAero_TorpedoPatch
    {
        public static void Prefix(Missile __instance)
        {
            if (__instance.definition == null) return;
            if (!TorpedoMounts_Patch.HoverAltitudeByName.ContainsKey(__instance.definition.jsonKey)) return;

            Traverse currentFinAreaTraverse = Traverse.Create(__instance).Field("currentFinArea");
            if (__instance.GlobalPosition().y > 0f)
            {
                currentFinAreaTraverse.SetValue(0f);
            }
            else if (currentFinAreaTraverse.GetValue<float>() <= 0f)
            {
                Traverse finAreaTraverse = Traverse.Create(__instance).Field("finArea");
                currentFinAreaTraverse.SetValue(finAreaTraverse.GetValue<float>());
            }
        }
    }
}
