using UnityEngine;
using UnityEngine.UI;
using KomiChallenge.Shared;
using Photon.Pun;
using System.Linq;
using KomiChallenge.Utils;

namespace KomiChallenge.Scripts.Roles
{
	public class Medic : MonoBehaviour
	{
		float HealCooldownTime;
		float HealRadius;
		float HealAmount;
		float HoldDuration;
		KeyCode HealKey;

		public Sprite SkillIcon;

		CharacterAfflictions afflictions;
		Character character;

		float healCooldown = 0f;

		// HUD elements
		Image iconImage;
		Text cooldownText;
		Text keybindText;
		GameObject hudObject;

		// New UI elements for progress bar
		Canvas healCanvas;
		Text chargingText;
		Text timerText;

		bool isHealingHold = false;
		float holdTimer = 0f;
		Slider progressBarSlider;
		
		void Initialize()
		{
			character = GameHelpers.GetCharacterComponent();
			if (character == null)
			{
				Debug.LogError("[Medic] Character component not found — disabling.");
				enabled = false;
				return;
			}

			afflictions = character.refs.afflictions;
			if (afflictions == null)
			{
				Debug.LogError("[Medic] CharacterAfflictions not found — disabling.");
				enabled = false;
				return;
			}

			HealCooldownTime = (PConfig.medic_HealCooldownTime.Value >= Const.medic_HealCooldownTime_Min && PConfig.medic_HealCooldownTime.Value <= Const.medic_HealCooldownTime_Max)
				? PConfig.medic_HealCooldownTime.Value
				: Const.medic_HealCooldownTime;

			HealRadius = (PConfig.medic_HealRadius.Value >= Const.medic_HealRadius_Min && PConfig.medic_HealRadius.Value <= Const.medic_HealRadius_Max)
				? PConfig.medic_HealRadius.Value
				: Const.medic_HealRadius;

			float validatedHealAmountPercent = (PConfig.medic_HealAmountPercent.Value >= Const.medic_HealAmount_Min && PConfig.medic_HealAmountPercent.Value <= Const.medic_HealAmount_Max)
				? PConfig.medic_HealAmountPercent.Value
				: Const.medic_HealAmountPercent;

			// Convert to fraction for actual use
			HealAmount = validatedHealAmountPercent / 100f;

			HoldDuration = (PConfig.medic_HoldDuration.Value >= Const.medic_HoldDuration_Min && PConfig.medic_HoldDuration.Value <= Const.medic_HoldDuration_Max)
				? PConfig.medic_HoldDuration.Value
				: Const.medic_HoldDuration;

			HealKey = PConfig.medic_HealKey.Value != KeyCode.None
				? PConfig.medic_HealKey.Value
				: Const.medic_HealKey;

			string base64Icon = Const.medic_SkillIconBase64;
			SkillIcon = ImageUtils.Base64ToSprite(base64Icon);

			CreateHUD();
			SetupHealingProgressUI();

			GiveStartingItems();
			healCooldown = 0f;

			Debug.Log("[Medic] Medic initialized with starting items.");
		}

		void Start()
		{
			Initialize();
			Debug.Log("[Medic] Medic effects started.");
		}

		void Update()
		{
			if (!character.refs.view.IsMine) return;

			if (healCooldown > 0f)
			{
				healCooldown -= Time.deltaTime;
				if (healCooldown < 0f) healCooldown = 0f;
			}

			UpdateHUD();

			// Handle healing hold logic
			if (healCooldown <= 0f)
			{
				if (Input.GetKey(HealKey))
				{
					if (!isHealingHold)
					{
						// Started holding
						isHealingHold = true;
						holdTimer = 0f;

						// Show healing circle for everyone
						var roleRPC = character.GetComponent<RoleRPC>();
						if (roleRPC != null)
						{
							Vector3 position = character.refs.hip.transform.position + Vector3.up * 1.5f;
							roleRPC.photonView.RPC(nameof(RoleRPC.ShowHealingCircleRPC), RpcTarget.All, position, HealRadius);
						}

						ShowHealingProgressUI(true);
					}

					holdTimer += Time.deltaTime;

					progressBarSlider.value = HoldDuration - holdTimer;
					float secondsLeft = Mathf.Max(HoldDuration - holdTimer, 0f);
					timerText.text = secondsLeft.ToString("F1") + "s";

					if (holdTimer >= HoldDuration)
					{
						// Hold complete, do heal
						isHealingHold = false;
						holdTimer = 0f;

						if (progressBarSlider != null)
							progressBarSlider.value = HoldDuration;

						// Trigger heal RPC
						var roleRPC = character.GetComponent<RoleRPC>();
						if (roleRPC != null)
						{
							Vector3 position = character.refs.hip.transform.position + Vector3.up * 1.5f;
							roleRPC.CallHealRPC(position, HealRadius, HealAmount);

							// Remove healing circle for everyone
							roleRPC.photonView.RPC(nameof(RoleRPC.HideHealingCircleRPC), RpcTarget.All);
						}

						healCooldown = HealCooldownTime;

						ShowHealingProgressUI(false);
					}
				}
				else if (isHealingHold)
				{
					// Released early - cancel healing hold & hide circle
					isHealingHold = false;
					holdTimer = 0f;

					if (progressBarSlider != null)
						progressBarSlider.value = HoldDuration;

					var roleRPC = character.GetComponent<RoleRPC>();
					if (roleRPC != null)
						roleRPC.photonView.RPC(nameof(RoleRPC.HideHealingCircleRPC), RpcTarget.All);

					ShowHealingProgressUI(false);
				}
			}
			else
			{
				ShowHealingProgressUI(false);
			}
		}

		#region Role Methods

		void GiveStartingItems()
		{
			Debug.Log("[Medic] Attempting to give starting medical items...");

			// Load all items once for efficiency
			Item[] allItems = Resources.FindObjectsOfTypeAll<Item>();

			// Find FirstAidKit prefab
			Item firstAidKit = allItems.FirstOrDefault(item => item.name == "FirstAidKit");
			if (firstAidKit != null)
				Debug.Log($"[Medic] Found FirstAidKit with ID: {firstAidKit.itemID}");
			else
				Debug.LogError("[Medic] Could not find FirstAidKit prefab!");
			
			// Find Cure-All prefab
			Item cureAll = allItems.FirstOrDefault(item => item.name == "Cure-All");
			if (cureAll != null)
				Debug.Log($"[Medic] Found Cure-All with ID: {cureAll.itemID}");
			else
				Debug.LogError("[Medic] Could not find Cure-All prefab!");
			
			if (character.player == null)
			{
				Debug.LogError($"[Medic] Character {character.characterName} has no player component!");
				return;
			}

			if (firstAidKit != null)
				TryGiveItemToPlayer(firstAidKit);
		
			if (cureAll != null)
				TryGiveItemToPlayer(cureAll);
		}

		/// <summary>
		/// Attempts to give an item to the player, or drops it on the ground if no slot is available.
		/// </summary>
		void TryGiveItemToPlayer(Item item)
		{
			if (character.player.HasEmptySlot(item.itemID))
			{
				if (!PhotonNetwork.IsMasterClient)
				{
					var roleRPC = character.GetComponent<RoleRPC>();
					if (roleRPC != null)
						roleRPC.photonView.RPC(nameof(RoleRPC.RequestGiveItemRPC), RpcTarget.MasterClient, character.refs.view.ViewID, item.name);
				}
				else
				{
					if (character.player.AddItem(item.itemID, null, out ItemSlot slot))
						Debug.Log($"[Medic] Successfully gave {item.name} to {character.characterName} in slot {slot.itemSlotID}");
					else
						Debug.LogError($"[Medic] Failed to add {item.name} to {character.characterName}");
				}
			}
			else
			{
				Debug.LogWarning($"[MedicEffects] No empty slots available for {item.name} — dropping on ground.");

				// Drop the item at player position
				Vector3 spawnPos = character.refs.hip.transform.position + Vector3.up * 1.5f;

				if (character.refs.view.IsMine)
					character.refs.view.RPC("MedicDropItemRPC", RpcTarget.All, item.name, item.data, spawnPos);
			}
		}

		[PunRPC]
		void MedicDropItemRPC(string prefabName, ItemInstanceData data, Vector3 spawnPosition)
		{
			Debug.Log($"[Medic] Attempting to instantiate prefab '{prefabName}' at {spawnPosition}");

			string prefabPath = "0_Items/" + prefabName;

			var instantiatedObj = PhotonNetwork.Instantiate(prefabPath, spawnPosition, Quaternion.identity, 0);
			if (instantiatedObj == null)
			{
				Debug.LogError($"[Medic] Failed to instantiate prefab '{prefabPath}'");
				return;
			}

			PhotonView component = instantiatedObj.GetComponent<PhotonView>();
			if (component == null)
			{
				Debug.LogError($"[Medic] Instantiated object missing PhotonView component");
				return;
			}

			Debug.Log($"[Medic] Successfully instantiated '{component.gameObject.name}'");
			
			component.RPC("SetItemInstanceDataRPC", RpcTarget.All, data);
			component.RPC("SetKinematicRPC", RpcTarget.All, false, component.transform.position, component.transform.rotation);
		}

		#endregion Role Methods

		#region HUD Methods

		void CreateHUD()
		{
			GameObject canvasHUD = GameObject.Find("GAME/GUIManager/Canvas_HUD");
			if (canvasHUD == null)
			{
				Debug.LogError("[MedicHUD] Canvas_HUD not found.");
				return;
			}

			hudObject = new GameObject("MedicSkillIcon");
			hudObject.transform.SetParent(canvasHUD.transform, false);

			RectTransform rect = hudObject.AddComponent<RectTransform>();
			rect.anchorMin = new Vector2(0.9f, 0.15f); // Bottom right-ish
			rect.anchorMax = rect.anchorMin;
			rect.sizeDelta = new Vector2(64, 64);

			// Icon image
			iconImage = hudObject.AddComponent<Image>();
			iconImage.sprite = SkillIcon;
			iconImage.type = Image.Type.Filled;
			iconImage.fillMethod = Image.FillMethod.Radial360;
			iconImage.fillOrigin = 2;
			iconImage.fillClockwise = false;
			iconImage.color = Color.gray;
			iconImage.fillAmount = 1f;

			// Cooldown text
			GameObject cooldownObj = new GameObject("CooldownText");
			cooldownObj.transform.SetParent(hudObject.transform, false);
			cooldownText = cooldownObj.AddComponent<Text>();
			cooldownText.alignment = TextAnchor.MiddleCenter;
			cooldownText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			cooldownText.fontSize = 20;
			cooldownText.color = Color.white;
			cooldownText.rectTransform.sizeDelta = new Vector2(64, 64);

			// Keybind text
			GameObject keybindObj = new GameObject("KeybindText");
			keybindObj.transform.SetParent(hudObject.transform, false);
			keybindText = keybindObj.AddComponent<Text>();
			keybindText.alignment = TextAnchor.UpperCenter;
			keybindText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			keybindText.fontSize = 14;
			keybindText.color = Color.yellow;
			keybindText.text = $"[{HealKey}]";
			keybindText.rectTransform.anchoredPosition = new Vector2(0, -40);
			keybindText.gameObject.SetActive(false);
		}

		void UpdateHUD()
		{
			if (healCooldown > 0f)
			{
				cooldownText.text = Mathf.CeilToInt(healCooldown).ToString();
				keybindText.gameObject.SetActive(false);
				iconImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
			}
			else
			{
				cooldownText.text = "";
				keybindText.gameObject.SetActive(true);
				iconImage.color = Color.Lerp(Color.white, Color.green, Mathf.PingPong(Time.time * 2f, 1f)); // Flash effect
			}
		}

		void SetupHealingProgressUI()
		{
			// Create Canvas
			GameObject canvasGO = new GameObject("MedicHealCanvas");
			healCanvas = canvasGO.AddComponent<Canvas>();
			healCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvasGO.AddComponent<CanvasScaler>();
			canvasGO.AddComponent<GraphicRaycaster>();

			// Create Slider GameObject
			GameObject sliderGO = new GameObject("ProgressBarSlider");
			sliderGO.transform.SetParent(canvasGO.transform);

			Slider slider = sliderGO.AddComponent<Slider>();

			// Setup RectTransform
			RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
			sliderRect.sizeDelta = new Vector2(300, 25);
			sliderRect.anchorMin = new Vector2(0.5f, 0f);
			sliderRect.anchorMax = new Vector2(0.5f, 0f);
			sliderRect.pivot = new Vector2(0.5f, 0f);
			sliderRect.anchoredPosition = new Vector2(0, 100);

			// Setup Slider values
			slider.minValue = 0f;
			slider.maxValue = HoldDuration;  // e.g. 5 seconds
			slider.value = 0f;

			// Create background (black border)
			GameObject backgroundGO = new GameObject("Background");
			backgroundGO.transform.SetParent(sliderGO.transform);
			Image bgImage = backgroundGO.AddComponent<Image>();
			bgImage.color = Color.black;

			RectTransform bgRect = backgroundGO.GetComponent<RectTransform>();
			bgRect.anchorMin = new Vector2(0, 0);
			bgRect.anchorMax = new Vector2(1, 1);
			bgRect.offsetMin = Vector2.zero;
			bgRect.offsetMax = Vector2.zero;

			slider.targetGraphic = bgImage;

			// Create fill area
			GameObject fillAreaGO = new GameObject("Fill Area");
			fillAreaGO.transform.SetParent(sliderGO.transform);
			RectTransform fillAreaRect = fillAreaGO.AddComponent<RectTransform>();
			fillAreaRect.anchorMin = new Vector2(0, 0);
			fillAreaRect.anchorMax = new Vector2(1, 1);
			fillAreaRect.offsetMin = new Vector2(5, 5);  // leave space for border
			fillAreaRect.offsetMax = new Vector2(-5, -5);

			// Create fill image
			GameObject fillGO = new GameObject("Fill");
			fillGO.transform.SetParent(fillAreaGO.transform);
			Image fillImage = fillGO.AddComponent<Image>();
			fillImage.color = Color.green;

			RectTransform fillRect = fillGO.GetComponent<RectTransform>();
			fillRect.anchorMin = new Vector2(0, 0);
			fillRect.anchorMax = new Vector2(1, 1);
			fillRect.offsetMin = Vector2.zero;
			fillRect.offsetMax = Vector2.zero;

			slider.fillRect = fillRect;

			// Disable interaction
			slider.interactable = false;

			// Save slider reference to a class variable for updating
			progressBarSlider = slider;

			progressBarSlider.gameObject.SetActive(false);

			GameObject chargingGO = new GameObject("ChargingText");
			chargingGO.transform.SetParent(canvasGO.transform);
			chargingText = chargingGO.AddComponent<Text>();
			chargingText.alignment = TextAnchor.MiddleCenter;
			chargingText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			chargingText.fontSize = 18;
			chargingText.color = Color.white;
			chargingText.text = "Préparation du soin...";
			RectTransform chargingRect = chargingGO.GetComponent<RectTransform>();
			chargingRect.anchorMin = new Vector2(0.5f, 0f);
			chargingRect.anchorMax = new Vector2(0.5f, 0f);
			chargingRect.anchoredPosition = new Vector2(0, 140);
			chargingRect.sizeDelta = new Vector2(200, 30);
			chargingText.gameObject.SetActive(false);

			// Timer Text
			GameObject timerGO = new GameObject("TimerText");
			timerGO.transform.SetParent(canvasGO.transform);
			timerText = timerGO.AddComponent<Text>();
			timerText.alignment = TextAnchor.MiddleCenter;
			timerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			timerText.fontSize = 18;
			timerText.color = Color.yellow;
			timerText.text = "";
			RectTransform timerRect = timerGO.GetComponent<RectTransform>();
			timerRect.anchorMin = new Vector2(0.5f, 0f);
			timerRect.anchorMax = new Vector2(0.5f, 0f);
			timerRect.anchoredPosition = new Vector2(0, 80);
			timerRect.sizeDelta = new Vector2(200, 30);
			timerText.gameObject.SetActive(false);
		}

		void ShowHealingProgressUI(bool show)
		{
			if (healCanvas != null)
				healCanvas.gameObject.SetActive(show);

			if (chargingText != null)
				chargingText.gameObject.SetActive(show);

			if (timerText != null)
				timerText.gameObject.SetActive(show);

			if (progressBarSlider != null)
				progressBarSlider.gameObject.SetActive(show);

			if (show && progressBarSlider != null)
				progressBarSlider.value = HoldDuration; // start full

			if (!show && timerText != null)
				timerText.text = "";
		}

		#endregion
	}
}
