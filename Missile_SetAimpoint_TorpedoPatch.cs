using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Torpedo
{
    [HarmonyPatch(typeof(Missile), "SetAimpoint")]
    public static class Missile_SetAimpoint_TorpedoPatch
    {
        private static readonly AccessTools.FieldRef<Missile, MissileSeeker> seekerRef =
            AccessTools.FieldRefAccess<Missile, MissileSeeker>("seeker");
        private static readonly AccessTools.FieldRef<OpticalSeekerCruiseMissile, float> altitudeTargetRef =
            AccessTools.FieldRefAccess<OpticalSeekerCruiseMissile, float>("altitudeTarget");
        private static readonly AccessTools.FieldRef<OpticalSeekerCruiseMissile, JinkEvasion> jinkRef =
            AccessTools.FieldRefAccess<OpticalSeekerCruiseMissile, JinkEvasion>("jinkEvasion");
        private static readonly AccessTools.FieldRef<OpticalSeekerCruiseMissile, TopAttack> topAttackRef =
            AccessTools.FieldRefAccess<OpticalSeekerCruiseMissile, TopAttack>("topAttack");
        private static readonly AccessTools.FieldRef<OpticalSeekerCruiseMissile, TerminalBoost> terminalBoostRef =
            AccessTools.FieldRefAccess<OpticalSeekerCruiseMissile, TerminalBoost>("terminalBoost");

        private static readonly HashSet<OpticalSeekerCruiseMissile> _neutered = new HashSet<OpticalSeekerCruiseMissile>();

        public static bool Prefix(Missile __instance, ref GlobalPosition aimPoint, ref Vector3 targetVel)
        {
            if (__instance.definition == null) return true;
            if (!TorpedoMounts_Patch.HoverAltitudeByName.TryGetValue(__instance.definition.jsonKey, out float hoverAltitude))
                return true;

            if (seekerRef(__instance) is OpticalSeekerCruiseMissile cSeeker && _neutered.Add(cSeeker))
            {
                JinkEvasion jink = jinkRef(cSeeker);
                if (jink != null) jink.amount = 0f;

                TopAttack topAttack = topAttackRef(cSeeker);
                if (topAttack != null)
                {
                    topAttack.Amount = 0f;
                    topAttack.Active = false;
                }

                TerminalBoost terminalBoost = terminalBoostRef(cSeeker);
                if (terminalBoost != null)
                {
                    terminalBoost.Amount = 0f;
                    terminalBoost.Active = false;
                }
            }

            if (seekerRef(__instance) is OpticalSeekerCruiseMissile seeker)
            {
                altitudeTargetRef(seeker) = hoverAltitude;
            }

            if (TorpedoPhysics.InCruisePhase(__instance))
            {
                aimPoint.y = hoverAltitude;

                if (__instance.GlobalPosition().y <= 0f)
                {
                    TorpedoPhysics.ApplyTorpedoPhysics(__instance, hoverAltitude);
                }

                int motorStage = Traverse.Create(__instance).Field("motorStage").GetValue<int>();
                if (motorStage >= 1)
                {
                    object irSourcesObj = Traverse.Create(__instance).Field("IRSources").GetValue();
                    if (irSourcesObj is System.Collections.IEnumerable irSources)
                    {
                        foreach (object source in irSources)
                        {
                            if (source is IRSource irSource) irSource.intensity = 0f;
                        }
                    }
                }

                Vector3 toTarget = aimPoint - __instance.GlobalPosition();
                if (toTarget.sqrMagnitude < 40000f)
                {
                    if (Vector3.Dot(toTarget, __instance.rb.velocity) < 0f)
                    {
                        AccessTools.Method(typeof(Missile), "Detonate").Invoke(__instance, new object[] { Vector3.up, true, false });
                        __instance.rb.velocity = Vector3.zero;
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
