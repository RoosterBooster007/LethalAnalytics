using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalConfig;
using LethalConfig.ConfigItems;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace LethalAnalytics
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("ainavt.lc.lethalconfig")]
    public class LethalAnalytics : BaseUnityPlugin
    {
        private const string modGUID = "net.RB007.LethalAnalytics";
        private const string modName = "LethalAnalytics";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);
        internal static ManualLogSource mls;

        internal static int totalEventsSent = 0;
        internal static int prevTotalEventsSent = 0;
        internal static List<string> recentModList = new List<string>();

        internal Timer _timer;

        internal static ConfigEntry<bool> enableTele;
        internal static ConfigEntry<int> eventsSent;

        internal static ConfigFile configFile;

        internal static GASession gaSession;

        void Awake()
        {
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo("LethalAnalytics is enabling...");

            mls.LogInfo("Reading config...");
            enableTele = Config.Bind("General", "Enable telemetry", true, "When enabled, some anonymous user data/input actions may be uploaded to various analytics sites. This helps mod creators understand what features are used the most and what they can improve. :) \n\nCheck each mod listed below to see what data is collected.\n\nSupported analytics sites:\n - Google Analytics");
            var cb_eT = new BoolCheckBoxConfigItem(enableTele, requiresRestart: false);
            LethalConfigManager.AddConfigItem(cb_eT);
            eventsSent = Config.Bind("Info", "Events sent this session", totalEventsSent, "[Read-only] This shows how many total analytics events have been sent, via the plugins listed below, while you've been online.");
            var int_eS = new IntInputFieldConfigItem(eventsSent, requiresRestart: false);
            LethalConfigManager.AddConfigItem(int_eS);
            configFile = Config;
            mls.LogInfo("Config loaded!");

            mls.LogInfo("Patching game...");
            harmony.PatchAll();
            mls.LogInfo("Patched all!");

            Patches.quickMenuOpened += OnQMenuOpen;
            gaSession = new GASession("net.RB007.LethalAnalytics", modName, modVersion, "G-KQFXJ36ZK6", "MainMenu", "InGame", true, enableTele, 30);
            _timer = new Timer(SentEventsInfo, null, TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
            mls.LogInfo("LethalAnalytics started! CID: " + SystemInfo.deviceUniqueIdentifier);
        }

        private void SentEventsInfo(object state)
        {
            string modsList = "";
            foreach (string m in recentModList)
            {
                modsList += m + ",";
            }

            gaSession.sendGAEvent("event", "sent_events", new Dictionary<string, string>() { ["events_count"] = (totalEventsSent - prevTotalEventsSent).ToString(), ["mods_count"] = (recentModList.Count).ToString(), ["mods_list"] = modsList }, true);
            prevTotalEventsSent = totalEventsSent;
        }

        private void OnQMenuOpen(object sender, EventArgs e)
        {
            eventsSent.BoxedValue = totalEventsSent;
            gaSession.sendGAEvent("event", "menu_open", new Dictionary<string, string>() { }, true);
        }
    }
}
