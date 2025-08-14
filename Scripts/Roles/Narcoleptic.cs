using PeakArchetypes.Shared;
using PeakArchetypes.Utils;
using System.Collections;
using UnityEngine;
using static CharacterAfflictions;

namespace PeakArchetypes.Scripts.Roles;

public class Narcoleptic : MonoBehaviour
{
	readonly float maxDrowsy = 1f;
	CharacterAfflictions afflictions;
	Character character;
	CharacterData characterData;
	float drowsyIncreasePerSecond;
	float originalDrowsyReductionCooldown;
	float originalDrowsyReductionPerSecond;
	float passOutDuration;

	#region Unity Methods

	void Initialize()
	{
		character = GameHelpers.GetCharacterComponent();
		if (character == null)
		{
			Debug.LogError("[Narcoleptic] Character component not found.");
			enabled = false;
			return;
		}

		afflictions = character.refs.afflictions;
		if (afflictions == null)
		{
			Debug.LogError("[Narcoleptic] CharacterAfflictions not found.");
			enabled = false;
			return;
		}

		characterData = character.data;

		float configPassOutDuration = PConfig.narco_passOutDuration.Value;
		float configTimeToFull = PConfig.narco_timeToFullDrowsy.Value;

		configTimeToFull = (configTimeToFull >= Const.narco_timeToFullDrowsy_Min && configTimeToFull <= Const.narco_timeToFullDrowsy_Max)
			? configTimeToFull
			: Const.narco_timeToFullDrowsy;

		drowsyIncreasePerSecond = maxDrowsy / configTimeToFull;

		Debug.Log($"[Narcoleptic] drowsyIncreasePerSecond set to {drowsyIncreasePerSecond} (time to full: {configTimeToFull}s)");

		passOutDuration = (configPassOutDuration >= Const.narco_passOutDuration_Min && configPassOutDuration <= Const.narco_passOutDuration_Max)
			? configPassOutDuration
			: Const.narco_passOutDuration;

		Debug.Log($"[Narcoleptic] passOutDuration set to {passOutDuration}s");

		SaveAndDisableDrowsyDecay();
	}

	void OnDestroy()
	{
		StopCoroutine(NarcolepticRoutine());
		RestoreDrowsyDecay();
		
		Debug.Log($"[Narcoleptic] Reset complete on destroy.");
	}

	void Start()
	{
		Initialize();
		StartCoroutine(NarcolepticRoutine());
		Debug.Log("[Narcoleptic] Narcoleptic Effects started.");
	}

	#endregion Unity Methods

	#region Role Methods

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

			Debug.Log("[Narcoleptic] Player passed out.");

			yield return new WaitForSeconds(passOutDuration);

			// Attempt to wake up if conditions met
			if (CanWakeUp())
			{
				Debug.Log("[Narcoleptic] Player waking up from narcolepsy.");
				afflictions.SetStatus(STATUSTYPE.Drowsy, 0f);
				yield return new WaitForSeconds(1f);
			}
			else
			{
				Debug.Log("[Narcoleptic] Cannot wake up yet, stamina too low.");
				yield return new WaitForSeconds(1f);
			}
		}
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

	#endregion Role Methods
}

