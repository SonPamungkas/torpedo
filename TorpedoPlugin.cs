using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace Torpedo
{
    [BepInPlugin("neutral.torpedo", "Torpedo", "1.1.0")]
    public class TorpedoPlugin : BaseUnityPlugin
    {
        public static TorpedoPlugin Instance;
        public static ManualLogSource ModLogger;

        private void Awake()
        {
            Instance = this;
            ModLogger = base.Logger;

            var harmony = new Harmony("neutral.torpedo");
            harmony.PatchAll();

            ModLogger.LogInfo("[Torpedo] Torpedo mod loaded!");
        }
    }
}
