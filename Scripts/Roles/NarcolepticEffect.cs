using KomiChallenge.Utils;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using static CharacterAfflictions;

namespace KomiChallenge.Scripts.Roles;

public class NarcolepticEffect : MonoBehaviour
{
	readonly float maxDrowsy = 1f;
	CharacterAfflictions afflictions;
	Character character;
	CharacterData characterData;
	float drowsyIncreasePerSecond = 0.01f;
	float originalDrowsyReductionCooldown;
	float originalDrowsyReductionPerSecond;
	float passOutDuration = 5f;

	bool CanWakeUp()
	{
		float totalStatus = afflictions.statusSum;
		float drowsy = afflictions.GetCurrentStatus(STATUSTYPE.Drowsy);

		float staminaIfNoDrowsy = 1f - (totalStatus - drowsy);

		// Only wake up if stamina would be between 0.1 and 1 after removing drowsy
		return staminaIfNoDrowsy >= 0.1f;
	}

	IEnumerator NarcolepticRoutine()
	{
		var photonView = character.photonView;

		while (true)
		{
			// Increase drowsiness gradually until max or player passes out
			while (!characterData.passedOut)
			{
				float currentDrowsy = afflictions.GetCurrentStatus(STATUSTYPE.Drowsy);

				if (currentDrowsy < maxDrowsy)
					afflictions.AddStatus(STATUSTYPE.Drowsy, drowsyIncreasePerSecond * Time.deltaTime);

				yield return null;
			}

			Debug.Log("[NarcolepticEffect] Player passed out.");

			yield return new WaitForSeconds(passOutDuration);

			// Attempt to wake up if conditions met
			if (CanWakeUp())
			{
				Debug.Log("[NarcolepticEffect] Player waking up from narcolepsy.");
				afflictions.SetStatus(STATUSTYPE.Drowsy, 0f);

				if (photonView != null && photonView.IsMine)
					photonView.RPC("RPCA_UnPassOut", RpcTarget.All);

				yield return new WaitForSeconds(1f);
			}
			else
			{
				Debug.Log("[NarcolepticEffect] Cannot wake up yet, stamina too low.");
				yield return new WaitForSeconds(1f);
			}
		}
	}

	void OnDestroy() => Reset("Destroy");

	void OnDisable() => Reset("Disable");

	void OnEnable()
	{
		character = GameHelpers.GetCharacterComponent();
		if (character == null)
		{
			Debug.LogError("[NarcolepticEffect] Character component not found.");
			enabled = false;
			return;
		}

		afflictions = character.refs.afflictions;
		if (afflictions == null)
		{
			Debug.LogError("[NarcolepticEffect] CharacterAfflictions not found.");
			enabled = false;
			return;
		}

		characterData = character.data;

		float configPassOutDuration = PConfig.narco_passOutDuration.Value;
		float configTimeToFull = PConfig.narco_timeToFullDrowsy.Value;

		configTimeToFull = (configTimeToFull >= 30f && configTimeToFull <= 600f)
			? configTimeToFull
			: 300f;

		drowsyIncreasePerSecond = maxDrowsy / configTimeToFull;

		Debug.Log($"[NarcolepticEffect] drowsyIncreasePerSecond set to {drowsyIncreasePerSecond} (time to full: {configTimeToFull}s)");

		passOutDuration = (configPassOutDuration >= 1f && configPassOutDuration <= 30f)
			? configPassOutDuration
			: 5f;

		Debug.Log($"[NarcolepticEffect] passOutDuration set to {passOutDuration}s");

		SaveAndDisableDrowsyDecay();

		StartCoroutine(NarcolepticRoutine());
	}

	void Reset(string msg)
	{
		RestoreDrowsyDecay();
		StopCoroutine(NarcolepticRoutine());

		Debug.Log($"[NarcolepticEffect] Reset complete on {msg}.");
	}

	void RestoreDrowsyDecay()
	{
		afflictions.drowsyReductionPerSecond = originalDrowsyReductionPerSecond;
		afflictions.drowsyReductionCooldown = originalDrowsyReductionCooldown;
	}

	void SaveAndDisableDrowsyDecay()
	{
		originalDrowsyReductionPerSecond = afflictions.drowsyReductionPerSecond;
		originalDrowsyReductionCooldown = afflictions.drowsyReductionCooldown;

		afflictions.drowsyReductionPerSecond = 0f;
		afflictions.drowsyReductionCooldown = 999999f; // Very large cooldown to prevent decay
	}
}

