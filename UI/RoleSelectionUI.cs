using PeakArchetypes.Scripts;
using Photon.Pun;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace PeakArchetypes.UI;

public class RoleSelectionUI : MonoBehaviour
{
	Text displayText;
	GameObject panel;
	int selectedIndex = -1;
	string lang;

	List<Role> RoleList => RoleManager.defaultTypes;

	#region Unity Methods 

	void Start()
	{
		// Detect system language
		lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
		if (lang != "en" && lang != "fr") lang = "en";

		CreateUI();
	}

	void OnDestroy()
	{
		if (panel != null)
			Destroy(panel);

		if(gameObject != null)
			Destroy(gameObject);
	}

	void Update()
	{
		// Role selection: support top-row & numpad keys
		for (int i = 0; i < RoleList.Count; i++)
		{
			KeyCode alphaKey = KeyCode.Alpha0 + i;
			KeyCode keypadKey = KeyCode.Keypad0 + i;

			if (Input.GetKeyDown(alphaKey) || Input.GetKeyDown(keypadKey))
			{
				selectedIndex = i;
				UpdateDisplayText();
				Debug.Log($">>> Selected index {i}: {RoleList[i].RoleName}");
				break;
			}
		}

		if (selectedIndex != -1 && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
			OnConfirm();
	}

	#endregion Unity Methods

	#region UI Methods

	void CreateUI()
	{
		GameObject canvas = GameObject.Find("GAME/GUIManager/Canvas_HUD");
		if (canvas == null)
		{
			Debug.Log(">>> Canvas not found in scene.");
			return;
		}

		int roleCount = RoleList.Count;
		int lineHeight = 25; // Approx. one text line
		int padding = 20;
		int panelHeight = Mathf.Max(400, roleCount * lineHeight + padding);

		// Panel
		panel = new GameObject("RoleSelectionPanel");
		panel.transform.SetParent(canvas.transform, false);
		panel.AddComponent<CanvasRenderer>();
		Image bg = panel.AddComponent<Image>();
		bg.color = new Color(0, 0, 0, 0.8f);

		RectTransform panelRect = panel.GetComponent<RectTransform>();
		panelRect.sizeDelta = new Vector2(600, panelHeight);
		panelRect.anchoredPosition = new Vector2(0, 0);

		// Text
		GameObject textObj = new("RoleText");
		textObj.transform.SetParent(panel.transform, false);
		displayText = textObj.AddComponent<Text>();
		displayText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		displayText.fontSize = 20;
		displayText.alignment = TextAnchor.UpperLeft;
		displayText.color = Color.white;

		RectTransform textRect = displayText.GetComponent<RectTransform>();
		textRect.sizeDelta = new Vector2(580, panelHeight - padding);
		textRect.anchoredPosition = new Vector2(0, 0);

		UpdateDisplayText();
	}

	void OnConfirm()
	{
		if (selectedIndex < 0 || selectedIndex >= RoleList.Count) return;

		Role selected = RoleList[selectedIndex];
		int localId = PhotonNetwork.LocalPlayer.ActorNumber;

		if (RoleManager.players.ContainsKey(localId))
			RoleManager.players[localId] = selected;
		else
			RoleManager.players.Add(localId, selected);

		Debug.Log($">>> Selected role: {selected.RoleName}");

		Destroy(panel);
		Destroy(gameObject);
	}

	void UpdateDisplayText()
	{
		string text = Localization.Get(lang, "ChooseRole") + "\n\n";

		for (int i = 0; i < RoleList.Count; i++)
		{
			string prefix = (i == selectedIndex) ? "➡ " : "   ";
			text += $"{prefix}<b>{i}</b>. {RoleList[i].RoleName} - {RoleList[i].Desc}\n";
		}

		text += "\n" + Localization.Get(lang, "EnterNumber");
		displayText.text = text;
	}

	#endregion
}
