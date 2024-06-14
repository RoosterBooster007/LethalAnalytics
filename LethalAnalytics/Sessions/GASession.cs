using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LethalAnalytics
{
    public class GASession
    {
        private string modId;
        private string modName;
        private string modVersion;
        private string measurementId;
        private string screenTitle;
        private string renewScreenTitle;
        private bool sendSystemInfo;
        private ConfigEntry<bool> enableTele;
        private int sessionLengthMins;
        private HttpClient HTTPclient = new HttpClient();
        private DateTimeOffset sessionStartTime = DateTimeOffset.UtcNow;
        private DateTimeOffset lastEng = DateTimeOffset.UtcNow;
        private int GAHitCount = 1;
        private int GASessionCount = 1;
        private string GASessionID = Regex.Replace(System.Guid.NewGuid().ToString(), "[^\\d]", "").Substring(0, 10);

        /// <summary>
        /// Whether analytics collection is enabled or disabled for this specific GA4 property.
        /// </summary>
        public bool telemetricsEnabled
        {
            get { return (bool)enableTele.BoxedValue; }
        }

        internal GASession(string modId, string modName, string modVersion, string measurementId, string screenTitle, string renewScreenTitle, bool sendSystemInfo, ConfigEntry<bool> enableTele, int sessionLengthMins)
        {
            this.modId = modId;
            this.modName = modName;
            this.modVersion = modVersion;
            this.measurementId = measurementId;
            this.screenTitle = screenTitle;
            this.renewScreenTitle = renewScreenTitle;
            this.sendSystemInfo = sendSystemInfo;
            this.enableTele = enableTele;
            this.sessionLengthMins = sessionLengthMins;
            Patches.mainMenuLaunched += OnMenuLaunch;
            Patches.userClosingLC += OnLCClosing;

            if (isFirstRun())
            {
                SendUserGA("page_view", true, "&_fv=1");
            } else
            {
                SendUserGA("page_view", true, "");
            }
        }

        private void OnLCClosing(object sender, EventArgs e)
        {
            sendGAEvent("state", "session_end", new Dictionary<string, string>() { ["closed"] = "true" });
        }

            private void OnMenuLaunch(object sender, EventArgs e)
        {
            if (recentlyUpdated())
            {
                sendGAEvent("state", "update", new Dictionary<string, string>() { ["version"] = modVersion });
            }
        }

        private static string getSystemArch()
        {
            if (Environment.Is64BitProcess)
            {
                return "x86_64";
            }
            else
            {
                return "x86";
            }
        }

        private static string getSystemBits()
        {
            if (Environment.Is64BitProcess)
            {
                return "64";
            }
            else
            {
                return "32";
            }
        }

        private bool isFirstRun()
        {
            if (!PlayerPrefs.HasKey(modId + ".newUser"))
            {
                PlayerPrefs.SetInt(modId + ".newUser", 1);
                PlayerPrefs.Save();
                return true;
            }
            return false;
        }

        private bool recentlyUpdated()
        {
            if (PlayerPrefs.GetString(modId + ".v") != modVersion)
            {
                PlayerPrefs.SetString(modId + ".v", modVersion);
                PlayerPrefs.Save();
                return true;
            }
            return false;
        }
        private void startNewGASession()
        {
            GASessionCount++;
            GASessionID = Regex.Replace(System.Guid.NewGuid().ToString(), "[^\\d]", "").Substring(0, 10);
            sessionStartTime = DateTimeOffset.UtcNow;
            lastEng = DateTime.UtcNow;
            GAHitCount = 1;
            SendUserGA("page_view", false, "");
        }

        /// <summary>
        /// Call this method to send custom GA4 events (category, name, params).
        /// </summary>
        /// <param name="event_category">The category/type for the event (ex. client, state, event, etc.).</param>
        /// <param name="event_name">The name of the event (ex. jeb_attack, host, emote, etc.). Check https://support.google.com/analytics/answer/13316687 for help. Do not use reserved prefixes or event names (type: web).</param>
        /// <param name="event_params">A dictionary of string key/value pairs that are sent as GA4 event parameters. Check https://support.google.com/analytics/answer/13316687 for help. Do not use reserved prefixes or parameter names (type: web).</param>
        /// <param name="isEngaged">Whether the event should send user engagement time with it (and prolong the current session). It's recommended to keep this set to true.</param>
        public void sendGAEvent(string event_category, string event_name, Dictionary<string, string> event_params, bool isEngaged = true)
        {
            if ((bool)LethalAnalytics.enableTele.BoxedValue && (bool)enableTele.BoxedValue)
            {
                if ((DateTime.UtcNow - lastEng).TotalMinutes >= sessionLengthMins)
                {
                    startNewGASession();
                }

                event_name = WebUtility.UrlEncode(event_name);
                event_category = WebUtility.UrlEncode(event_category);

                string e_params = "";
                foreach (KeyValuePair<string, string> kvp in event_params)
                {
                    if (int.TryParse(kvp.Value, out int value))
                    {
                        e_params += "&epn." + WebUtility.UrlEncode(kvp.Key) + "=" + value;
                    }
                    else
                    {
                        e_params += "&ep." + WebUtility.UrlEncode(kvp.Key) + "=" + WebUtility.UrlEncode(kvp.Value);
                    }
                }

                string engagedTime = "";
                if (isEngaged)
                {
                    engagedTime += "&_et = " + (long)Math.Floor((DateTime.UtcNow - lastEng).TotalMilliseconds);
                }

                GAHitCount++;
                _ = HTTPclient.GetAsync("https://www.google-analytics.com/g/collect?v=2&tid=" + measurementId + "&cid=" + SystemInfo.deviceUniqueIdentifier + "&sid=" + GASessionID + "&ul=" + CultureInfo.CurrentCulture.Name + "&sr=" + Screen.currentResolution.width + "x" + Screen.currentResolution.height + "&uap=" + WebUtility.UrlEncode(Environment.OSVersion.Platform.ToString()) + "&uam=" + WebUtility.UrlEncode(Application.version) + "&uapv=" + WebUtility.UrlEncode(modVersion) + "&en=" + event_name + "&ep.category=" + event_category + e_params + engagedTime + "&tfd=" + (long)Math.Floor((DateTime.UtcNow - sessionStartTime).TotalMilliseconds) + "&uamb=0&uaw=0&pscdl=noapi&_s=" + GAHitCount + "&sct=" + GASessionCount + "&seg=1&_ee=1&npa=0&dma=0&frm=0&are=1");
                if (!LethalAnalytics.recentModList.Contains(modName))
                {
                    LethalAnalytics.recentModList.Add(modName);
                }
                LethalAnalytics.totalEventsSent++;
                if (isEngaged)
                {
                    lastEng = DateTime.UtcNow;
                }
            }
        }

        private void SendUserGA(string e, bool startingSession, string args)
        {
            if ((bool)LethalAnalytics.enableTele.BoxedValue && (bool)enableTele.BoxedValue)
            {
                string sysInfo = "";
                string eventTitle = "";

                if (sendSystemInfo)
                {
                    sysInfo += "&ep.cpu=" + SystemInfo.processorType + "&ep.gpu=" + SystemInfo.graphicsDeviceName;
                }

                if (startingSession)
                {
                    eventTitle = screenTitle;
                } else
                {
                    eventTitle = renewScreenTitle;
                }

                _ = HTTPclient.GetAsync("https://www.google-analytics.com/g/collect?v=2&tid=" + measurementId + "&cid=" + SystemInfo.deviceUniqueIdentifier + "&sid=" + GASessionID + "&ul=" + CultureInfo.CurrentCulture.Name + "&sr=" + Screen.currentResolution.width + "x" + Screen.currentResolution.height + "&uap=" + WebUtility.UrlEncode(Environment.OSVersion.Platform.ToString()) + "&uam=" + WebUtility.UrlEncode(Application.version) + "&uapv=" + WebUtility.UrlEncode(modVersion) + "&en=" + e + sysInfo + "&tfd=" + (long)Math.Floor((DateTime.UtcNow - sessionStartTime).TotalMilliseconds) + "&uaa=" + getSystemArch() + "&uab=" + getSystemBits() + "&uamb=0&uaw=0&dt=" + WebUtility.UrlEncode(eventTitle + " (v" + modVersion + ")") + "&are=1&frm=0&pscdl=noapi&seg=1&npa=0&_s=1&sct=" + GASessionCount + "&dma=0&_ss=1&_nsi=1&_ee=1" + args);
                if (!LethalAnalytics.recentModList.Contains(modName))
                {
                    LethalAnalytics.recentModList.Add(modName);
                }
                LethalAnalytics.totalEventsSent++;
            }
        }
    }
}
