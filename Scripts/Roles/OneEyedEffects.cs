using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using KomiChallenge.Shared;
using static CharacterAfflictions;
using KomiChallenge.Utils;

namespace KomiChallenge.Scripts.Roles
{
	public class OneEyedEffects : MonoBehaviour
	{
		readonly List<EffectParam> oneEyedParams = [];

		CharacterAfflictions afflictions;
		Character character;

		float targetInjury;

		#region Unity Methods

		void Initialize()
		{
			character = GameHelpers.GetCharacterComponent();
			if (character == null)
			{
				Debug.LogError("[OneEyedEffects] Character component not found — disabling.");
				enabled = false;
				return;
			}

			afflictions = character.refs.afflictions;
			if (afflictions == null)
			{
				Debug.LogError("[OneEyedEffects] CharacterAfflictions not found — disabling.");
				enabled = false;
				return;
			}

			float configInjuryPercent = PConfig.oneEyed_targetInjuryPercent.Value;

			// Clamp and fallback check
			if (configInjuryPercent < 10f || configInjuryPercent > 90f)
			{
				Debug.LogWarning($"[OneEyed] Invalid injury percentage {configInjuryPercent} — using default 50%");
				configInjuryPercent = 50f;
			}

			targetInjury = configInjuryPercent / 100f;

			Debug.Log($"[OneEyed] targetInjury set to {targetInjury} ({configInjuryPercent}%)");

			var volumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);
			if (volumes.Length == 0)
			{
				Debug.Log("[OneEyedEffects] No Volume components found in the scene.");
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

					foreach (var field in fields)
					{
						if (!typeof(VolumeParameter).IsAssignableFrom(field.FieldType)) continue;
						if (field.Name != "debugMode") continue;

						var volumeParam = field.GetValue(effect);
						if (volumeParam == null) continue;

						var valueProp = volumeParam.GetType().GetProperty("value", BindingFlags.Public | BindingFlags.Instance);
						var overrideProp = volumeParam.GetType().GetProperty("overrideState", BindingFlags.Public | BindingFlags.Instance);
						if (valueProp == null || overrideProp == null) continue;

						object originalValue = valueProp.GetValue(volumeParam);

						Type enumType = valueProp.PropertyType;
						object splitValue = Enum.Parse(enumType, "SplitWithoutAOAndAOOnly");

						overrideProp.SetValue(volumeParam, true);
						valueProp.SetValue(volumeParam, splitValue);

						oneEyedParams.Add(new EffectParam
						{
							volumeParam = volumeParam,
							valueProp = valueProp,
							overrideProp = overrideProp,
							originalValue = originalValue,
							volume = volume,
							effect = effect
						});

						Debug.Log($"[OneEyedEffects] Set '{field.Name}' on '{effectName}' to '{splitValue}'.");
					}
				}
			}
		}

		void OnDestroy()
		{
			StopCoroutine(OneEyedRoutine());

			foreach (var param in oneEyedParams)
			{
				param.overrideProp?.SetValue(param.volumeParam, false);

				if (param.originalValue != null && param.valueProp != null)
					param.valueProp.SetValue(param.volumeParam, param.originalValue);

				if (param.volume != null)
					param.volume.isGlobal = false;
			}

			oneEyedParams.Clear();

			Debug.Log("[OneEyedEffects] Reset complete on destroy.");
		}

		void Start()
		{
			Initialize();
			StartCoroutine(OneEyedRoutine());
			Debug.Log("[OneEyedEffects] OneEyed Effects started.");
		}

		#endregion Unity Methods

		#region Role Methods

		IEnumerator OneEyedRoutine()
		{
			while (true)
			{
				float currentInjury = afflictions.GetCurrentStatus(STATUSTYPE.Injury);

				if (currentInjury < targetInjury)
				{
					afflictions.AddStatus(STATUSTYPE.Injury, targetInjury - currentInjury);
					Debug.Log($"[OneEyedEffects] Injury level restored to {targetInjury}");
				}

				yield return new WaitForSeconds(1f);
			}
		}

		#endregion Role Methods
	}
}