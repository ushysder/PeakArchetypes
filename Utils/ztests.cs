using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine.InputSystem;

namespace KomiChallenge.Utils
{
    [HarmonyPatch]
    internal class ztests
    {

        [HarmonyPatch(typeof(MountainProgressHandler), "Update")]
        [HarmonyPostfix]
        private static void boopPatch(MountainProgressHandler __instance)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                GUIManager.instance.SetHeroTitle("boop!", __instance.progressPoints[0].clip);
            }
        }

    }
}
