using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Torpedo
{
    
    
    
    
    
    public static class TorpedoWake
    {
        private const float SpawnInterval = 0.2f;
        private const float SplashLifetime = 16f;

        private static readonly Dictionary<Missile, float> _nextSpawnTime = new Dictionary<Missile, float>();

        private static readonly List<GameObject> _shipWakeTemplates = new List<GameObject>();
        private static bool _shipWakeLookupDone;

        public static void UpdateWake(Missile missile, bool active)
        {
            if (!active)
            {
                _nextSpawnTime.Remove(missile);
                return;
            }

            if (GameAssets.i == null || GameAssets.i.splash_large == null) return;

            if (_nextSpawnTime.TryGetValue(missile, out float nextTime) && Time.time < nextTime) return;
            _nextSpawnTime[missile] = Time.time + SpawnInterval;

            EnsureShipWakeTemplates();

            
            
            
            GlobalPosition gp = missile.GlobalPosition();
            if (gp.y < -2f) return;
            gp.y = 0f;
            Vector3 position = gp.ToLocalPosition();

            Vector3 vel = missile.rb.velocity;
            Quaternion rotation = Quaternion.LookRotation(Vector3.up - new Vector3(vel.x, 0f, vel.z) * 0.1f);

            UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate(GameAssets.i.splash_large, position, rotation), SplashLifetime);

            foreach (GameObject template in _shipWakeTemplates)
            {
                UnityEngine.Object.Destroy(UnityEngine.Object.Instantiate(template, position, rotation), SplashLifetime);
            }
        }

        
        
        
        
        private static void EnsureShipWakeTemplates()
        {
            if (_shipWakeLookupDone) return;
            _shipWakeLookupDone = true;

            try
            {
                Type wakeParticlesType = AccessTools.Inner(typeof(Ship), "WakeParticles");
                if (wakeParticlesType == null) return;

                Ship ship = Resources.FindObjectsOfTypeAll<Ship>().FirstOrDefault(s => s != null);
                if (ship == null) return;

                object wakeParticlesArray = Traverse.Create(ship).Field("wakeParticles").GetValue();
                if (!(wakeParticlesArray is Array entries)) return;

                foreach (object entry in entries)
                {
                    if (entry == null) continue;
                    if (_shipWakeTemplates.Count >= 3) break;

                    ParticleSystem system = Traverse.Create(entry).Field("system").GetValue<ParticleSystem>();
                    if (system == null) continue;

                    _shipWakeTemplates.Add(system.gameObject);
                }
            }
            catch (Exception ex)
            {
                TorpedoPlugin.ModLogger.LogError("[Torpedo] TorpedoWake: ship wake discovery failed: " + ex);
            }
        }

        public static void RemoveWake(Missile missile)
        {
            _nextSpawnTime.Remove(missile);
        }
    }
}

