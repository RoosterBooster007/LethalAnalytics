using HarmonyLib;
using System;

namespace LethalAnalytics
{
    internal class Patches
    {
        internal static event EventHandler mainMenuLaunched;
        internal static event EventHandler quickMenuOpened;
        internal static event EventHandler userClosingLC;

        [HarmonyPatch(typeof(MenuManager))]
        internal class MenuPatch
        {
            [HarmonyPatch("Awake")]
            [HarmonyPostfix]
            public static void patchUpdateEvent()
            {
                mainMenuLaunched?.Invoke(null, EventArgs.Empty);
            }
        }

        [HarmonyPatch(typeof(QuickMenuManager))]
        public class QMenuPatch
        {
            [HarmonyPatch("OpenQuickMenu")]
            [HarmonyPostfix]
            public static void patchOpen()
            {
                quickMenuOpened?.Invoke(null, EventArgs.Empty);
            }
        }

        [HarmonyPatch(typeof(GameNetworkManager))]
        public class LeavePatch
        {
            [HarmonyPatch("OnDisable")]
            [HarmonyPrefix]
            public static void patchQuit()
            {
                userClosingLC?.Invoke(null, EventArgs.Empty);
            }
        }
    }
}
