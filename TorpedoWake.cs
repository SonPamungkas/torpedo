using System;
using System.Collections.Generic;
using UnityEngine;

namespace Torpedo
{


    public static class TorpedoWake
    {
        private static readonly Dictionary<Missile, float> _nextSplashTime = new Dictionary<Missile, float>();
        private const float SplashLifetime = 16f;

        public static void UpdateWake(Missile missile, bool active)
        {
            if (!active)
            {
                RemoveWake(missile);
                return;
            }

            if (GameAssets.i == null || GameAssets.i.splash_large == null) return;

            GlobalPosition gp = missile.GlobalPosition();
            gp.y = 0f;
            Vector3 position = gp.ToLocalPosition();
            Vector3 vel = missile.rb.velocity;
            
            float speed = new Vector3(vel.x, 0f, vel.z).magnitude;

            float dynamicInterval = Mathf.Clamp(10f / Mathf.Max(1f, speed), 0.06f, 1.0f);
            if (!_nextSplashTime.TryGetValue(missile, out float nextTime) || Time.time >= nextTime)
            {
                _nextSplashTime[missile] = Time.time + dynamicInterval;
                Quaternion splashRotation = Quaternion.LookRotation(Vector3.up + new Vector3(vel.x, 0f, vel.z) * 0.1f);
                
                GameObject splash = UnityEngine.Object.Instantiate(GameAssets.i.splash_large, position, splashRotation);
                foreach (var ps in splash.GetComponentsInChildren<ParticleSystem>())
                {
                    if (ps.gameObject.name.Equals("waterImpactSmall(Clone)", StringComparison.OrdinalIgnoreCase) || 
                        ps.gameObject.name.Equals("waterImpactSmall", StringComparison.OrdinalIgnoreCase))
                    {
                        ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                        var em = ps.emission;
                        em.enabled = false;
                        continue;
                    }
                    var main = ps.main;
                    main.simulationSpeed *= 2f;
                    main.startLifetimeMultiplier *= 0.5f;
                }
                UnityEngine.Object.Destroy(splash, SplashLifetime);
            }
        }

        public static void RemoveWake(Missile missile)
        {
            _nextSplashTime.Remove(missile);
        }
    }
}
