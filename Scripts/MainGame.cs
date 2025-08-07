using System.Collections.Generic;
using HarmonyLib;
using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace KomiChallenge.Scripts;

[HarmonyPatch]
internal class MainGame
{
    public static bool enteredAwake = false;

	/// <summary>
	/// When the map area gets displayed, replace the text with the
	/// Debuff the player has
	/// </summary>
	[HarmonyPatch(typeof(GUIManager), nameof(GUIManager.SetHeroTitle))]
	[HarmonyPrefix]
	static bool AnnounceRole(ref string text)
	{
		SetRoles();
		int localID = Character.localCharacter.gameObject.GetComponent<PhotonView>().Owner.ActorNumber;
		if (RoleManager.players.ContainsKey(localID))
		{
			Role plrRole = Character.localCharacter.gameObject.GetComponent<Role>();
			text = plrRole.RoleName;
		}
		return true;
	}

	static IEnumerator AssignDebuffs()
	{
		yield return new WaitForSeconds(8.4f);

		List<int> allPlayerIds = [];
		foreach (Character plr in Character.AllCharacters)
		{
			if (plr.isBot) continue;
			allPlayerIds.Add(plr.GetComponent<PhotonView>().Owner.ActorNumber);
		}

		// For players who have already chosen a role, keep it.
		// For players without a role, assign randomly.

		foreach (int playerId in allPlayerIds)
		{
			if (!RoleManager.players.ContainsKey(playerId))
			{
				int randIndex = Random.Range(0, RoleManager.defaultTypes.Count);
				Role randomRole = RoleManager.defaultTypes[randIndex];

				RoleManager.players[playerId] = randomRole;
				Debug.Log($">>> Randomly assigned {randomRole.RoleName} to player {playerId}");
			}
		}

		Debug.Log(">>> Assigned Roles:");

		foreach (var kvp in RoleManager.players)
		{
			Debug.Log($">>> Player {kvp.Key} Role {kvp.Value.RoleName}");
		}
	}

	/// <summary>
	/// Remove all debuffs once the game ends
	/// </summary>
	[HarmonyPatch(typeof(Character), "EndGame")]
	[HarmonyPrefix]
	static bool EndGamePatch()
	{
		ResetVars("Game has ended");
		return true;
	}

	[HarmonyPatch(typeof(Campfire), "Awake")]
	[HarmonyPostfix]
    static void InGame()
    {
        if (enteredAwake) return;

		int localId = PhotonNetwork.LocalPlayer.ActorNumber;
		if (RoleManager.players.ContainsKey(localId)) return;

		GameObject uiGO = new("RoleSelectionUI");
		uiGO.AddComponent<UI.RoleSelectionUI>();

		GUIManager.instance.StartCoroutine(AssignDebuffs());
        enteredAwake = true;
    }

	///// <summary>
	///// If user looks at campfire, their debuff will be removed
	///// </summary>
	//[HarmonyPatch(typeof(Campfire), nameof(Campfire.HoverEnter))]
	//[HarmonyPostfix]
	//static void CampfirePatch()
	//{
	//    RoleManager.RemoveAllDebuffs();
	//    Debug.Log(">>> Removed Debuffs");
	//}

	[HarmonyPatch(typeof(PassportManager), "Awake")]
	[HarmonyPrefix]
	static bool OnPlayerLeftRoomPatch()
	{
		//ResetVars("Player Spawned in");
		RoleManager.players.Clear();
		enteredAwake = false;
		return true;
	}

	[HarmonyPatch(typeof(Character), "Start")]
	[HarmonyPostfix]
	static void HookRevive(Character __instance)
	{
		if (!__instance.IsLocal) return;

		__instance.reviveAction += () =>
		{
			Debug.Log(">>> Local player revived, reapplying debuffs...");
			RoleManager.ReapplyDebuffs();
		};
	}

	/// <summary>
	/// Remove all debuffs once player dies
	/// </summary>
	[HarmonyPatch(typeof(Character), nameof(Character.RPCA_Die))]
    [HarmonyPostfix]
    static void PlayerDiedPatch()
    {
        Character chr = Character.localCharacter;

        if (!chr.data.dead) return;
    
        RoleManager.RemoveAllDebuffs();
        Debug.Log(">>> You died (rip)");
    }

    static void ResetVars(string msg = "")
    {  
        RoleManager.RemoveAllDebuffs();
        RoleManager.players.Clear();
        enteredAwake = false;
        Debug.Log($">>> {msg}");
    }

    static void SetRoles()
    {
		int localID = Character.localCharacter.gameObject.GetComponent<PhotonView>().Owner.ActorNumber;
		if (RoleManager.players.ContainsKey(localID))
		{
			Role plrRole = Character.localCharacter.gameObject.GetComponent<Role>();
			if (plrRole == null)
				plrRole = Character.localCharacter.gameObject.AddComponent<Role>();
			else
				plrRole.enabled = true;

			plrRole.RoleName = RoleManager.players[localID].RoleName;
			plrRole.RoleType = RoleManager.players[localID].RoleType;
			plrRole.Desc = RoleManager.players[localID].Desc;
		}
	}
}
