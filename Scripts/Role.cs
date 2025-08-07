using KomiChallenge.Scripts.Roles;
using UnityEngine;
using static KomiChallenge.Scripts.RoleManager;

namespace KomiChallenge.Scripts;

public class Role : MonoBehaviour
{
	public RoleType RoleType { get; set; }
	public string RoleName { get; set; }
	public string Desc { get; set; }

	public Role(string roleName, string desc, RoleType roleType)
	{
		RoleName = roleName;
		Desc = desc;
		RoleType = roleType;

	}

	void Start() => GiveDebuff();

	public void GiveDebuff()
	{
		var character = GameHelpers.GetCharacterComponent();
		if (character == null) return;

		switch (RoleType)
		{
			case RoleType.blind:
				if (character.GetComponent<BlindEffect>() == null)
					character.gameObject.AddComponent<BlindEffect>();
				break;
			case RoleType.deaf:
				if (character.GetComponent<DeafEffect>() == null)
					character.gameObject.AddComponent<DeafEffect>();
				break;
			case RoleType.mute:
				if (character.GetComponent<MuteEffect>() == null)
					character.gameObject.AddComponent<MuteEffect>();
				break;
			case RoleType.clumsy:
				if (character.GetComponent<ClumsyEffects>() == null)
					character.gameObject.AddComponent<ClumsyEffects>();
				break;
			case RoleType.drunk:
				if (character.GetComponent<DrunkController>() == null)
					character.gameObject.AddComponent<DrunkController>();
				break;
			case RoleType.drugs:
				if (character.GetComponent<DrugsEffects>() == null)
					character.gameObject.AddComponent<DrugsEffects>();
				break;
			case RoleType.nothing:
				break;
		}
	}
}
