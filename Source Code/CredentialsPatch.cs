﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public static class CredentialsPatch {
        public static string fullCredentials = 
$@"<color=#FCCE03FF>The Other Roles</color> <color=#6F6195FF>Le Crew</color> v{TheOtherRolesPlugin.Version.ToString()}
<size=80%>Modded by <color=#FCCE03FF>Eisbison</color>
<color=#6F6195FF>Le Crew</color> by <color=#18A5FFFF>Jerem2772</color> & <color=#970606FF>Eno</color>";

    public static string mainMenuCredentials = 
$@"Modded by <color=#FCCE03FF>Eisbison</color>
<color=#6F6195FF>Le Crew</color> by <color=#18A5FFFF>Jerem2772</color> & <color=#970606FF>Eno</color>";

        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))] 
        private static class VersionShowerPatch
        {
            static void Postfix(VersionShower __instance) {
                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo == null) return;

                var credentials = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(__instance.text);
                credentials.transform.position = new Vector3(0, 0.1f, 0);
                credentials.SetText(mainMenuCredentials);
                credentials.alignment = TMPro.TextAlignmentOptions.Center;
                credentials.fontSize *= 0.75f;

                var version = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(credentials);
                version.transform.position = new Vector3(0, -0.25f, 0);
                version.SetText($"v{TheOtherRolesPlugin.Version.ToString()}");

                credentials.transform.SetParent(amongUsLogo.transform);
                version.transform.SetParent(amongUsLogo.transform);
            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        private static class PingTrackerPatch
        {
            
            // static void Prefix(PingTracker __instance)
            // {
            //     var lcObject = new GameObject("LeCrew");
            //     lcObject.AddComponent<SpriteRenderer>().sprite = TheOtherRoles.getLogo("TheOtherRoles.Resources.LeCrew.png", 300f);
            //     lcObject.transform.parent = __instance.transform;
            //     lcObject.transform.localPosition = new Vector3(1f, -0.1f, __instance.transform.localPosition.z);
            //     lcObject.transform.localScale *= 0.55f;
            //
            // }
            
            static void Postfix(PingTracker __instance){
                
                __instance.text.alignment = TMPro.TextAlignmentOptions.TopRight;
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) {
                    __instance.text.text = $"<color=#FCCE03FF>The Other Roles</color> <color=#6F6195FF>Le Crew</color> v{TheOtherRolesPlugin.Version.ToString()}\nBy <color=#18A5FFFF>Jerem2772</color> & <color=#970606FF>Eno</color>\n {__instance.text.text}";
                    if (PlayerControl.LocalPlayer.Data.IsDead) {
                        __instance.transform.localPosition = new Vector3(3.45f, 2.675f, __instance.transform.localPosition.z);
                    } else {
                        __instance.transform.localPosition = new Vector3(4.2f, 2.675f, __instance.transform.localPosition.z);
                    }
                } else {
                    __instance.text.text = $"{fullCredentials}\n{__instance.text.text}";
                    __instance.transform.localPosition = new Vector3(3.5f, 2.675f, __instance.transform.localPosition.z);
                }
            }
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        private static class LogoPatch
        {
            static void Postfix(PingTracker __instance) {
                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo != null) {
                    amongUsLogo.transform.localScale *= 0.6f;
                    amongUsLogo.transform.position += Vector3.up * 0.25f;
                }

                var lcObject = new GameObject("LeCrew");
                lcObject.AddComponent<SpriteRenderer>().sprite = TheOtherRoles.getLogo("TheOtherRoles.Resources.LeCrew.png", 300f);
                lcObject.transform.parent = __instance.transform;
                lcObject.transform.localPosition = new Vector3(3.8f, -1.2f, __instance.transform.localPosition.z);
                lcObject.transform.localScale *= 2.5f;



                var torLogo = new GameObject("bannerLogo_TOR");
                torLogo.transform.position = Vector3.up;
                var renderer = torLogo.AddComponent<SpriteRenderer>();
                renderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Banner.png", 300f);                                
            }
        }
    }
}