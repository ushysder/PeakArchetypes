using System.Collections;
using System.Reflection;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using KomiChallenge.Utils;
using Photon.Pun;
using System.Collections.Generic;
using Zorro.Core;

namespace KomiChallenge.Scripts.Roles
{
	internal class ClumsyEffects : MonoBehaviour
	{
		Character character;
		CharacterMovement characterMovement;
		FieldInfo invertXField;
		FieldInfo invertYField;
		float validatedMaxTime;
		int validatedDropChance;
		float validatedMinTime;
		PropertyInfo valuePropX;
		PropertyInfo valuePropY;

		#region Unity Methods

		void Initialize()
		{
			if (Character.localCharacter == null)
			{
				Debug.LogWarning("[ClumsyEffects] Character.localCharacter is null — skipping initialization.");
				enabled = false;
				return;
			}

			character = GameHelpers.GetCharacterComponent();
			characterMovement = character.refs.movement;

			if (characterMovement == null || character == null)
			{
				Debug.LogError("[ClumsyEffects] Missing required components.");
				enabled = false;
				return;
			}

			var movementType = characterMovement.GetType();
			invertXField = movementType.GetField("invertXSetting", BindingFlags.Instance | BindingFlags.NonPublic);
			invertYField = movementType.GetField("invertYSetting", BindingFlags.Instance | BindingFlags.NonPublic);

			if (invertXField == null || invertYField == null)
			{
				Debug.LogError("[ClumsyEffects] Invert fields not found.");
				enabled = false;
				return;
			}

			var invertXObj = invertXField.GetValue(characterMovement);
			var invertYObj = invertYField.GetValue(characterMovement);

			if (invertXObj == null || invertYObj == null)
			{
				Debug.LogError("[ClumsyEffects] Invert field values are null.");
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

			validatedMinTime = (PConfig.clumsy_InvertMinTime.Value >= 1f && PConfig.clumsy_InvertMinTime.Value <= 60f)
				? PConfig.clumsy_InvertMinTime.Value
				: 10f;

			validatedMaxTime = (PConfig.clumsy_InvertMaxTime.Value >= validatedMinTime && PConfig.clumsy_InvertMaxTime.Value <= 120f)
				? PConfig.clumsy_InvertMaxTime.Value
				: Mathf.Max(validatedMinTime, 30f);

			validatedDropChance = (PConfig.clumsy_ItemDropChancePercent.Value >= 0 && PConfig.clumsy_ItemDropChancePercent.Value <= 100)
				? PConfig.clumsy_ItemDropChancePercent.Value
				: 50;

			Debug.Log($"[ClumsyEffects] Configured time range: {validatedMinTime} to {validatedMaxTime} seconds");
		}

		void OnDestroy()
		{
			ApplyInversion(false, false);
			StopCoroutine(ClumsyRoutine());
			Debug.Log($"[ClumsyEffects] Reset ClumsyEffects on destroy");
		}

		void Start()
		{
			Initialize();
			StartCoroutine(ClumsyRoutine());
			Debug.Log("[ClumsyEffects] Clumsy effect coroutine started.");
		}

		#endregion Unity Methods

		#region Role Methods

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

			Debug.Log($"[ClumsyEffects] Applied inversion - X: {enumValueX}, Y: {enumValueY}");
		}

		IEnumerator ClumsyRoutine()
		{
			while (true)
			{
				float waitTime = Random.Range(validatedMinTime, validatedMaxTime);
				yield return new WaitForSeconds(waitTime);

				bool invertX = Random.value > 0.5f;
				bool invertY = Random.value > 0.5f;
				ApplyInversion(invertX, invertY);

				if (Random.Range(0, 100) < validatedDropChance)
					DropRandomItem();
			}
		}

		void DropRandomItem()
		{
			if (!character.refs.view.IsMine) return;

			var nonEmptySlots = new List<byte>();
			ItemSlot[] array = character.player.itemSlots;
			for (byte i = 0; i < array.Length; i++)
			{
				if (!array[i].IsEmpty())
					nonEmptySlots.Add(i);
			}

			if (nonEmptySlots.Count == 0) return;

			// Pick random slot
			byte randomSlot = nonEmptySlots[Random.Range(0, nonEmptySlots.Count)];

			// Get hip transform
			Transform hip = character.refs.hip.transform;
			Vector3 forward = hip.forward;

			if (Vector3.Dot(forward, Vector3.up) < 0f)
				forward = -forward;

			Vector3 dropPos = hip.position + forward * 0.6f;
			dropPos += Vector3.up * Random.Range(0f, 0.5f); // Slight vertical variation

			if (character.refs.view.IsMine)
			{
				Debug.Log($"[ClumsyEffects] Attempting to drop random item at slot {randomSlot} | CurrentSelectedSlot : {character.refs.items.currentSelectedSlot.Value}.");
				if (character.refs.items.currentSelectedSlot.IsSome && character.refs.items.currentSelectedSlot.Value == randomSlot)
				{
					character.refs.items.EquipSlot(Optionable<byte>.None);
					Debug.Log($"[ClumsyEffects] Unselected slot {randomSlot} before dropping item.");
				}

				character.refs.view.RPC("DropItemFromSlotRPC", PhotonNetwork.LocalPlayer, randomSlot, dropPos);
				Debug.Log($"[ClumsyEffects] Dropped item from slot {randomSlot} at pos {dropPos}");
			}
		}

		#endregion Role Methods
	}
}