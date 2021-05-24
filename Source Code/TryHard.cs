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
    
    // ------------------------------------
    // ------------ Scan Abuse ------------
    // ------------------------------------
    internal class ScannerPatches
    {
        [HarmonyPatch(typeof(MedScanMinigame), nameof(MedScanMinigame.FixedUpdate))]
        public static class MedScanMinigame_FixedUpdate
        {
            public static void Prefix(MedScanMinigame __instance)
            {
                if (CustomOptionHolder.disableScanAbuse.getBool()) {
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
    
    // -------------------------------------
    // ------------ Vent in fog ------------
    // -------------------------------------
    
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
    
    
    // ------------------------------------------
    // ------------ CD after meeting ------------
    // ------------------------------------------
    
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
    
    
    // ---------------------------------------
    // ------------ Block Options ------------
    // ---------------------------------------
    
    public class BlockUtilitiesPatches
    {
        private static bool vitalsBool = true;
        private static bool adminBool = true;
        private static bool camsBool = true;

        [HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
        public static class VitalsMinigameUpdate
        {
            public static bool Prefix(VitalsMinigame __instance)
            {
                if (!__instance.SabText.isActiveAndEnabled && vitalsBool)
                {
                    __instance.SabText.gameObject.SetActive(true);
                    for (int j = 0; j < __instance.vitals.Length; j++)
                    {
                        __instance.vitals[j].gameObject.SetActive(false);
                    }
                }

                return !vitalsBool;
            }
        }

        [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.OnEnable))]
        public static class MapCountOverlayOnEnable
        {
            public static void Postfix(MapCountOverlay __instance)
            {
                if (adminBool)
                    __instance.BackgroundColor.SetColor(Palette.DisabledGrey);
            }
        }

        [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
        public static class MapCountOverlayUpdate
        {
            public static bool Prefix(MapCountOverlay __instance)
            {
                //Delay before display (Among Us code)
                __instance.timer += Time.deltaTime;
                if (__instance.timer < 0.1f)
                {
                    return false;
                }

                //Toggle ON/OFF depending on minPlayerAdmin parameter
                if (!__instance.isSab && adminBool)
                {
                    __instance.isSab = true;
                    __instance.BackgroundColor.SetColor(Palette.DisabledGrey);
                    __instance.SabotageText.gameObject.SetActive(true);
                }

                return !adminBool;
            }
        }
        
        // [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.OnEnable))]
        // public static class MapCountOverlayOnEnable
        // {
        //     public static void Postfix(MapCountOverlay __instance)
        //     {
        //         System.Console.WriteLine("                         OnEnable Call");
        //         __instance.isSab = true;
        //         if (adminBool)
        //         {
        //             
        //             //__instance.BackgroundColor.SetColor(Palette.DisabledGrey);
        //             // __instance.SabotageText.gameObject.SetActive(true);
        //             //__instance.SabotageText.text = "T E S T";
        //         }
        //         System.Console.WriteLine("isSab : " + __instance.isSab);
        //         System.Console.WriteLine("isActiveAndEnabled : " + __instance.isActiveAndEnabled);
        //     }
        // }
        
        // [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.OnDisable))]
        // public static class MapCountOverlayOnDisable
        // {
        //     public static void Postfix(MapCountOverlay __instance)
        //     {
        //         System.Console.WriteLine("                         OnDisable Call");
        //         if (!adminBool)
        //         {
        //             __instance.enabled = true;
        //             //__instance.BackgroundColor.SetColor(Palette.DisabledGrey);
        //             // __instance.SabotageText.gameObject.SetActive(true);
        //             //__instance.SabotageText.text = "T E S T";
        //         }
        //     }
        // }
        
        // [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
        // public static class MapCountOverlayUpdate
        // {
        //     public static bool Prefix(MapCountOverlay __instance)
        //     {
        //         
        //         System.Console.WriteLine("      Method Update Call");
        //         
        //         //Delay before display (Among Us code)
        //         __instance.timer += Time.deltaTime;
        //         if (__instance.timer < 0.1f)
        //         {
        //             return false;
        //         }
        //
        //         //Toggle ON/OFF depending on minPlayerAdmin parameter
        //         if (!__instance.isSab && adminBool) {
        //             System.Console.WriteLine("Method Update Call If");
        //             //__instance.enabled = false;
        //             //__instance.isSab = true;
        //             //__instance.BackgroundColor.SetColor(Palette.DisabledGrey);
        //             //__instance.SabotageText.gameObject.SetActive(true);
        //             //__instance.SabotageText.text = "T E S T";
        //         }
        //         System.Console.WriteLine("isSab : " + __instance.isSab);
        //         System.Console.WriteLine("Enable : " + __instance.enabled);
        //         System.Console.WriteLine("isActiveAndEnabled : " + __instance.isActiveAndEnabled);
        //         return !adminBool;
        //     }
        // }

        [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Update))]
        public static class PlanetSurveillanceMinigameUpdate
        {
            public static bool Prefix(PlanetSurveillanceMinigame __instance)
            {
                //Toggle ON/OFF depending on minPlayerCams parameter
                if (!__instance.isStatic && camsBool)
                {
                    __instance.isStatic = true;
                    __instance.ViewPort.sharedMaterial = __instance.StaticMaterial;
                    __instance.SabText.gameObject.SetActive(true);
                    __instance.SabText.text = "[C A M]\n[D E S A T I V A T E]";
                }

                return !camsBool;
            }
        }

        [HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.NextCamera))]
        public static class PlanetSurveillanceMinigameNextCamera
        {
            public static bool Prefix(PlanetSurveillanceMinigame __instance, int direction)
            {
                if (camsBool)
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
                //Toggle ON/OFF depending on minPlayerCams parameter
                if (!__instance.isStatic && camsBool)
                {
                    __instance.isStatic = true;
                    for (int j = 0; j < __instance.ViewPorts.Length; j++)
                    {
                        __instance.ViewPorts[j].sharedMaterial = __instance.StaticMaterial;
                        __instance.SabText[j].gameObject.SetActive(true);
                        __instance.SabText[j].text = "[C A M]\n[D E S A T I V A T E]";
                    }
                }

                return !camsBool;
            }
        }

        public static void udpateBools(int playersLeft)
        {
            vitalsBool = playersLeft > CustomOptionHolder.maxPlayerVitals.getFloat();
            adminBool = playersLeft > CustomOptionHolder.maxPlayerAdmin.getFloat();
            camsBool = playersLeft > CustomOptionHolder.maxPlayerCams.getFloat();
            // System.Console.WriteLine("Method 3");
            // System.Console.WriteLine("PlayerControl Count : " + PlayerControl.AllPlayerControls.Count);
            // System.Console.WriteLine("Players Left : " + playersLeft);
            // System.Console.WriteLine("Admin : " + adminBool + " / " + CustomOptionHolder.maxPlayerAdmin.getFloat() + "\nVitals : " + vitalsBool + " / " + CustomOptionHolder.maxPlayerVitals.getFloat() + "\nCams : " + camsBool + " / " + CustomOptionHolder.maxPlayerCams.getFloat());

        }

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
        public static class ExileControllerBegin
        {
            public static void Postfix(ExileController __instance, GameData.PlayerInfo exiled)
            {
                int playersLeft = PlayerControl.AllPlayerControls.ToArray().Count(pc =>
                    !pc.Data.IsDead && !pc.Data.Disconnected) - (exiled != null ? 1 : 0);
                udpateBools(playersLeft);
                System.Console.WriteLine("Method 2");
                System.Console.WriteLine("PlayerControl Count : " + PlayerControl.AllPlayerControls.Count);
                System.Console.WriteLine("Players Left : " + playersLeft);
                System.Console.WriteLine("Admin : " + adminBool + " / " + CustomOptionHolder.maxPlayerAdmin.getFloat() + "\nVitals : " + vitalsBool + " / " + CustomOptionHolder.maxPlayerVitals.getFloat() + "\nCams : " + camsBool + " / " + CustomOptionHolder.maxPlayerCams.getFloat());
                
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
                udpateBools(playersLeft);
                System.Console.WriteLine("Method 1");
                System.Console.WriteLine("PlayerControl Count : " + PlayerControl.AllPlayerControls.Count);
                System.Console.WriteLine("Players Left : " + playersLeft);
                System.Console.WriteLine("Admin : " + adminBool + " / " + CustomOptionHolder.maxPlayerAdmin.getFloat() + "\nVitals : " + vitalsBool + " / " + CustomOptionHolder.maxPlayerVitals.getFloat() + "\nCams : " + camsBool + " / " + CustomOptionHolder.maxPlayerCams.getFloat());
                

            }
        }
    }
}