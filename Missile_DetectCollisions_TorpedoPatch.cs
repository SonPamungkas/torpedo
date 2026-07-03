using HarmonyLib;
using UnityEngine;

namespace Torpedo
{
    [HarmonyPatch(typeof(Missile), "DetectCollisions")]
    public static class Missile_DetectCollisions_TorpedoPatch
    {
        public static bool Prefix(Missile __instance)
        {
            if (__instance.definition == null) return true;
            if (!TorpedoMounts_Patch.HoverAltitudeByName.ContainsKey(__instance.definition.jsonKey)) return true;
            if (__instance.rb == null) return true;
            if (__instance.GlobalPosition().y >= 50f) return true;
            if (!TorpedoPhysics.IsOverWater(__instance)) return true;

            RaycastHit[] hits = __instance.rb.SweepTestAll(
                __instance.rb.velocity.normalized,
                __instance.rb.velocity.magnitude * Time.fixedDeltaTime,
                QueryTriggerInteraction.Ignore);

            foreach (var hit in hits)
            {
                Unit unit = hit.collider.GetComponentInParent<Unit>();
                if (unit != null)
                {
                    AccessTools.Method(typeof(Missile), "Detonate").Invoke(__instance, new object[] { hit.normal, true, false });
                    __instance.rb.velocity = Vector3.zero;
                    return false;
                }
            }

            return false;
        }
    }
}
