using HarmonyLib;
using UnityEngine;
namespace Torpedo
{
    [HarmonyPatch(typeof(Unit), "DisableUnit")]
    public static class Unit_DisableUnit_TorpedoPatch
    {
        public static bool Prefix(Unit __instance)
        {
            if (!(__instance is Missile missile)) return true;
            if (missile.definition == null) return true;
            if (!TorpedoMounts_Patch.HoverAltitudeByName.ContainsKey(missile.definition.jsonKey)) return true;
            missile.Detonate(Vector3.up, hitArmor: false, hitTerrain: true);
            return false;
        }
    }
}