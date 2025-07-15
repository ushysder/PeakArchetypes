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


        [HarmonyPatch(typeof(MountainProgressHandler), "Update")]
        [HarmonyPostfix]
        private static void boopPatch(MountainProgressHandler __instance)
        {

            if (!Plugin.FoundThisMod("zeebloTesting.zeeblo.dev")) return;

        }






    }
}
