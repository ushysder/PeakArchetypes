using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Voice.PUN;
using Photon.Voice.Unity;

namespace KomiChallenge.Utils
{
    [HarmonyPatch]
    internal class ztests
    {

        public static bool isMute = false;
        public static bool isDeaf = false;
        public static bool setDeaf = false;
        public static bool setMute = false;
        public static Recorder rec;



        [HarmonyPatch(typeof(MountainProgressHandler), "Update")]
        [HarmonyPostfix]
        private static void boopPatch(MountainProgressHandler __instance)
        {

            if (!Plugin.FoundThisMod("zeebloTesting.zeeblo.dev")) return;

            if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                BlindUser();

                GUIManager.instance.SetHeroTitle("Blinded", __instance.progressPoints[0].clip);
            }

            if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                DeafUser();

                GUIManager.instance.SetHeroTitle("Deafened", __instance.progressPoints[0].clip);
            }



            if (setDeaf)
            {
                AudioListener.volume = 0;
            }


        }


        [HarmonyPatch(typeof(Player), "Awake")]
        [HarmonyPostfix]
        private static void GiveID(Player __instance)
        {
            PhotonView comp = __instance.GetComponent<PhotonView>();
            Plugin.localID = comp.Owner.ActorNumber;
        }



        [HarmonyPatch(typeof(Character), "Update")]
        [HarmonyPostfix]
        private static void CharUpPatch(Character __instance)
        {
            if (!Plugin.FoundThisMod("zeebloTesting.zeeblo.dev")) return;

            if (Keyboard.current.digit4Key.wasPressedThisFrame)
            {
                isMute = true;
                Plugin.SendLog(">>> Muted This Player");
            }


            if (isMute)
            {
                //AudioLevels.SetPlayerLevel(Plugin.localID, 0);
                //AudioLevels.SetPlayerLevel(1, 0);
                rec = Character.localCharacter.GetComponent<PhotonVoiceView>().RecorderInUse;
                rec.TransmitEnabled = false;

            }
        }

        /*
        [HarmonyPatch(typeof(Character), "Start")]
        [HarmonyPostfix]
        private static void StartPatch(Character __instance)
        {
            rec = __instance.GetComponent<PhotonVoiceView>().RecorderInUse;

            Plugin.SendLog(">>> Muted This Player");
        }
        */

        /*
        [HarmonyPatch(typeof(CharacterVoiceHandler), "LateUpdate")]
        [HarmonyPrefix]
        private static bool MuteMe3(CharacterVoiceHandler __instance)
        {
            __instance.
        }
        */


        // MicrophoneSetting
        // SettingHandler
        // CharacterVoiceHandler 
        // AudioLevels
        // VoiceDemoUI


        private static void BlindUser()
        {
            // Disable camera component
            MainCamera.instance.transform.GetComponent<Camera>().enabled = !MainCamera.instance.transform.GetComponent<Camera>().enabled;

        }
        private static void DeafUser()
        {
            if (isDeaf)
            {
                AudioListener.volume = 1;
            }
            else
            {
                setDeaf = true;
                AudioListener.volume = 0;
            }
            
        }

        private static bool MuteUser()
        {
            return isMute;
        }

    }
}
