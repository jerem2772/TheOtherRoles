using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TheOtherRoles;

namespace TheOtherRoles
{
    [HarmonyPatch]
    public static class CredentialsPatch {
        public static string fullCredentials = 
$@"<color=#FCCE03FF>The Other Roles</color> <color=#6F6195FF>Le Crew</color> v{TheOtherRolesPlugin.Version.ToString()}:
Modded by <color=#FCCE03FF>Eisbison</color>
<color=#6F6195FF>Le Crew</color> By <color=#18A5FFFF>Jerem2772</color>";


        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        private static class VersionShowerPatch
        {
            static void Postfix(VersionShower __instance) {
                string spacer = new String('\n', 8);

                if (__instance.text.text.Contains(spacer))
                    __instance.text.text = __instance.text.text + "\n" + fullCredentials;
                else
                    __instance.text.text = __instance.text.text + spacer + fullCredentials;
                __instance.text.alignment = TMPro.TextAlignmentOptions.TopLeft;
            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        private static class PingTrackerPatch
        {
           

        static void Postfix(PingTracker __instance)
        { 
            if (!__instance.GetComponentInChildren<SpriteRenderer>())
            {
                    var lcObject = new GameObject("LeCrew");

                    lcObject.AddComponent<SpriteRenderer>().sprite = TheOtherRoles.getLogo("TheOtherRoles.Resources.LeCrew.png", 100f);

                    lcObject.transform.parent = __instance.transform;
                    lcObject.transform.localPosition = new Vector3(-0.8f, 0f, -1);
                    lcObject.transform.localScale *= 0.72f;

                    if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) 
                    {
                        __instance.text.text = $"<color=#FCCE03FF>The Other Roles</color> by <color=#FCCE03FF>Eisbison</color>\n<color=#6F6195FF>Le Crew : </color> by <color=#18A5FFFF>Jerem2772</color>\n" + __instance.text.text;
                        __instance.transform.localPosition = new Vector3(2f, 2.675f, __instance.transform.localPosition.z);
                    } 
                    else 
                    {

                        __instance.text.text = $"{fullCredentials}\n{__instance.text.text}"; 
                        __instance.transform.localPosition = new Vector3(1.25f, 2.675f, __instance.transform.localPosition.z);
                    }
                }
            }
        }
    }
}
