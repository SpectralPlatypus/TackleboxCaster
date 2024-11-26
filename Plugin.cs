using BepInEx;
using HarmonyLib;
using HarmonyLib.Tools;
using System;

namespace TackleboxDbg
{
    static class ModInfo
    {
        public const string PLUGIN_GUID = "TackleboxCaster";
        public const string PLUGIN_NAME = "TackleboxCaster";
        public const string PLUGIN_VERSION = "1.2.0";
    }

    [BepInPlugin(ModInfo.PLUGIN_GUID, ModInfo.PLUGIN_NAME, ModInfo.PLUGIN_VERSION)]
    [BepInProcess("The Big Catch Tacklebox.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {ModInfo.PLUGIN_NAME} is loaded!");

            var harmony = new Harmony(ModInfo.PLUGIN_GUID);
            try
            {
                harmony.PatchAll();
            } 
            catch (Exception ex)
            {
                Logger.LogError((object)("Failed to patch: " + ex));
            }
        }

    }
}
