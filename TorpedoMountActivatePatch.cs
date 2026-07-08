using System;
using HarmonyLib;
using UnityEngine;

namespace Torpedo
{



    [HarmonyPatch(typeof(Hardpoint), "SpawnMount")]
    public static class TorpedoMountActivatePatch
    {
        private static void Postfix(WeaponMount weaponMount, GameObject __result)
        {
            try
            {
                if (__result != null && weaponMount != null
                    && TorpedoMounts_Patch.CreatedMountNames.Contains(weaponMount.name)
                    && !__result.activeSelf)
                {
                    __result.SetActive(true);
                }
            }
            catch (Exception ex)
            {
                TorpedoPlugin.ModLogger.LogWarning("[Torpedo] SpawnMount postfix failed: " + ex.Message);
            }
        }
    }
}

