using System.Collections.Generic;
using KomiChallenge.Scripts.Roles;
using UnityEngine;

namespace KomiChallenge.Scripts;

public class RoleManager
{
	public static List<Role> defaultTypes = [];

	public static Dictionary<int, Role> players = [];

	public enum RoleType
	{
		blind,
		deaf,
		mute,
		drunk,
		drugs,
		clumsy,
		nothing,
		narcoleptic
	}

	public static void AppendRoles()
	{
		defaultTypes.Add(new Role(
		"Basique",
		"Vous n'avez pas les bases",
		RoleType.nothing
		));

		defaultTypes.Add(new Role(
		"Aveugle",
		"Comme Daredevil, stylé non ?",
		RoleType.blind
		));

		defaultTypes.Add(new Role(
		"Sourd",
		"Mais pas muet, n'hésite pas à gueuler.",
		RoleType.deaf
		));

		defaultTypes.Add(new Role(
		"Muet",
		"Tg batar",
		RoleType.mute
		));

		defaultTypes.Add(new Role(
		"Drogué",
		"C'est de la bonne",
		RoleType.drugs));

		defaultTypes.Add(new Role(
		"Bourré",
		"Arrête de picoler, c'est pas sameglou !",
		RoleType.drunk));

		defaultTypes.Add(new Role(
		"Maladroit",
		"Fais attention où tu mets les pieds.",
		RoleType.clumsy));

		defaultTypes.Add(new Role(
		"Narcoleptique",
		"Si tu trouves quelqu’un qui ronfle, c’est sûrement toi !",
		RoleType.narcoleptic));
	}

	public static void ReapplyDebuffs()
	{
		var character = GameHelpers.GetCharacterComponent();
		if (character == null) return;

		// Re-enable all debuff components (if they still exist but are disabled)
		var debuffs = new MonoBehaviour[]
		{
		character.GetComponent<DrugsEffects>(),
		character.GetComponent<DrunkController>(),
		character.GetComponent<ClumsyEffects>(),
		character.GetComponent<BlindEffect>(),
		character.GetComponent<DeafEffect>(),
		character.GetComponent<MuteEffect>(),
		character.GetComponent<NarcolepticEffect>()
		};

		foreach (var debuff in debuffs)
		{
			if (debuff != null)
				debuff.enabled = true;
		}

		Debug.Log("[AssignRoles] Debuffs re-applied to character.");
	}

	public static void RemoveAllDebuffs()
	{
		var character = GameHelpers.GetCharacterComponent();
		if (character == null) return;

		foreach (var effect in character.GetComponents<MonoBehaviour>())
		{
			if (effect is BlindEffect ||
				effect is DeafEffect ||
				effect is MuteEffect ||
				effect is DrunkController ||
				effect is ClumsyEffects ||
				effect is DrugsEffects ||
				effect is NarcolepticEffect) 
			{
				effect.enabled = false;
			}
		}

		Debug.Log("[AssignRoles] All debuffs removed from character.");
	}
}
