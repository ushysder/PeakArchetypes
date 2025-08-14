using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using static CharacterAfflictions;
using KomiChallenge.Utils;
using KomiChallenge.Shared;

namespace KomiChallenge.Scripts.Roles
{
	public class OneEyed : MonoBehaviour
	{
		CharacterAfflictions afflictions;
		Character character;

		float targetInjury;
		GameObject blackHalfScreen;

		#region Unity Methods

		void Initialize()
		{
			character = GameHelpers.GetCharacterComponent();
			if (character == null)
			{
				Debug.LogError("[OneEyed] Character component not found — disabling.");
				enabled = false;
				return;
			}

			afflictions = character.refs.afflictions;
			if (afflictions == null)
			{
				Debug.LogError("[OneEyed] CharacterAfflictions not found — disabling.");
				enabled = false;
				return;
			}

			float configInjuryPercent = PConfig.oneEyed_targetInjuryPercent.Value;

			if (configInjuryPercent < Const.oneEyed_targetInjuryPercent_Min || configInjuryPercent > Const.oneEyed_targetInjuryPercent_Max)
			{
				Debug.LogWarning($"[OneEyed] Invalid injury percentage {configInjuryPercent} — using default 50%");
				configInjuryPercent = Const.oneEyed_targetInjuryPercent;
			}

			targetInjury = configInjuryPercent / 100f;

			Debug.Log($"[OneEyed] targetInjury set to {targetInjury} ({configInjuryPercent}%)");

			CreateHalfBlackScreen();
		}

		void OnDestroy()
		{
			StopCoroutine(OneEyedRoutine());

			if (blackHalfScreen != null)
				Destroy(blackHalfScreen);

			Debug.Log("[OneEyed] Reset complete on destroy.");
		}

		void Start()
		{
			Initialize();
			StartCoroutine(OneEyedRoutine());
			Debug.Log("[OneEyed] OneEyed Effects started.");
		}

		#endregion Unity Methods

		#region Role Methods

		void CreateHalfBlackScreen()
		{
			GameObject parent = GameObject.Find("GAME/GUIManager/Canvas_HUD");
			if (parent == null)
			{
				Debug.LogError("[OneEyed] Canvas_HUD not found — cannot create black screen overlay.");
				return;
			}

			blackHalfScreen = new GameObject("blackHalfScreen");
			blackHalfScreen.transform.SetParent(parent.transform, false);

			var img = blackHalfScreen.AddComponent<Image>();
			img.color = Color.black;

			var rect = blackHalfScreen.GetComponent<RectTransform>();
			rect.anchorMin = new Vector2(0.5f, 0f); // Right half of the screen
			rect.anchorMax = new Vector2(1f, 1f);
			rect.offsetMin = Vector2.zero;
			rect.offsetMax = Vector2.zero;

			blackHalfScreen.transform.SetAsFirstSibling();

			Debug.Log("[OneEyed] Half-screen black overlay created.");
		}

		IEnumerator OneEyedRoutine()
		{
			while (true)
			{
				float currentInjury = afflictions.GetCurrentStatus(STATUSTYPE.Injury);

				if (currentInjury < targetInjury)
				{
					afflictions.AddStatus(STATUSTYPE.Injury, targetInjury - currentInjury);
					Debug.Log($"[OneEyed] Injury level restored to {targetInjury}");
				}

				yield return new WaitForSeconds(1f);
			}
		}

		#endregion Role Methods
	}
}