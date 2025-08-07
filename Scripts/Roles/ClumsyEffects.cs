using System.Reflection;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using KomiChallenge.Utils;

namespace KomiChallenge.Scripts.Roles;

internal class ClumsyEffects : MonoBehaviour
{
	object characterMovement;

	// === Clumsy Movement ===
	float invertTimer = 0f;
	bool invertX = false;
	bool invertY = false;

	FieldInfo invertXField;
	FieldInfo invertYField;
	Type movementType;
	PropertyInfo valuePropX;
	PropertyInfo valuePropY;

	float validatedMinTime;
	float validatedMaxTime;

	void OnEnable()
	{
		Initialize();
		ApplyInversion(invertX, invertY); // Re-apply current inversion
		Debug.Log("[ClumsyEffects] Clumsy input effect enabled.");
	}

	void Initialize()
	{
		if (characterMovement != null) return; // Already initialized

		characterMovement = GameHelpers.GetMovementComponent();
		if (characterMovement == null)
		{
			Debug.LogError("[ClumsyEffects] CharacterMovement not found.");
			enabled = false;
			return;
		}

		movementType = characterMovement.GetType();

		invertXField = movementType.GetField("invertXSetting", BindingFlags.Instance | BindingFlags.NonPublic);
		invertYField = movementType.GetField("invertYSetting", BindingFlags.Instance | BindingFlags.NonPublic);

		if (invertXField == null || invertYField == null)
		{
			Debug.LogError("[ClumsyEffects] Could not find invert fields.");
			enabled = false;
			return;
		}

		var invertXObj = invertXField.GetValue(characterMovement);
		var invertYObj = invertYField.GetValue(characterMovement);

		if (invertXObj == null || invertYObj == null)
		{
			Debug.LogError("[ClumsyEffects] Invert field objects are null.");
			enabled = false;
			return;
		}

		valuePropX = invertXObj.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		valuePropY = invertYObj.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

		if (valuePropX == null || valuePropY == null)
		{
			Debug.LogError("[ClumsyEffects] Value properties not found.");
			enabled = false;
			return;
		}

		validatedMinTime = Mathf.Clamp(PConfig.clumsy_InvertMinTime.Value, 1f, 60f);
		validatedMaxTime = Mathf.Clamp(PConfig.clumsy_InvertMaxTime.Value, validatedMinTime, 120f);

		Debug.Log($"[ClumsyEffects] Initialized with invertTimer range ({validatedMinTime}, {validatedMaxTime})");
	}

	void ApplyInversion(bool invertX, bool invertY)
	{
		if (characterMovement == null || invertXField == null || invertYField == null || valuePropX == null || valuePropY == null)
			return;

		var invertXObj = invertXField.GetValue(characterMovement);
		var invertYObj = invertYField.GetValue(characterMovement);

		if (invertXObj == null || invertYObj == null) return;

		object enumValueX = Enum.Parse(valuePropX.PropertyType, invertX ? "ON" : "OFF");
		object enumValueY = Enum.Parse(valuePropY.PropertyType, invertY ? "ON" : "OFF");

		valuePropX.SetValue(invertXObj, enumValueX);
		valuePropY.SetValue(invertYObj, enumValueY);

		Debug.Log($"[ClumsyEffects] Set invertX to {enumValueX}, invertY to {enumValueY}");
	}

	void Reset(string msg)
	{
		if (characterMovement == null || invertXField == null || invertYField == null || valuePropX == null || valuePropY == null)
		{
			Debug.LogWarning("[ClumsyEffects] Skipping inversion reset — not initialized properly.");
			return;
		}

		var invertXObj = invertXField.GetValue(characterMovement);
		var invertYObj = invertYField.GetValue(characterMovement);

		if (invertXObj == null || invertYObj == null) return;

		valuePropX.SetValue(invertXObj, Enum.Parse(valuePropX.PropertyType, "OFF"));
		valuePropY.SetValue(invertYObj, Enum.Parse(valuePropY.PropertyType, "OFF"));

		Debug.Log($"[ClumsyEffects] Inversion reset on {msg}.");
	}

	void OnDisable() => Reset("Disable");
	void OnDestroy() => Reset("Destroy");

	void Update()
	{
		invertTimer -= Time.deltaTime;
		if (invertTimer <= 0f)
		{
			invertX = Random.value > 0.5f;
			invertY = Random.value > 0.5f;

			ApplyInversion(invertX, invertY);
			invertTimer = Random.Range(validatedMinTime, validatedMaxTime);
		}
	}

	float GetValidatedInvertMinTime()
	{
		float val = PConfig.clumsy_InvertMinTime.Value;
		if (val < 1f) return 1f;
		if (val > 60f) return 60f;
		return val;
	}

	float GetValidatedInvertMaxTime(float minTime)
	{
		float val = PConfig.clumsy_InvertMaxTime.Value;
		if (val < minTime) return minTime;
		if (val > 120f) return 120f;
		return val;
	}
}


