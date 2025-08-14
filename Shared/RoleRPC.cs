using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PeakArchetypes.Shared;
public class RoleRPC : MonoBehaviourPun
{
	GameObject currentHealingCircle;

	void Awake()
	{
		var pv = GetComponent<PhotonView>();
		Debug.Log($"[RoleRPC] Awake on {gameObject.name}, PhotonView {pv?.ViewID}");
	}

	[PunRPC]
	void MedicHealNearbyPlayersRPC(Vector3 healCenter, float healRadius, float healAmount)
	{
		Debug.Log($"[MedicHealNearbyPlayersRPC] Heal center: {healCenter}, radius: {healRadius}");

		//StartCoroutine(SpawnDebugWireframeCircle(healCenter, Color.green, healRadius, 30f));

		int characterLayer = LayerMask.NameToLayer("Character");
		Collider[] colliders = Physics.OverlapSphere(healCenter, healRadius);

		Debug.Log($"[MedicHealNearbyPlayersRPC] Found {colliders.Length} colliders in radius (all layers).");

		HashSet<string> healedCharacters = [];

		foreach (Collider col in colliders)
		{
			if (col.gameObject.layer != characterLayer)
				continue;

			var targetChar = col.GetComponentInParent<Character>();
			if (targetChar == null)
				continue;

			if (healedCharacters.Contains(targetChar.name))
				continue;

			var targetAfflictions = targetChar.refs.afflictions;
			float currentInjury = targetAfflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Injury);

			Debug.Log($"[MedicHealNearbyPlayersRPC] Found Character {targetChar.name} with current injury {currentInjury}");

			if (currentInjury > 0f)
			{
				float newInjury = Mathf.Max(currentInjury - healAmount, 0f);
				targetAfflictions.SetStatus(CharacterAfflictions.STATUSTYPE.Injury, newInjury);
				Debug.Log($"[MedicEffects] Healed {targetChar.name} by {healAmount}. New injury: {newInjury}");
				healedCharacters.Add(targetChar.name);
			}
		}
	}

	public void CallHealRPC(Vector3 healCenter, float healRadius, float healAmount)
	{
		var pv = GetComponent<PhotonView>();
		if (pv == null)
		{
			Debug.LogError("[RoleRPC] PhotonView missing!");
			return;
		}

		pv.RPC(nameof(MedicHealNearbyPlayersRPC), RpcTarget.All, healCenter, healRadius, healAmount);
	}

	[PunRPC]
	public void ShowHealingCircleRPC(Vector3 position, float radius)
	{
		if (currentHealingCircle != null) Destroy(currentHealingCircle);

		int segments = 60;
		currentHealingCircle = new GameObject("HealingCircle");
		LineRenderer line = currentHealingCircle.AddComponent<LineRenderer>();

		line.positionCount = segments + 1;
		line.loop = true;
		line.widthMultiplier = 0.05f;
		line.material = new Material(Shader.Find("Sprites/Default"));
		line.startColor = Color.green;
		line.endColor = Color.green;

		for (int i = 0; i <= segments; i++)
		{
			float angle = 2 * Mathf.PI * i / segments;
			float x = Mathf.Cos(angle) * radius;
			float z = Mathf.Sin(angle) * radius;
			line.SetPosition(i, position + new Vector3(x, 0, z));
		}
	}

	[PunRPC]
	public void HideHealingCircleRPC()
	{
		if (currentHealingCircle != null)
		{
			Destroy(currentHealingCircle);
			currentHealingCircle = null;
		}
	}

	[PunRPC]
	public void RequestGiveItemRPC(int targetPlayerViewID, string itemName)
	{
		if (!PhotonNetwork.IsMasterClient) return;

		PhotonView targetView = PhotonView.Find(targetPlayerViewID);
		if (targetView == null)
		{
			Debug.LogError("RequestGiveItemRPC: target player view not found");
			return;
		}

		var targetCharacter = targetView.GetComponent<Character>();
		if (targetCharacter == null || targetCharacter.player == null)
		{
			Debug.LogError("RequestGiveItemRPC: target player component not found");
			return;
		}

		// Find item by name
		Item[] allItems = Resources.FindObjectsOfTypeAll<Item>();
		Item item = allItems.FirstOrDefault(i => i.name == itemName);
		if (item == null)
		{
			Debug.LogError($"RequestGiveItemRPC: Item '{itemName}' not found");
			return;
		}

		bool success = targetCharacter.player.AddItem(item.itemID, null, out ItemSlot slot);
		if (!success)
			Debug.LogError($"RequestGiveItemRPC: Failed to add {itemName} to {targetCharacter.characterName}");
		else
			Debug.Log($"RequestGiveItemRPC: Successfully added {itemName} to {targetCharacter.characterName}");
	}
}
