using HarmonyLib;
using Photon.Pun;
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
		RoleManager.ApplyDebuffs();
		int localID = Character.localCharacter.gameObject.GetComponent<PhotonView>().Owner.ActorNumber;
		if (RoleManager.players.ContainsKey(localID))
		{
			Role plrRole = Character.localCharacter.gameObject.GetComponent<Role>();
			text = plrRole.RoleName;
		}
		return true;
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

		enteredAwake = true;
	}

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
			RoleManager.ApplyDebuffs();
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
	
		RoleManager.RemoveDebuffs();
		Debug.Log(">>> You died (rip)");
	}

	static void ResetVars(string msg = "")
	{  
		RoleManager.RemoveDebuffs();
		RoleManager.players.Clear();
		enteredAwake = false;
		Debug.Log($">>> {msg}");
	}
}
