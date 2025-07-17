using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using Photon.Pun;
using KomiChallenge.Comps;
using System.Collections;
using UnityEngine;


namespace KomiChallenge.Scripts
{

    [HarmonyPatch]
    internal class MainGame
    {
        public static bool enteredAwake = false;


        [HarmonyPatch(typeof(Player), "Awake")]
        [HarmonyPostfix]
        private static void GiveID(Player __instance)
        {
            PhotonView comp = __instance.GetComponent<PhotonView>();
            //Plugin.localID = comp.Owner.ActorNumber;
        }


        [HarmonyPatch(typeof(Campfire), "Awake")]
        [HarmonyPostfix]
        private static void InGame()
        {
            if (enteredAwake) return;
            GUIManager.instance.StartCoroutine(AssginDebuffs());
            enteredAwake = true;
        }



        /// <summary>
        /// When the map area gets displayed, replace the text with the
        /// Debuff the player has
        /// </summary>
        [HarmonyPatch(typeof(GUIManager), nameof(GUIManager.SetHeroTitle))]
        [HarmonyPrefix]
        private static bool AnnounceRole(ref string text)
        {
            int localID = Character.localCharacter.gameObject.GetComponent<PhotonView>().Owner.ActorNumber;
            if (AssignRoles.players.ContainsKey(localID))
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

                plrRole.roleName = AssignRoles.players[localID].roleName;
                plrRole.roleType = AssignRoles.players[localID].roleType;
                plrRole.desc = AssignRoles.players[localID].desc;

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
            Character chr = Character.localCharacter;

            if (!chr.data.dead) return;

            Role plrRole = chr.GetComponent<Role>();
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
            ResetVars("Game has ended");
            return true;
        }



        [HarmonyPatch(typeof(PassportManager), "Awake")]
        [HarmonyPrefix]
        private static bool OnPlayerLeftRoomPatch()
        {
            //ResetVars("Player Spawned in");
            AssignRoles.players.Clear();
            enteredAwake = false;
            return true;
        }



        private static void ResetVars(string msg = "")
        {
            Character ch = Character.localCharacter;
            if (ch == null) return;
            Role plrRole = ch.GetComponent<Role>();
            if (plrRole != null)
            {
                plrRole.enabled = false;
            }

            AssignRoles.RemoveDebuffs();
            AssignRoles.players.Clear();
            enteredAwake = false;
            Plugin.SendLog($">>> {msg}");
        }


        private static IEnumerator AssginDebuffs()
        {
            yield return new WaitForSeconds(8.4f);

            List<int> plrIds = new List<int>();
            foreach (Character plr in Character.AllCharacters)
            {
                if (plr.isBot) continue;
                plrIds.Add(plr.GetComponent<PhotonView>().Owner.ActorNumber);

                Plugin.SendLog($">>> Player: {plr.GetComponent<PhotonView>().Owner.ActorNumber}");
            }
            AssignRoles.SplitList(plrIds);


            Plugin.SendLog(">>> Assigned ROLLLES");
        }

    }
}
