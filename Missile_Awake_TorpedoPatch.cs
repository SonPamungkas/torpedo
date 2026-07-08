using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Torpedo
{







    [HarmonyPatch(typeof(Missile), "Awake")]
    public static class Missile_Awake_TorpedoPatch
    {
        private static readonly List<Collider> _activeTorpedoColliders = new List<Collider>();

        [HarmonyPostfix]
        public static void Postfix(Missile __instance)
        {
            if (__instance.definition == null) return;
            if (!TorpedoMounts_Patch.HoverAltitudeByName.ContainsKey(__instance.definition.jsonKey)) return;

            Collider[] myColliders = __instance.GetComponentsInChildren<Collider>(true);
            if (myColliders.Length == 0) return;

            _activeTorpedoColliders.RemoveAll(c => c == null);
            foreach (Collider existing in _activeTorpedoColliders)
            {
                foreach (Collider mine in myColliders)
                {
                    Physics.IgnoreCollision(mine, existing, true);
                }
            }
            _activeTorpedoColliders.AddRange(myColliders);
        }
    }
}

