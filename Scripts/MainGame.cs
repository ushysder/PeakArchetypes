using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Photon.Pun;
using KomiChallenge.Comps;


namespace KomiChallenge.Scripts
{

    [HarmonyPatch]
    internal class MainGame
    {

        [HarmonyPatch(typeof(Campfire), "Awake")]
        [HarmonyPostfix]
        private static void InGame()
        {
            AssignRoles.SplitList(Character.AllCharacters);

            Plugin.SendLog(">>> Assigned ROLLLES");
        }



        /// <summary>
        /// When the map area gets displayed, replace the text with the
        /// Debuff the player has
        /// </summary>
        [HarmonyPatch(typeof(GUIManager), nameof(GUIManager.SetHeroTitle))]
        [HarmonyPrefix]
        private static bool AnnounceRole(ref string text)
        {
            if (AssignRoles.players.ContainsKey(Character.localCharacter))
            {
                Role plrRole = Character.localCharacter.gameObject.GetComponent<Role>();
                if (plrRole == null)
                {
                    plrRole = Character.localCharacter.gameObject.AddComponent<Role>();
                }
                else
                {
                    plrRole.enabled = true;
                }

                plrRole.roleName = AssignRoles.players[Character.localCharacter].roleName;
                plrRole.roleType = AssignRoles.players[Character.localCharacter].roleType;
                plrRole.desc = AssignRoles.players[Character.localCharacter].desc;

                text = plrRole.roleName;
                Plugin.SendLog(">>> Contains player!");
            }
            return true;
        }



        /// <summary>
        /// If user looks at campfire, their debuff will be removed
        /// </summary>
        [HarmonyPatch(typeof(Campfire), nameof(Campfire.HoverEnter))]
        [HarmonyPostfix]
        private static void CampfirePatch()
        {
            Role plrRole = Character.localCharacter.GetComponent<Role>();
            if (plrRole != null)
            {
                plrRole.enabled = false;
            }
            AssignRoles.RemoveDebuffs();

            Plugin.SendLog(">>> Removed Debuffs");
        }



        /// <summary>
        /// Remove all debuffs once player dies
        /// </summary>
        [HarmonyPatch(typeof(Character), nameof(Character.RPCA_Die))]
        [HarmonyPostfix]
        private static void PlayerDiedPatch()
        {
            Role plrRole = Character.localCharacter.GetComponent<Role>();
            if (plrRole != null)
            {
                plrRole.enabled = false;
            }

            AssignRoles.RemoveDebuffs();
            Plugin.SendLog(">>> You died (rip)");
        }



        /// <summary>
        /// Remove all debuffs once the game ends
        /// </summary>
        [HarmonyPatch(typeof(Character), "EndGame")]
        [HarmonyPrefix]
        private static bool EndGamePatch()
        {
            Role plrRole = Character.localCharacter.GetComponent<Role>();
            if (plrRole != null)
            {
                plrRole.enabled = false;
            }

            AssignRoles.RemoveDebuffs();
            AssignRoles.players.Clear();
            Plugin.SendLog(">>> Game has ended");
            return true;
        }

    }
}
