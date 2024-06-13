using BepInEx.Configuration;
using LethalConfig;
using LethalConfig.ConfigItems;
using System.Text.RegularExpressions;
using UnityEngine.Windows;

namespace LethalAnalytics
{
    public static class AnalyticsManager
    {
        /// <summary>
        /// Whether analytics collection is enabled or disabled globally.
        /// </summary>
        public static bool telemetricsEnabled
        {
            get {  return (bool)LethalAnalytics.enableTele.BoxedValue; }
        }

        /// <summary>
        /// Call this method to set up Google Analytics (GA4) collection for your mod. It's recommended to call this when your plugin starts (inside the Awake() method).
        /// </summary>
        /// <param name="modId">A unique static ID for your mod (ex. net.RB007.LethalMod). This should NEVER change. It's used to send user update events and check if a user is new.</param>
        /// <param name="modName">The name of your mod. This should also NEVER change (unless the public name of your mod somehow changes). Must only contain chars A-z, 0-9, and spaces (no special chars). This is the name users will see inside the LethalAnalytics config.</param>
        /// <param name="modVersion">Your mod's version (ex. 1.0.0). This will be displayed in your GA4 reports and is used to send user update events. This should change when your mod version changes.</param>
        /// <param name="modDesc">A short description for your mod. Please explain what data/events you wish to collect AND WHY you'll be collecting them. NEVER collect PII or otherwise private info. Please be transparent and respect user privacy. Make the Lethal Company modding community proud!</param>
        /// <param name="measurementId">Your GA4 measurement ID (ex. G-RB007ONKOFI). Check that this is correct. Be careful about sharing it and feel free to hide/obfuscate it in your code if desired.</param>
        /// <param name="screenTitle">The title name to send when a session is created for GA4 (default: MainMenu).</param>
        /// <param name="renewScreenTitle">The title name to send when a session is continued for GA4 (default: InGame).</param>
        /// <param name="sendSystemInfo">Whether to send the user's CPU and GPU type along with the GA4 session_start event (as event params). OS info and screen res. will be sent regardless.</param>
        /// <param name="sessionLengthMins">How long a GA4 user session is set to last in minutes (default: 30). By default, sessions are re-created if they timeout (as long as the game is still open). Only change this if you know what you're doing.</param>
        /// <returns>This method returns an instance of GASession. You can use this class instance to send custom events to GA4 and check whether telemetrics have been disabled for this particular property.</returns>
        public static GASession registerGASession(string modId, string modName, string modVersion, string modDesc, string measurementId, string screenTitle = "MainMenu", string renewScreenTitle = "InGame", bool sendSystemInfo = true, int sessionLengthMins = 30)
        {
            // cleans modName so that it's supported by LethalConfig
            modName = Regex.Replace(modName, @"[^A-Za-z0-9 ]", "");
            modName = modName.Trim();
            // should be moved to its own method once other analytics sites are supported
            ConfigEntry<bool> enableTele = LethalAnalytics.configFile.Bind("Mods", modName, true, modDesc + "\n\n[Important: This entry indicates whether a mod receives analytics updates. Consult the description above.]");
            var cb_eT = new BoolCheckBoxConfigItem(enableTele, requiresRestart: false);
            LethalConfigManager.AddConfigItem(cb_eT);

            GASession gaSession = new GASession(modId, modName, modVersion, measurementId, screenTitle, renewScreenTitle, sendSystemInfo, enableTele, sessionLengthMins);

            LethalAnalytics.mls.LogInfo(modName + " v" + modVersion + " successfully registered a GA property!");
            return gaSession;
        }
    }
}
