using UnityEngine;
using System.Reflection;
using System;
using System.Collections;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Photon.Pun;

namespace KomiChallenge.Scripts.Roles;

public class DrunkController : MonoBehaviour
{
	[Range(0f, 1f)]
	public float drunkennessLevel = 0f;
	public float maxFallInterval = 45f;
	public float minFallInterval = 10f;
	public float passOutDuration = 5f;
	public float timeToMaxDrunkness = 300f;

	readonly List<VignetteParam> vignetteParams = [];
	Character character;
	float drunkTimer = 0f;
	bool isDrunk = false;
	bool isInitialized = false;
	bool passedOut = false;

	VolumeComponent vignetteEffect;
	Volume vignetteVolume;

	IEnumerator FallWithScreenShakeRepeater()
	{
		Debug.Log("[DrunkController] Coroutine started.");

		var photonView = character.photonView;

		while (isDrunk)
		{
			float drunkProgress = Mathf.Clamp01(drunkTimer / timeToMaxDrunkness);
			float currentInterval = Mathf.Lerp(maxFallInterval, minFallInterval, drunkProgress);
			yield return new WaitForSeconds(currentInterval);

			// Owner sends fall RPC
			if (!passedOut && character != null && photonView != null && photonView.IsMine)
			{
				Debug.Log($"[DrunkController] Falling... drunkenness level: {drunkProgress:F2}");
				photonView.RPC(nameof(Character.RPCA_FallWithScreenShake), RpcTarget.All, 0.5f, 5f);
			}

			if (!passedOut && drunkProgress >= 1f)
			{
				Debug.Log("[DrunkController] Max drunkness reached. Passing out...");
				passedOut = true;
				isDrunk = false;

				if (photonView != null && photonView.IsMine)
					photonView.RPC(nameof(Character.RPCA_PassOut), RpcTarget.All);
				
				yield return new WaitForSeconds(passOutDuration);

				Debug.Log("[DrunkController] Recovering from pass out.");

				if (photonView != null && photonView.IsMine)
				{
					// Optional: use Photon RPC if UnPassOut also needs to sync
					character.photonView.RPC("RPCA_UnPassOut", RpcTarget.All);
				}

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

	void Initialize()
	{
		if (isInitialized) return;

		float configMaxFall = Utils.PConfig.drunk_maxFallInterval.Value;
		float configMinFall = Utils.PConfig.drunk_minFallInterval.Value;
		float configPassOut = Utils.PConfig.drunk_passOutDuration.Value;
		float configTimeToMax = Utils.PConfig.drunk_timeToMaxDrunkness.Value;

		maxFallInterval = (configMaxFall >= 1 && configMaxFall <= 600) 
			? configMaxFall 
			: 45f;

		minFallInterval = (configMinFall >= 1 && configMinFall <= maxFallInterval) 
			? configMinFall 
			: 10f;

		passOutDuration = (configPassOut >= 1f && configPassOut <= 30f) 
			? configPassOut 
			: 5f;

		timeToMaxDrunkness = (configTimeToMax >= 30f && configTimeToMax <= 600f) 
			? configTimeToMax 
			: 300f;
		
		Debug.Log($"[DrunkController] Validated config: maxFall={maxFallInterval}, minFall={minFallInterval}, passOut={passOutDuration}, timeToMax={timeToMaxDrunkness}");
		
		FindVignetteEffect();

		character = GetComponent<Character>();
		if (character == null)
		{
			Debug.LogWarning("[DrunkController] No Character component found.");
			enabled = false;
			return;
		}

		isDrunk = true;
		StartCoroutine(FallWithScreenShakeRepeater());

		isInitialized = true;
		Debug.Log("[DrunkController] Initialized and ready to go.");
	}

	void OnDestroy() => Reset("Destroy");

	void OnDisable() => Reset("Disable");

	void OnEnable()
	{
		if (!isInitialized)
			Initialize();

		if (character != null && !isDrunk)
		{
			isDrunk = true;
			StartCoroutine(FallWithScreenShakeRepeater());
		}

		Debug.Log("[DrunkController] Drunk Effects enabled.");
	}

	void Reset(string msg)
	{
		isDrunk = false;
		passedOut = false;
		drunkTimer = 0f;
		drunkennessLevel = 0f;

		StopCoroutine(FallWithScreenShakeRepeater());

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

		Debug.Log($"[DrunkController] Reset complete on {msg}.");
	}

	void Update()
	{
		if (isDrunk && !passedOut)
		{
			drunkTimer += Time.deltaTime;
			drunkennessLevel = Mathf.Clamp01(drunkTimer / timeToMaxDrunkness);
			UpdateVignette(drunkennessLevel);
		}
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

	struct VignetteParam
	{
		public string fieldName;
		public PropertyInfo overrideProp;
		public object param;
		public PropertyInfo valueProp;
	}
}

