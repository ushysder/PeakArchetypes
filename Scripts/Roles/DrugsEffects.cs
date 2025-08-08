using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace KomiChallenge.Scripts.Roles;

public class DrugsEffects : MonoBehaviour
{
	readonly List<EffectParam> drugsParams = [];

	#region Unity Methods

	void Initialize()
	{
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

				// Enable the effect if it has an 'active' property
				var activeProp = effect.GetType().GetProperty("active", BindingFlags.Public | BindingFlags.Instance);
				activeProp?.SetValue(effect, true);

				var fields = effect.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				foreach (var field in fields)
				{
					if (!typeof(VolumeParameter).IsAssignableFrom(field.FieldType)) continue;

					var volumeParam = field.GetValue(effect);
					if (volumeParam == null) continue;

					var valueProp = volumeParam.GetType().GetProperty("value", BindingFlags.Public | BindingFlags.Instance);
					var overrideProp = volumeParam.GetType().GetProperty("overrideState", BindingFlags.Public | BindingFlags.Instance);
					if (valueProp == null || overrideProp == null) continue;

					object newValue = GetDrugsValue(effectName, field.Name.ToLower(), valueProp.PropertyType);
					if (newValue == null) continue;

					// Cache original value before overriding
					object originalValue = valueProp.GetValue(volumeParam);

					// Set override and value
					overrideProp.SetValue(volumeParam, true);
					valueProp.SetValue(volumeParam, newValue);

					drugsParams.Add(new EffectParam
					{
						volumeParam = volumeParam,
						valueProp = valueProp,
						overrideProp = overrideProp,
						originalValue = originalValue,
						volume = volume,
						effect = effect
					});

					Debug.Log($"[DrugsEffects] Set '{field.Name}' on '{effectName}' to '{newValue}'.");
				}
			}
		}
	}

	void OnDestroy()
	{
		foreach (var param in drugsParams)
		{
			// Reset override and value to original
			param.overrideProp?.SetValue(param.volumeParam, false);

			if (param.originalValue != null && param.valueProp != null)
				param.valueProp.SetValue(param.volumeParam, param.originalValue);
			
			// Reset the volume itself
			if (param.volume != null)
				param.volume.isGlobal = false;
		}

		drugsParams.Clear();

		Debug.Log($"[DrugsEffects] Reset complete on destroy.");
	}

	void Start()
	{
		Initialize();
		Debug.Log("[DrugsEffects] Drugs effects started.");
	}

	#endregion Unity Methods

	#region Role Methods

	object GetDrugsValue(string effectName, string fieldName, Type fieldType)
	{
		string effect = effectName.ToLower();
		string name = fieldName.ToLower();

		if (fieldType == typeof(float))
		{
			if (effect.Contains("blur") && name.Contains("amount")) return 200f;
			if (effect.Contains("motionblur") && name.Contains("intensity")) return 100f;
			if (effect.Contains("motionblur") && name.Contains("clamp")) return 100f;
			if (effect.Contains("doublevision") && name.Contains("intensity")) return 100f;
		}
		else if (fieldType == typeof(bool))
		{
			return true;
		}
		else if (fieldType.IsEnum)
		{
			var values = Enum.GetValues(fieldType);
			return values.GetValue(values.Length - 1); // Most intense
		}

		return null; // fallback
	}

	#endregion Role Methods

	class EffectParam
	{
		public VolumeComponent effect;
		public object originalValue;
		public PropertyInfo overrideProp;
		public PropertyInfo valueProp;
		public Volume volume;
		public object volumeParam;
	}
}
