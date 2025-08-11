using UnityEngine;
using System.Reflection;
using System;
using System.Collections;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Photon.Pun;
using KomiChallenge.Shared;

namespace KomiChallenge.Scripts.Roles;

public class DrunkEffects : MonoBehaviour
{
	readonly List<VignetteParam> vignetteParams = [];

	Character character;

	[Range(0f, 1f)]
	float drunkennessLevel;
	float drunkTimer = 0f;
	bool isDrunk = false;
	float maxFallInterval;
	float minFallInterval;
	bool passedOut = false;
	float passOutDuration;
	float timeToMaxDrunkness;
	VolumeComponent vignetteEffect;
	Volume vignetteVolume;

	#region Unity Methods

	void Initialize()
	{
		float configMaxFall = Utils.PConfig.drunk_maxFallInterval.Value;
		float configMinFall = Utils.PConfig.drunk_minFallInterval.Value;
		float configPassOut = Utils.PConfig.drunk_passOutDuration.Value;
		float configTimeToMax = Utils.PConfig.drunk_timeToMaxDrunkness.Value;

		maxFallInterval = (configMaxFall >= Const.drunk_maxFallInterval_Min && configMaxFall <= Const.drunk_maxFallInterval_Max)
			? configMaxFall
			: Const.drunk_maxFallInterval;

		minFallInterval = (configMinFall >= Const.drunk_minFallInterval_Min && configMinFall <= maxFallInterval)
			? configMinFall
			: Const.drunk_minFallInterval;

		passOutDuration = (configPassOut >= Const.drunk_passOutDuration_Min && configPassOut <= Const.drunk_passOutDuration_Max)
			? configPassOut
			: Const.narco_passOutDuration;

		timeToMaxDrunkness = (configTimeToMax >= Const.drunk_timeToMaxDrunkness_Min && configTimeToMax <= Const.drunk_timeToMaxDrunkness_Max)
			? configTimeToMax
			: Const.drunk_timeToMaxDrunkness;

		Debug.Log($"[DrunkController] Validated config: maxFall={maxFallInterval}, minFall={minFallInterval}, passOut={passOutDuration}, timeToMax={timeToMaxDrunkness}");

		FindVignetteEffect();

		character = GameHelpers.GetCharacterComponent();
		if (character == null)
		{
			Debug.LogWarning("[DrunkController] No Character component found.");
			enabled = false;
			return;
		}

		isDrunk = true;

		Debug.Log("[DrunkController] Initialized and ready to go.");
	}

	void OnDestroy()
	{
		isDrunk = false;
		passedOut = false;
		drunkTimer = 0f;
		drunkennessLevel = 0f;

		StopCoroutine(DrunkRoutine());

		foreach (var param in vignetteParams)
		{
			if (param.valueProp != null)
			{
				if (param.valueProp.PropertyType == typeof(float))
					param.valueProp.SetValue(param.param, 0f);
				else if (param.valueProp.PropertyType == typeof(bool))
					param.valueProp.SetValue(param.param, false);
			}

			param.overrideProp?.SetValue(param.param, false);
		}

		if (vignetteVolume != null)
			vignetteVolume.isGlobal = false;

		Debug.Log($"[DrunkController] Reset complete on destroy.");
	}

	void Start()
	{
		Initialize();
		StartCoroutine(DrunkRoutine());
		Debug.Log("[DrunkController] Drunk Effects started.");
	}

	#endregion

	#region Role Methods

	IEnumerator DrunkRoutine()
	{
		Debug.Log("[DrunkController] Coroutine started.");

		var view = character.refs.view;

		while (isDrunk)
		{
			float drunkProgress = Mathf.Clamp01(drunkTimer / timeToMaxDrunkness);
			float currentInterval = Mathf.Lerp(maxFallInterval, minFallInterval, drunkProgress);
			float elapsed = 0f;

			while (elapsed < currentInterval && isDrunk && !passedOut)
			{
				// Per-frame drunkness progression
				drunkTimer += Time.deltaTime;
				drunkProgress = Mathf.Clamp01(drunkTimer / timeToMaxDrunkness);
				drunkennessLevel = drunkProgress;
				UpdateVignette(drunkennessLevel);

				elapsed += Time.deltaTime;
				yield return null; // wait for next frame
			}

			if (!passedOut && character != null && view != null && view.IsMine)
			{
				Debug.Log($"[DrunkController] Falling... drunkenness level: {drunkProgress:F2}");
				view.RPC(nameof(Character.RPCA_FallWithScreenShake), PhotonNetwork.LocalPlayer, 0.5f, 5f);
			}

			if (!passedOut && drunkProgress >= 1f)
			{
				Debug.Log("[DrunkController] Max drunkness reached. Passing out...");
				passedOut = true;
				isDrunk = false;

				if (view != null && view.IsMine)
					view.RPC(nameof(Character.RPCA_PassOut), PhotonNetwork.LocalPlayer);

				yield return new WaitForSeconds(passOutDuration);

				Debug.Log("[DrunkController] Recovering from pass out.");

				if (view != null && view.IsMine)
					view.RPC("RPCA_UnPassOut", PhotonNetwork.LocalPlayer);

				drunkTimer = 0f;
				isDrunk = true;
				passedOut = false;
			}
		}

		Debug.Log("[DrunkController] Coroutine exited.");
	}

	void FindVignetteEffect()
	{
		var volumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);
		foreach (var volume in volumes)
		{
			if (volume.profile == null) continue;

			foreach (var effect in volume.profile.components)
			{
				if (effect == null) continue;

				if (effect.GetType().Name.ToLower().Contains("vignette"))
				{
					vignetteVolume = volume;
					vignetteEffect = effect;
					vignetteVolume.isGlobal = true;

					var fields = vignetteEffect.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					foreach (var field in fields)
					{
						if (!typeof(VolumeParameter).IsAssignableFrom(field.FieldType)) continue;

						var volumeParam = field.GetValue(vignetteEffect);
						if (volumeParam == null) continue;

						var valueProp = volumeParam.GetType().GetProperty("value", BindingFlags.Public | BindingFlags.Instance);
						var overrideProp = volumeParam.GetType().GetProperty("overrideState", BindingFlags.Public | BindingFlags.Instance);

						if (valueProp != null && overrideProp != null)
						{
							vignetteParams.Add(new VignetteParam
							{
								param = volumeParam,
								valueProp = valueProp,
								overrideProp = overrideProp,
								fieldName = field.Name.ToLower()
							});
						}
					}

					Debug.Log("[DrunkController] Found and cached vignette effect.");
					return;
				}
			}
		}

		Debug.LogWarning("[DrunkController] No vignette effect found.");
	}

	void UpdateVignette(float level)
	{
		if (vignetteEffect == null || vignetteParams.Count == 0) return;

		level = Mathf.Clamp01(level);

		foreach (var param in vignetteParams)
		{
			param.overrideProp.SetValue(param.param, true);

			if (param.valueProp.PropertyType == typeof(float) && param.fieldName.Contains("intensity"))
			{
				float intensity = Mathf.Lerp(0.2f, 1f, level);
				param.valueProp.SetValue(param.param, intensity);
			}
			else if (param.valueProp.PropertyType == typeof(bool))
			{
				param.valueProp.SetValue(param.param, true);
			}
		}
	}

	#endregion

	struct VignetteParam
	{
		public string fieldName;
		public PropertyInfo overrideProp;
		public object param;
		public PropertyInfo valueProp;
	}
}

