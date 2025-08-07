using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using KomiChallenge.Scripts;

namespace KomiChallenge.UI;

public class RoleSelectionUI : MonoBehaviour
{
	Text displayText;
	GameObject panel;
	int selectedIndex = -1;

	List<Role> RoleList => RoleManager.defaultTypes;

	void CreateUI()
	{
		GameObject canvas = GameObject.Find("GAME/GUIManager/Canvas_HUD");
		if (canvas == null)
		{
			Debug.Log(">>> Canvas not found in scene.");
			return;
		}

		// Panel
		panel = new GameObject("RoleSelectionPanel");
		panel.transform.SetParent(canvas.transform, false);
		panel.AddComponent<CanvasRenderer>();
		Image bg = panel.AddComponent<Image>();
		bg.color = new Color(0, 0, 0, 0.8f);

		RectTransform panelRect = panel.GetComponent<RectTransform>();
		panelRect.sizeDelta = new Vector2(600, 300);
		panelRect.anchoredPosition = new Vector2(0, 0);

		// Text
		GameObject textObj = new GameObject("RoleText");
		textObj.transform.SetParent(panel.transform, false);
		displayText = textObj.AddComponent<Text>();
		displayText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		displayText.fontSize = 20;
		displayText.alignment = TextAnchor.UpperLeft;
		displayText.color = Color.white;

		RectTransform textRect = displayText.GetComponent<RectTransform>();
		textRect.sizeDelta = new Vector2(580, 280);
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


		Debug.Log($">>> Sent role selection: {selected.RoleName}");

		Destroy(panel);
	}

	void Start() => CreateUI();

	void OnDestroy()
	{
		if (panel != null)
			Destroy(panel);
	}

	void Update()
	{
		// Role selection: support top-row & numpad keys (1–7)
		for (int i = 0; i < 7; i++)
		{
			KeyCode alphaKey = KeyCode.Alpha1 + i;
			KeyCode keypadKey = KeyCode.Keypad1 + i;

			if (Input.GetKeyDown(alphaKey) || Input.GetKeyDown(keypadKey))
			{
				selectedIndex = i;
				UpdateDisplayText();
				Debug.Log($">>> Selected index {i}: {RoleList[i].RoleName}");
				break;
			}
		}

		// Confirm: support both Return and Numpad Enter
		if (selectedIndex != -1 &&
			(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
		{
			OnConfirm();
		}
	}

	void UpdateDisplayText()
	{
		string text = "<b>Choisissez votre rôle :</b>\n\n";

		for (int i = 0; i < RoleList.Count; i++)
		{
			string prefix = (i == selectedIndex) ? "➡ " : "   ";
			text += $"{prefix}<b>{i + 1}</b>. {RoleList[i].RoleName} - {RoleList[i].Desc}\n";
		}

		text += "\nAppuyez sur un chiffre (1-7) pour choisir, puis Entrée pour confirmer.";
		displayText.text = text;
	}
}
