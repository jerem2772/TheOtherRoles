using System;
using HarmonyLib;
using PowerTools;
using UnityEngine;
using System.Linq;
using Hazel;

// ------------------------------------
// ------------- Try Hard -------------
// ------------------------------------
// Based on AmongUsTryhard
// https://github.com/Herysia/AmongUsTryhard



namespace TheOtherRoles
{
    
    // ------------------------------------------------------------------------
    // ------------------------------ Scan Abuse ------------------------------
    // ------------------------------------------------------------------------
    internal class ScannerPatches
    {
        [HarmonyPatch(typeof(MedScanMinigame), nameof(MedScanMinigame.FixedUpdate))]
        public static class MedScanMinigame_FixedUpdate
        {
            public static void Prefix(MedScanMinigame __instance)
            {
                if (MapOptions.disableScanAbuse) {
                    if (__instance.MyNormTask.IsComplete)
                    {
                        return;
                    }

                    byte playerId = PlayerControl.LocalPlayer.PlayerId;
                    if (__instance.medscan.CurrentUser != playerId)
                    {
                        __instance.medscan.CurrentUser = playerId;
                    }
                }
            }
        }
    }
    
    // -------------------------------------------------------------------------
    // ------------------------------ Vent in fog ------------------------------
    // -------------------------------------------------------------------------
    
    internal class VisibleVentPatches
    {
        public static int ShipAndObjectsMask = LayerMask.GetMask(new string[]
        {
            "Ship",
            "Objects"
        });
        
        [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))] //EnterVent
        public static class EnterVentPatch
        {
            public static bool Prefix(Vent __instance, PlayerControl pc)
            {
                if (!__instance.EnterVentAnim)
                {
                    return false;
                }
                
                var truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                
                Vector2 vector = pc.GetTruePosition() - truePosition;
                var magnitude = vector.magnitude;
                if (pc.AmOwner || magnitude < PlayerControl.LocalPlayer.myLight.LightRadius &&
                    !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude,
                        ShipAndObjectsMask))
                {
                    __instance.GetComponent<SpriteAnim>().Play(__instance.EnterVentAnim, 1f);
                }
                
                if (pc.AmOwner && Constants.ShouldPlaySfx()) //ShouldPlaySfx
                {
                    SoundManager.Instance.StopSound(ShipStatus.Instance.VentEnterSound);
                    SoundManager.Instance.PlaySound(ShipStatus.Instance.VentEnterSound, false, 1f).pitch =
                        UnityEngine.Random.Range(0.8f, 1.2f);
                }
                
                return false;
            }
        }
    
        [HarmonyPatch(typeof(Vent), nameof(Vent.ExitVent))] //ExitVent
        public static class ExitVentPatch
        {
            public static bool Prefix(Vent __instance, PlayerControl pc)
            {
                if (!__instance.ExitVentAnim)
                {
                    return false;
                }
        
                var truePosition = PlayerControl.LocalPlayer.GetTruePosition();
        
                Vector2 vector = pc.GetTruePosition() - truePosition;
                var magnitude = vector.magnitude;
                if (pc.AmOwner || magnitude < PlayerControl.LocalPlayer.myLight.LightRadius &&
                    !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude,
                        ShipAndObjectsMask))
                {
                    __instance.GetComponent<SpriteAnim>().Play(__instance.ExitVentAnim, 1f);
                }
        
                if (pc.AmOwner && Constants.ShouldPlaySfx()) //ShouldPlaySfx
                {
                    SoundManager.Instance.StopSound(ShipStatus.Instance.VentEnterSound);
                    SoundManager.Instance.PlaySound(ShipStatus.Instance.VentEnterSound, false, 1f).pitch =
                        UnityEngine.Random.Range(0.8f, 1.2f);
                }
        
                return false;
            }
        }
    }
    
    
    // ------------------------------------------------------------------------------
    // ------------------------------ CD after meeting ------------------------------
    // ------------------------------------------------------------------------------
    
    // class KillCooldownAfterMeeting
    // {
    //     [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))] //WrapUp
    //     static class ExileController_WrapUp
    //     {
    //         [HarmonyPriority(Priority.Last)]
    //         static void Postfix()
    //         {
    //             if (DestroyableSingleton<TutorialManager>.InstanceExists || !ShipStatus.Instance.IsGameOverDueToDeath())
    //             {
    //                 PlayerControl.LocalPlayer.killTimer *= 50 / 100f;
    //             }
    //         }
    //     }
    // }
    
    
    // ---------------------------------------------------------------------------
    // ------------------------------ Block Options ------------------------------
    // ---------------------------------------------------------------------------

    public class BlockUtilitiesPatches
    {
        private static bool vitalsBlocked = true;
        public static bool adminBlocked = true;
        private static bool camsBlocked = true;

        // --------------------------
        // --------- Vitals ---------
        // --------------------------
        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
        public static class VitalsMinigameUpdate
        {
            public static bool Prefix(VitalsMinigame __instance)
            {
                bool commsActive = false;
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                    if (task.TaskType == TaskTypes.FixComms)
                        commsActive = true;

                __instance.SabText.color = Color.white;

                if (commsActive)
                {
                    __instance.SabText.text = "[ C O M M S  D I S A B L E D ]";
                    __instance.SabText.SetFaceColor(Palette.ImpostorRed);
                }
                else
                {
                    __instance.SabText.text = "[ V I T A L S  D E S A T I V A T É ]\n\nAu-dessus de " +
                                              CustomOptionHolder.maxPlayerVitals.getFloat() + " joueurs";
                    __instance.SabText.SetFaceColor(new Color32(255, 200, 0, Byte.MaxValue));
                }

                if (!__instance.SabText.isActiveAndEnabled && vitalsBlocked)
                {
                    __instance.SabText.gameObject.SetActive(true);
                    for (int j = 0; j < __instance.vitals.Length; j++)
                    {
                        __instance.vitals[j].gameObject.SetActive(false);

                    }
                }

                return !vitalsBlocked;
            }
        }
        
        // -------------------------
        // --------- Admin ---------
        // -------------------------
        
        //Gestion of admin are integrated in UsablesPatch
        
        // ----------------------------
        // --------- Security ---------
        // ----------------------------
        [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Update))]
        public static class PlanetSurveillanceMinigameUpdate
        {
            public static bool Prefix(PlanetSurveillanceMinigame __instance)
            {
                bool commsActive = false;
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                    if (task.TaskType == TaskTypes.FixComms) commsActive = true; 
                
                __instance.SabText.color = Color.white;
                if (commsActive) {
                    __instance.SabText.text = "[ C O M M S  D I S A B L E D ]";
                    __instance.SabText.SetFaceColor(Palette.ImpostorRed);
                } else {
                    __instance.SabText.text = "[ C A M  D E S A T I V A T É E S ]\n\nAu-dessus de " + CustomOptionHolder.maxPlayerCams.getFloat() + " joueurs";
                    __instance.SabText.SetFaceColor(new Color32(255, 200, 0, Byte.MaxValue));
                }
                
                //Toggle ON/OFF depending on minPlayerCams parameter
                if (!__instance.isStatic && camsBlocked)
                {
                    __instance.isStatic = true;
                    __instance.ViewPort.sharedMaterial = __instance.StaticMaterial;
                    __instance.SabText.gameObject.SetActive(true);
                    
                }

                return !camsBlocked;
            }
        }

        [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.NextCamera))]
        public static class PlanetSurveillanceMinigameNextCamera
        {
            public static bool Prefix(PlanetSurveillanceMinigame __instance, int direction)
            {
                if (camsBlocked)
                {
                    if (direction != 0 && Constants.ShouldPlaySfx())
                    {
                        SoundManager.Instance.PlaySound(__instance.ChangeSound, false, 1f);
                    }

                    __instance.Dots[__instance.currentCamera].sprite = __instance.DotDisabled;

                    __instance.currentCamera = Extensions.Wrap(__instance.currentCamera + direction,
                        __instance.survCameras.Length);
                    __instance.Dots[__instance.currentCamera].sprite = __instance.DotEnabled;
                    SurvCamera survCamera = __instance.survCameras[__instance.currentCamera];
                    __instance.Camera.transform.position = survCamera.transform.position +
                                                           __instance.survCameras[__instance.currentCamera].Offset;
                    __instance.LocationName.text = survCamera.CamName;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Update))]
        public static class SurveillanceMinigameUpdate
        {
            public static bool Prefix(SurveillanceMinigame __instance)
            {
                
                bool commsActive = false;
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                    if (task.TaskType == TaskTypes.FixComms) commsActive = true;

                for (int j = 0; j < __instance.ViewPorts.Length; j++)
                {
                    __instance.SabText[j].color = Color.white;
                    if (commsActive)
                    {
                        __instance.SabText[j].text = "[ C O M M S  D I S A B L E D ]";
                        __instance.SabText[j].SetFaceColor(Palette.ImpostorRed);
                    }
                    else
                    {
                        __instance.SabText[j].text = "[ C A M  D E S A T I V A T É E S ]\n\nAu-dessus de " +
                                                    CustomOptionHolder.maxPlayerCams.getFloat() + " joueurs";
                        __instance.SabText[j].SetFaceColor(new Color32(255, 200, 0, Byte.MaxValue));
                    }
                }

                //Toggle ON/OFF depending on minPlayerCams parameter
                if (!__instance.isStatic && camsBlocked)
                {
                    __instance.isStatic = true;
                    for (int j = 0; j < __instance.ViewPorts.Length; j++)
                    {
                        __instance.ViewPorts[j].sharedMaterial = __instance.StaticMaterial;
                        __instance.SabText[j].gameObject.SetActive(true);
                    }
                }
                
                return !camsBlocked;
            }
        }

        public static void udpateBools(int playersLeft, string here)
        {
            vitalsBlocked = playersLeft > CustomOptionHolder.maxPlayerVitals.getFloat();
            adminBlocked = playersLeft > CustomOptionHolder.maxPlayerAdmin.getFloat();
            camsBlocked = playersLeft > CustomOptionHolder.maxPlayerCams.getFloat();
            // System.Console.WriteLine("Methode : " + here);
            // System.Console.WriteLine("PlayerControl Count : " + PlayerControl.AllPlayerControls.Count);
            // System.Console.WriteLine("Players Left : " + playersLeft);
            // System.Console.WriteLine("Admin : " + adminBlocked + " / " + CustomOptionHolder.maxPlayerAdmin.getFloat() + "\nVitals : " + vitalsBlocked + " / " + CustomOptionHolder.maxPlayerVitals.getFloat() + "\nCams : " + camsBlocked + " / " + CustomOptionHolder.maxPlayerCams.getFloat());

        }

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
        public static class ExileControllerBegin
        {
            public static void Postfix(ExileController __instance, GameData.PlayerInfo exiled)
            {
                int playersLeft = PlayerControl.AllPlayerControls.ToArray().Count(pc =>
                    !pc.Data.IsDead && !pc.Data.Disconnected) - (exiled != null ? 1 : 0);
                udpateBools(playersLeft, "1");
            }
        }
        
        [HarmonyPatch(typeof(InnerNet.InnerNetClient), nameof(InnerNet.InnerNetClient.HandleMessage))]
        public static class InnerNetClientHandleMessage
        {
            public static void Postfix(InnerNet.InnerNetClient __instance, MessageReader reader, SendOption sendOption)
            {
                //MessageReader reader = reader;
                if (reader.Tag != 2) return;


                int playersLeft = PlayerControl.AllPlayerControls.ToArray().Count(pc =>
                    !pc.Data.IsDead && !pc.Data.Disconnected);
                udpateBools(playersLeft, "2");
            }
        }
    }
}