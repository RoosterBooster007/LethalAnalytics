<img src="https://github.com/RoosterBooster007/LethalAnalytics/blob/master/icon.png?raw=true" width="160px" height="160px"></img>

# LethalAnalytics
[![Thunderstore Version](https://img.shields.io/thunderstore/v/RB007/LethalAnalytics?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/lethal-company/p/RB007/LethalAnalytics/)
[![Thunderstore Downloads](https://img.shields.io/thunderstore/dt/RB007/LethalAnalytics?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/lethal-company/p/RB007/LethalAnalytics/)

Allows mod creators to see how their mods are used, easily catch errors, and learn about their audience.

This is a mod library or dependency. **If you're playing with this mod installed, check out its LethalConfig for more info.**

## Dependencies
| Required | Name | Version |
|:--------:|:-------------|:------|
| ✅ | [LethalConfig](https://thunderstore.io/c/lethal-company/p/AinaVT/LethalConfig/versions/#1.4.2:~:text=1.4.2) | 1.4.2 |
| ✅ | [BepInExPack](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/versions/#:~:text=5.4.2100) | 5.4.2100 |

## Overview
- [Supported Analytics Sites](#supported-analytics-sites)
- [Usage](#usage)
  - [Setting up](#setting-up)
  - [Creating a GA session](#creating-a-ga-session)
  - [Sending GA events](#sending-ga-events)
  - [Creating a README note](#creating-a-readme-note)
  - [Creating a GA4 property](#creating-a-ga4-property)
- [Telemetrics](#telemetrics)
- [Issues and Contributions](#issues-and-contributions)
- [Licencing](#licencing)

## Supported Analytics Sites:
- Google Analytics (GA4) [Free]

![GA realtime users screen example](https://i.ibb.co/NmXyzhf/GA.png)
![GA event params example gif](https://i.ibb.co/xh99Jkg/GA-event.gif)

-
  - User & new user charts supported
  - Location (city) map & charts supported
  - Events (with dimensions and metrics) supported
  - Engagement time charts supported
  - Screen resolution chart supported

- More coming soon (upon request)

## Usage
### Setting up
First, reference the ``LethalAnalytics.dll`` file (downloaded from [Thunderstore](https://thunderstore.io/c/lethal-company/p/RB007/LethalAnalytics/) or [Github](https://github.com/RoosterBooster007/LethalAnalytics/)) from your project.

In order to access the LethalAnalytics API, use the ``LethalAnalytics`` namespace:
```c#
using LethalAnalytics;
```
Also include LethalAnalytics as a ``BepInDependency`` of your mod so that BepInEx can work its dependency graphing magic (and properly load LethalAnalytics):
```c#
[BepInPlugin(modGUID, modName, modVersion)]
[BepInDependency("net.RB007.LethalAnalytics")]   <--- (right here)
public class YourHopefullyErrorFreeModOrSmth : BaseUnityPlugin { ... }
```

### Creating a GA session
Reference the static ``AnalyticsManager`` class and call the ``registerGASession(...)`` method to connect your plugin to Google Analytics (GA4):
```c#
GASession gaSession = AnalyticsManager.registerGASession("net.YourUsername.YourMod", "Your Mod Name", "1.0.0", "Your mod desc. that explains what data you will collect and WHY you're collecting it.", "G-YOURMEASUREMENTID");
```
See [Creating a GA4 property](https://github.com/RoosterBooster007/LethalAnalytics/#Creating-a-GA4-property) if you need a ``MEASUREMENT ID``.

If needed, you can register multiple GA sessions (and still send events to each, separately).
Please be careful about changing your ``modId``, ``modName``, and ``modVersion``. This can cause issues with session creation and user consent settings.

Here's some documentation about its parameters:
| Parameter | Type | Description |
|:--------|:-----:|:-------------|
| modId | string | A unique static ID for your mod (ex. net.RB007.LethalMod). This should NEVER change. It's used to send user update events and check if a user is new. |
| modName | string | The name of your mod. This should also NEVER change (unless the public name of your mod somehow changes). Must only contain chars A-z, 0-9, and spaces (no special chars). This is the name users will see inside the LethalAnalytics config. |
| modVersion | string | Your mod's version (ex. 1.0.0). This will be displayed in your GA4 reports and is used to send user update events. This should change when your mod version changes. |
| modDesc | string | A short description for your mod. Please explain what data/events you wish to collect AND WHY you'll be collecting them. <ins>**NEVER collect PII or otherwise private info.**</ins> Please be transparent and respect user privacy. Make the Lethal Company modding community proud! |
| measurementId | string | Your GA4 measurement ID (ex. G-RB007ONKOFI). Check that this is correct. Be careful about sharing it and feel free to hide/obfuscate it in your code if desired. |
| screenTitle | string (optional) | The title name to send when a session is created for GA4 (default: MainMenu). |
| renewScreenTitle | string (optional) | The title name to send when a session is continued for GA4 (default: InGame). |
| sendSystemInfo | bool (optional) | Whether to send the user's CPU and GPU type along with the GA4 session_start event (as event params). OS info and screen res. will be sent regardless. |
| sessionLengthMins | int (optional) | How long a GA4 user session is set to last in minutes (default: 30). By default, sessions are re-created if they timeout (as long as the game is still open). Only change this if you know what you're doing. |

By default, every instance of ``GASession`` sends a few user events:
| Event Name | Description |
|:--------|:-------------|
| session_start | This event is sent as soon as the ``registerGASession(...)`` method is called for each user. It starts each user's session. |
| page_visit | This event is sent as soon as the ``registerGASession(...)`` method is called for each user. Includes some basic version info (and their CPU (type) and GPU (type) if enabled). |
| first_visit | This event is sent as soon as ``registerGASession(...)`` method is called for each user (for the first time). It's what lets you know if a user ran your mod for the first time. |
| update | This event is sent as soon as each user reaches Lethal Company's main menu (and they just updated your mod). It's what lets you know if a user recently updated your mod (and to what version). |
| session_end | This event is sent as soon as each user quits Lethal Company (through the main menu). It ends each user's session. |

### Sending GA events
In order to send GA events, you'll need an instance of a ``GASession``. It's recommended that you store the previously shown GASession instance in an internal static variable. That way, you can safely access it throughout your mod assembly.
```c#
internal static GASession gaSession = AnalyticsManager.registerGASession(...);
```
Then, call the ``SendGAEvent(...)`` method, from your ``GASession`` instance, to send custom GA events to your property.
```c#
gaSession.SendGAEvent("category", "name", new Dictionary<string, string>() { ["event_param"] = "event_value" });
```
Please be careful about sending events too quickly and keep event params minimal. Your mod shouldn't dramatically affect a user's networking.

Here's some documentation about its parameters:
| Parameter | Type | Description |
|:--------|:-----:|:-------------|
| event_category | string | The category/type for the event (ex. client, state, event, etc.). |
| event_name | string | The name of the event (ex. jeb_attack, host, emote, etc.). Check https://support.google.com/analytics/answer/13316687 for help. Do not use reserved prefixes or event names (type: web). |
| event_params | Dictionary<string, string> | A dictionary of string key/value pairs that are sent as GA4 event parameters. Check https://support.google.com/analytics/answer/13316687 for help. Do not use reserved prefixes or parameter names (type: web). |
| isEngaged | bool (optional) | Whether the event should send user engagement time with it (and prolong the current session). It's recommended to keep this set to true. |

And here's an example of it in action (in the wild 😲):
```c#
[HarmonyPatch(typeof(GrabbableObject))]
public class ItemPatch
{
  [HarmonyPatch("OnBroughtToShip")]
  [HarmonyPostfix]
  public static void patchCollect(ref Item ___itemProperties, ref int ___scrapValue)
  {
    YourModClass.gaSession.SendGAEvent("event", "collect", new Dictionary<string, string>() { ["item_name"] = ___itemProperties.itemName, ["scrap_value"] = ___scrapValue.ToString(), ["elapsed"] = StartOfRound.Instance.timeSinceRoundStarted.ToString() });
  }
}
```
The event logs when a user brings a new scrap item to the ship and sends its name, value, and the time since the ship landed, to the GA property.

### Creating a README note
> [!IMPORTANT] 
> When using LethalAnalytics, it's extremely recommended that you include some sort of README notice for your mod. 

See [Telemetrics](https://github.com/RoosterBooster007/LethalAnalytics/#Telemetrics) for an example (below). **It's always important to be transparent and respect user privacy.** Users should know what events are being collected and why.

### Creating a GA4 property
- Navigate to ``https://analytics.google.com/`` --> ``Create`` --> ``Property``.
- Enter your ``property name`` and ``time zone`` --> ``Next``.

![Create property screen](https://i.ibb.co/gzHV1MH/create-property.png)
- Select an industry (ex. ``Games``) and business size (ex. ``Small``) --> ``Next``.

![Business info screen](https://i.ibb.co/BNNfXPd/business.png)
- Select (at least) `Examine user behavior` --> ``Create``.

![Select intent screen](https://i.ibb.co/gwn3B8w/eub.png)
- Choose the ``Web`` platform. --> Enter your ``modId`` (ex. net.YourUsername.YourMod) as the ``URL`` and type your mod name as the ``Stream name`` (verify that ``Enhanced measurement`` is on).

![Web platform img](https://i.ibb.co/RpGBJxp/cap.png)

![Create web stream screen](https://i.ibb.co/SyyhNGV/web.png)
- Select your newly-created ``web stream`` --> Copy the ``MEASUREMENT ID``.

![Copy mid image](https://i.ibb.co/X5XZ2q4/mid.png)
- That's it. You're all set! Use your ``MEASUREMENT ID`` when registering a GA session above. Once your users run your mod, they'll show up in your ``Realtime`` (and normal) reports with any sent events.

> [!TIP]
> Generally, it's best to keep your MEASUREMENT ID safe. Anyone with it can send data to your property (Google does a decent job at removing bots/spam, though). Feel free to hide/obfuscate it in your code.

## Telemetrics
Unless turned off in the config, LethalAnalytics may upload small amounts of Lethal Company and LAs user data. This helps me understand what features are used the most and what I can improve.

**What's collected?**
- OS, LC, and LethalAnalytics version data
- Your CPU (type), GPU (type), and screen resolution
- The names of mods that use LAs, the amount of events sent (but not their contents), and a few game events
- Your language and region (ex. en-US, Chicago)

> [!IMPORTANT]
> Your Steam username is NOT sent (to keep things anon.).

This section doesn't include analytics/telemetrics data collected by other mods. Consult each mod README and their config description for more info.

## Issues and Contributions
Please follow the available issue templates when submitting requests. Use the discussions tab when wanting to chat. Pull requests (to the ```dev``` branch) and other user contributions are encouraged and always welcome.

## Licencing
Read the **LICENSE** file for more information. The **Contributor Agreement** can be found there too. By downloading or contributing to this repo, you agree to the terms stated in the **LICENSE** file.

```Copyright (C) 2024 RoosterBooster007```

> [!IMPORTANT]
> Please include the above copyright notice when distributing or modifying any code.
