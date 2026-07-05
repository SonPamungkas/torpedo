using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirage;

namespace Torpedo
{
    [HarmonyPatch(typeof(Encyclopedia), "AfterLoad", new Type[] { })]
    [HarmonyPriority(Priority.Last)]
    public static class TorpedoMounts_Patch
    {
        private struct VariantInfo
        {
            public string SourceMissile;
            public string NewName;
            public string DisplayName;
            public string ShortName;
            public float Mass;
            public float Cost;
            public string Description;
            public float HoverAltitude;
            public float SpeedMultiplier;
            public float ExplosiveMultiplier;
            public float PenetrationBonus;
        }

        private static readonly VariantInfo[] Variants = new[]
        {
            new VariantInfo { SourceMissile = "AShM1", NewName = "TorpedoFast",
                DisplayName = "SCT-350 'Mako'", ShortName = "TORP-FAST", Mass = 300f, Cost = 12.5f, HoverAltitude = -2f, SpeedMultiplier = 0.144f, ExplosiveMultiplier = 2f, PenetrationBonus = 500f,
                Description = "A super-cavitating interceptor engineered for pure kinetic urgency. By generating a localized gas-bubble envelope to negate hydrodynamic drag, the Mako closes the distance to high-value targets with predatory velocity, denying enemies any window to deploy countermeasures." },
            new VariantInfo { SourceMissile = "AShM2", NewName = "TorpedoLight",
                DisplayName = "Type-88 'Lemon'", ShortName = "TORP-LIGHT", Mass = 250f, Cost = 10.5f, HoverAltitude = -1f, SpeedMultiplier = 0.141f, ExplosiveMultiplier = 2.5f, PenetrationBonus = 500f,
                Description = "Compact, agile, and deceptively lethal. While its 'Lemon' designation suggests a dud, this torpedo is a nightmare for defensive grids. Its high maneuverability and low-profile signature make it the premier choice for saturation strikes and ambush that exhaust enemy point-defense systems." },
            new VariantInfo { SourceMissile = "CruiseMissile1", NewName = "TorpedoBig",
                DisplayName = "HT-200 'Hammerhead'", ShortName = "TORP-BIG", Mass = 450f, Cost = 12.5f, HoverAltitude = -8f, SpeedMultiplier = 0.047f, ExplosiveMultiplier = 3f, PenetrationBonus = 4000f,
                Description = "The anvil of the fleet. The Hammerhead sacrifices sprint velocity for raw, concentrated payload capacity. Equipped with an armor-piercing tandem warhead, it is purpose-built to breach reinforced plating and shatter the structural integrity of capital ship keels." },
            new VariantInfo { SourceMissile = "CruiseMissile20kt", NewName = "Torpedo20kt",
                DisplayName = "NT-2 'Megalodon' (20kt)", ShortName = "TORP-20KT", Mass = 450f, Cost = 12.5f, HoverAltitude = -10f, SpeedMultiplier = 0.059f, ExplosiveMultiplier = 1f, PenetrationBonus = 0f,
                Description = "The apex predator of the naval theater. This nuclear-tipped strategic ordnance is designed for total area denial. It doesn't just damage; it erases. A single detonation is capable of neutralizing entire battlegroups, turning a tactical engagement into a strategic vacuum." },
        };

                 private static readonly HashSet<string> _createdMissiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> _createdMounts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static readonly Dictionary<string, float> HoverAltitudeByName =
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

        public static readonly HashSet<string> CreatedMountNames =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static readonly List<(WeaponMount Mount, string MountName, string SourceMountName)> _mountPairs =
            new List<(WeaponMount, string, string)>();

        static TorpedoMounts_Patch()
        {
            foreach (var variant in Variants)
                HoverAltitudeByName[variant.NewName] = variant.HoverAltitude;
        }

        [HarmonyPrefix]
        public static void Prefix(Encyclopedia __instance)
        {
            try { AddMissingMounts(__instance); }
            catch (Exception ex) { TorpedoPlugin.ModLogger.LogError("[Torpedo] TorpedoMounts Prefix failed: " + ex); }
        }

        [HarmonyPostfix]
        public static void Postfix(Encyclopedia __instance)
        {
            try { AddMissingMounts(__instance); }
            catch (Exception ex) { TorpedoPlugin.ModLogger.LogError("[Torpedo] TorpedoMounts Postfix failed: " + ex); }
        }

        private static void AddMissingMounts(Encyclopedia __instance)
        {
            foreach (var variant in Variants)
            {
                MissileDefinition missileDefinition = __instance.missiles.FirstOrDefault(m => m != null && m.name == variant.NewName);
                WeaponInfo info = null;
                GameObject missileClone = null;

                if (missileDefinition == null)
                {
                    if (_createdMissiles.Contains(variant.NewName)) continue;

                    var result = CreateMissileVariant(__instance, variant);
                    if (result == null) continue;
                    (missileDefinition, info, missileClone) = result.Value;

                    _createdMissiles.Add(variant.NewName);
                    TorpedoPlugin.ModLogger.LogInfo($"[Torpedo] TorpedoMounts: added missile {variant.NewName} (from {variant.SourceMissile})");
                }
                else
                {
                    info = ResourceLookupWeaponInfo(missileDefinition, variant);
                    missileClone = missileDefinition.unitPrefab;
                    if (info == null || missileClone == null) continue;
                }

                string prefix = variant.SourceMissile + "_";
                var sourceMounts = Resources.FindObjectsOfTypeAll<WeaponMount>()
                    .Where(m => m != null && m.name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .GroupBy(m => m.name, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First());

                foreach (var sourceMount in sourceMounts)
                {
                    string suffix = sourceMount.name.Substring(variant.SourceMissile.Length);
                    string newMountName = variant.NewName + suffix;

                    WeaponMount mount = __instance.weaponMounts.FirstOrDefault(m => m != null && m.name == newMountName);
                    if (mount == null)
                    {
                        if (_createdMounts.Contains(newMountName)) continue;

                        mount = CreateMountVariant(__instance, sourceMount, newMountName, variant, info, missileClone);
                        if (mount == null) continue;

                        _createdMounts.Add(newMountName);
                        CreatedMountNames.Add(newMountName);
                        TorpedoPlugin.ModLogger.LogInfo($"[Torpedo] TorpedoMounts: added {newMountName} (from {sourceMount.name})");
                    }

                    RegisterOnHardpointsCarrying(mount, newMountName, sourceMount.name);

                    if (!_mountPairs.Any(p => p.MountName == newMountName))
                        _mountPairs.Add((mount, newMountName, sourceMount.name));
                }
            }
        }

        internal static void RegisterOnWeaponManager(WeaponManager wm)
        {
            if (wm == null || wm.hardpointSets == null) return;

            foreach (var hardpointSet in wm.hardpointSets)
            {
                if (hardpointSet == null || hardpointSet.weaponOptions == null) continue;

                foreach (var (mount, mountName, sourceMountName) in _mountPairs)
                {
                    if (mount == null) continue;
                    if (!hardpointSet.weaponOptions.Any(m => m != null && m.name == sourceMountName)) continue;
                    if (hardpointSet.weaponOptions.Contains(mount)) continue;

                    hardpointSet.weaponOptions.Add(mount);
                    TorpedoPlugin.ModLogger.LogInfo($"[Torpedo] TorpedoMounts: registered {mountName} on {wm.gameObject.name} hardpoint '{hardpointSet.name}' (WeaponManager.Awake)");
                }
            }
        }

        private static WeaponInfo ResourceLookupWeaponInfo(MissileDefinition missileDefinition, VariantInfo variant)
        {
            GameObject prefab = missileDefinition.unitPrefab;
            Missile missile = prefab != null ? prefab.GetComponent<Missile>() : null;
            return missile != null ? missile.GetWeaponInfo() : null;
        }

        private static (MissileDefinition, WeaponInfo, GameObject)? CreateMissileVariant(Encyclopedia enc, VariantInfo variant)
        {
            GameObject sourceMissileGO = Resources.FindObjectsOfTypeAll<GameObject>()
                .FirstOrDefault(go => go != null && go.transform.parent == null && go.name == variant.SourceMissile && go.GetComponent<Missile>() != null);
            if (sourceMissileGO == null) return null;

            Missile sourceMissile = sourceMissileGO.GetComponent<Missile>();
            WeaponInfo sourceWeaponInfo = sourceMissile.GetWeaponInfo();
            MissileDefinition sourceMissileDefinition = sourceMissile.definition as MissileDefinition;
            if (sourceWeaponInfo == null || sourceMissileDefinition == null) return null;

            GameObject missileClone = CleanClone(sourceMissileGO, variant.NewName);
            Missile clonedMissile = missileClone.GetComponent<Missile>();
            ApplySpeedMultiplier(clonedMissile, variant.SpeedMultiplier);

            VLSBooster booster = missileClone.GetComponentInChildren<VLSBooster>(true);
            if (booster != null) UnityEngine.Object.Destroy(booster.gameObject);

            MissileDefinition missileDefinition = UnityEngine.Object.Instantiate(sourceMissileDefinition);
            missileDefinition.name = variant.NewName;
            missileDefinition.jsonKey = variant.NewName;
            missileDefinition.unitName = variant.DisplayName;
            missileDefinition.description = variant.Description;
            missileDefinition.unitPrefab = missileClone;
            missileDefinition.dontAutomaticallyAddToEncyclopedia = false;
            Traverse.Create(clonedMissile).Field("definition").SetValue(missileDefinition);

            WeaponInfo info = UnityEngine.Object.Instantiate(sourceWeaponInfo);
            info.name = $"{variant.NewName}_info";
            info.weaponName = variant.DisplayName;
            info.shortName = variant.ShortName;
            info.massPerRound = variant.Mass;
            info.costPerRound = variant.Cost;
            info.description = variant.Description;
            info.weaponPrefab = missileClone;
            info.blastDamage *= variant.ExplosiveMultiplier;
            info.pierceDamage += variant.PenetrationBonus;
            Traverse.Create(clonedMissile).Field("info").SetValue(info);

            Traverse blastYieldTraverse = Traverse.Create(clonedMissile).Field("blastYield");
            blastYieldTraverse.SetValue(blastYieldTraverse.GetValue<float>() * variant.ExplosiveMultiplier);

            Traverse pierceDamageTraverse = Traverse.Create(clonedMissile).Field("pierceDamage");
            pierceDamageTraverse.SetValue(pierceDamageTraverse.GetValue<float>() + variant.PenetrationBonus);

            enc.missiles.Add(missileDefinition);

            return (missileDefinition, info, missileClone);
        }

        private static WeaponMount CreateMountVariant(Encyclopedia enc, WeaponMount sourceMount, string newMountName, VariantInfo variant, WeaponInfo info, GameObject missileClone)
        {
            if (sourceMount.prefab == null) return null;

            GameObject mountClone = CleanClone(sourceMount.prefab, newMountName);

            foreach (var mounted in mountClone.GetComponentsInChildren<MountedMissile>(true))
                Traverse.Create(mounted).Field("info").SetValue(info);
            foreach (var cargo in mountClone.GetComponentsInChildren<MountedCargo>(true))
                Traverse.Create(cargo).Field("info").SetValue(info);

            WeaponMount mount = UnityEngine.Object.Instantiate(sourceMount);
            mount.name = newMountName;
            mount.jsonKey = newMountName;
            mount.mountName = variant.DisplayName;
            mount.prefab = mountClone;
            mount.info = info;
            mount.dontAutomaticallyAddToEncyclopedia = false;
            try { mount.Initialize(); }
            catch (Exception ex) { TorpedoPlugin.ModLogger.LogWarning("[Torpedo] WeaponMount.Initialize failed for " + newMountName + ": " + ex.Message); }

            enc.weaponMounts.Add(mount);

            return mount;
        }

        private static void ApplySpeedMultiplier(Missile missile, float speedMultiplier)
        {
            float thrustMultiplier = Mathf.Sqrt(speedMultiplier);
            float finAreaMultiplier = Mathf.Min(1f / speedMultiplier, 8f);

            Traverse finAreaTraverse = Traverse.Create(missile).Field("finArea");
            finAreaTraverse.SetValue(finAreaTraverse.GetValue<float>() * finAreaMultiplier);

            object motorsObj = Traverse.Create(missile).Field("motors").GetValue();
            if (!(motorsObj is Array motorsArray)) return;

            foreach (object motor in motorsArray)
            {
                if (motor == null) continue;
                Traverse motorTraverse = Traverse.Create(motor);
                motorTraverse.Field("thrust").SetValue(motorTraverse.Field("thrust").GetValue<float>() * thrustMultiplier);
                motorTraverse.Field("topSpeed").SetValue(motorTraverse.Field("topSpeed").GetValue<float>() * speedMultiplier);

                motorTraverse.Field("burnTime").SetValue(motorTraverse.Field("burnTime").GetValue<float>() / speedMultiplier);
            }

            if (motorsArray.Length > 1)
            {
                Traverse.Create(missile).Field("motorStage").SetValue(motorsArray.Length - 1);
            }
        }

        private static GameObject CleanClone(GameObject original, string newName)
        {
            GameObject clone = UnityEngine.Object.Instantiate(original);
            clone.name = newName;
            clone.transform.SetParent(null);
            clone.hideFlags = HideFlags.HideAndDontSave;
            clone.SetActive(false);
            NetworkIdentity networkIdentity = clone.GetComponentInChildren<NetworkIdentity>();
            if (networkIdentity != null)
            {
                Traverse identityTraverse = Traverse.Create(networkIdentity);
                networkIdentity.PrefabHash = newName.GetHashCode();
                identityTraverse.Field("_hasSpawned").SetValue(false);
                identityTraverse.Method("NetworkReset", Array.Empty<object>()).GetValue();
            }
            UnityEngine.Object.DontDestroyOnLoad(clone);
            return clone;
        }

        private static void RegisterOnHardpointsCarrying(WeaponMount mount, string mountName, string sourceMountName)
        {
            var weaponManagers = Resources.FindObjectsOfTypeAll<WeaponManager>();

            foreach (var wm in weaponManagers)
            {
                if (wm.hardpointSets == null) continue;
                foreach (var hardpointSet in wm.hardpointSets)
                {
                    if (hardpointSet == null || hardpointSet.weaponOptions == null) continue;
                    if (!hardpointSet.weaponOptions.Any(m => m != null && m.name == sourceMountName)) continue;
                    if (hardpointSet.weaponOptions.Contains(mount)) continue;

                    hardpointSet.weaponOptions.Add(mount);
                    TorpedoPlugin.ModLogger.LogInfo($"[Torpedo] TorpedoMounts: registered {mountName} on {wm.gameObject.name} hardpoint '{hardpointSet.name}'");
                }
            }
        }
    }

    [HarmonyPatch(typeof(WeaponManager), "Awake")]
    [HarmonyPriority(Priority.Last)]
    public static class TorpedoMounts_WeaponManagerAwake_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(WeaponManager __instance)
        {
            try { TorpedoMounts_Patch.RegisterOnWeaponManager(__instance); }
            catch (Exception ex) { TorpedoPlugin.ModLogger.LogError("[Torpedo] TorpedoMounts WeaponManager.Awake postfix failed: " + ex); }
        }
    }

    [HarmonyPatch(typeof(WeaponChecker), "VetLoadout")]
    public static class TorpedoMounts_VetLoadout_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(AircraftDefinition definition)
        {
            try
            {
                WeaponManager wm = definition?.unitPrefab?.GetComponent<Aircraft>()?.weaponManager;
                if (wm != null) TorpedoMounts_Patch.RegisterOnWeaponManager(wm);
            }
            catch (Exception ex) { TorpedoPlugin.ModLogger.LogError("[Torpedo] TorpedoMounts VetLoadout prefix failed: " + ex); }
        }
    }
}
