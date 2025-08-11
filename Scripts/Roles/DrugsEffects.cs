using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Reflection;
using System.Collections.Generic;
using Photon.Pun;
using static CharacterAfflictions;
using System.Collections;
using Random = UnityEngine.Random;
using KomiChallenge.Utils;
using KomiChallenge.Shared;

namespace KomiChallenge.Scripts.Roles;

public class DrugsEffects : MonoBehaviour
{
	readonly List<EffectParam> drugsParams = [];

	readonly float maxPoison = 1f;

	CharacterAfflictions afflictions;
	Character character;

	float originalPoisonReductionCooldown;
	float originalPoisonReductionPerSecond;

	float poisonIncreasePerSecond;

	#region Unity Methods

	void Initialize()
	{
		character = GameHelpers.GetCharacterComponent();
		if (character == null)
		{
			Debug.LogError("[DrugsEffects] Character component not found — disabling.");
			enabled = false;
			return;
		}

		afflictions = character.refs.afflictions;
		if (afflictions == null)
		{
			Debug.LogError("[DrugsEffects] CharacterAfflictions not found — disabling.");
			enabled = false;
			return;
		}

		float configTimeToFull = PConfig.drugs_timeToFullPoison.Value;

		configTimeToFull = (configTimeToFull >= Const.drugs_timeToFullPoison_Min && configTimeToFull <= Const.drugs_timeToFullPoison_Max)
			? configTimeToFull
			: Const.drugs_timeToFullPoison;

		poisonIncreasePerSecond = maxPoison / configTimeToFull;

		Debug.Log($"[DrugsEffects] poisonIncreasePerSecond set to {poisonIncreasePerSecond} (time to full: {configTimeToFull}s)");

		SaveAndDisablePoisonDecay();

		var volumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);
		if (volumes.Length == 0)
		{
			Debug.Log("[DrugsEffects] No Volume components found in the scene.");
			return;
		}

		foreach (var volume in volumes)
		{
			if (volume.profile == null) continue;
			volume.isGlobal = true;

			foreach (var effect in volume.profile.components)
			{
				if (effect == null) continue;

				string effectName = effect.GetType().Name.ToLower();
				if (effectName != "hbao") continue;

				var activeProp = effect.GetType().GetProperty("active", BindingFlags.Public | BindingFlags.Instance);
				activeProp?.SetValue(effect, true);

				var fields = effect.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

				var fieldsToEdit = new HashSet<string>
				{
					"debugMode"
				};

				foreach (var field in fields)
				{
					if (!typeof(VolumeParameter).IsAssignableFrom(field.FieldType)) continue;

					if (!fieldsToEdit.Contains(field.Name))
						continue;

					var volumeParam = field.GetValue(effect);
					if (volumeParam == null) continue;

					var valueProp = volumeParam.GetType().GetProperty("value", BindingFlags.Public | BindingFlags.Instance);
					var overrideProp = volumeParam.GetType().GetProperty("overrideState", BindingFlags.Public | BindingFlags.Instance);
					if (valueProp == null || overrideProp == null) continue;

					// Cache original value before overriding
					object originalValue = valueProp.GetValue(volumeParam);

					Type enumType = valueProp.PropertyType;
					object viewNormalsValue = Enum.Parse(enumType, "ViewNormals");

					// Set override and value
					overrideProp.SetValue(volumeParam, true);
					valueProp.SetValue(volumeParam, viewNormalsValue);
					
					drugsParams.Add(new EffectParam
					{
						volumeParam = volumeParam,
						valueProp = valueProp,
						overrideProp = overrideProp,
						originalValue = originalValue,
						volume = volume,
						effect = effect
					});

					Debug.Log($"[DrugsEffects] Set '{field.Name}' on '{effectName}' to '{viewNormalsValue}'.");
				}
			}
		}
	}

	void OnDestroy()
	{
		foreach (var param in drugsParams)
		{
			param.overrideProp?.SetValue(param.volumeParam, false);

			if (param.originalValue != null && param.valueProp != null)
				param.valueProp.SetValue(param.volumeParam, param.originalValue);

			if (param.volume != null)
				param.volume.isGlobal = false;
		}

		drugsParams.Clear();

		Debug.Log("[DrugsEffects] Reset complete on destroy.");
	}

	void Start()
	{
		Initialize();
		StartCoroutine(DrugsRoutine());
		Debug.Log("[DrugsEffects] Drugs effects started.");
	}

	#endregion Unity Methods

	#region Role Methods

	IEnumerator DrugsRoutine()
	{
		var view = character.refs.view;

		while (true)
		{
			float currentPoison = afflictions.GetCurrentStatus(STATUSTYPE.Poison);

			if (currentPoison < maxPoison)
				afflictions.AddStatus(STATUSTYPE.Poison, poisonIncreasePerSecond * Time.deltaTime);
			else
			{
				if (view != null && view.IsMine)
				{
					Transform hip = character.refs.hip.transform;
					Vector3 forward = hip.forward;

					if (Vector3.Dot(forward, Vector3.up) < 0f)
						forward = -forward;

					Vector3 dropPos = hip.position + forward * 0.6f;
					dropPos += Vector3.up * Random.Range(0f, 0.5f);

					view.RPC(nameof(Character.RPCA_Die), PhotonNetwork.LocalPlayer, dropPos);

					Debug.Log("[DrugsEffects] Player overdosed and died.");
				}
				yield break; // Stop coroutine after death
			}

			yield return null;
		}
	}

	void RestorePoisonDecay()
	{
		afflictions.poisonReductionPerSecond = originalPoisonReductionPerSecond;
		afflictions.poisonReductionCooldown = originalPoisonReductionCooldown;
		Debug.Log("[DrugsEffects] Poison decay restored to original values.");
	}

	void SaveAndDisablePoisonDecay()
	{
		originalPoisonReductionPerSecond = afflictions.poisonReductionPerSecond;
		originalPoisonReductionCooldown = afflictions.poisonReductionCooldown;

		afflictions.poisonReductionPerSecond = 0f;
		afflictions.poisonReductionCooldown = 999999f;
		Debug.Log("[DrugsEffects] Poison decay disabled for overdose effect.");
	}

	#endregion Role Methods
}
