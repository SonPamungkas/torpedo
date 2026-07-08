using HarmonyLib;
using UnityEngine;

namespace Torpedo
{





    [HarmonyPatch(typeof(Unit), "OnDestroy")]
    public static class Unit_OnDestroy_TorpedoPatch
    {
        public static void Prefix(Unit __instance)
        {
            if (!(__instance is Missile missile)) return;
            if (missile.definition == null) return;
            if (!TorpedoMounts_Patch.HoverAltitudeByName.ContainsKey(missile.definition.jsonKey)) return;

            try
            {
                AccessTools.Method(typeof(Missile), "Detonate").Invoke(missile, new object[] { Vector3.up, false, true });
            }
            catch
            {

            }
        }
    }
}

