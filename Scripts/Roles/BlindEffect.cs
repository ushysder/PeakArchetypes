using UnityEngine;
using UnityEngine.UI;

namespace KomiChallenge.Scripts.Roles;

public class BlindEffect : MonoBehaviour
{
	GameObject blackScreen;

	void OnDestroy()
	{
		if (blackScreen != null)
			Destroy(blackScreen);
		Debug.Log("[BlindEffect] Blind effect destroyed.");
	}

	void Start()
	{
		GameObject parent = GameObject.Find("GAME/GUIManager/Canvas_HUD");

		blackScreen = new GameObject("blackScreen");
		blackScreen.transform.SetParent(parent.transform, false);
		blackScreen.AddComponent<Image>().color = Color.black;
		blackScreen.transform.SetAsFirstSibling();

		RectTransform rect = blackScreen.GetComponent<RectTransform>();
		rect.sizeDelta = new Vector2(7200, 7200);

		Debug.Log("[BlindEffect] Blind effect started.");
	}
}

