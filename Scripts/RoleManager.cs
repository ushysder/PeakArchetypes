using Photon.Pun;
using System.Collections.Generic;
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

	public static void ApplyDebuffs()
	{
		int localID = Character.localCharacter.gameObject.GetComponent<PhotonView>().Owner.ActorNumber;
		if (players.ContainsKey(localID))
		{
			Role plrRole = Character.localCharacter.gameObject.GetComponent<Role>();
			if (plrRole == null)
				plrRole = Character.localCharacter.gameObject.AddComponent<Role>();

			plrRole.RoleName = players[localID].RoleName;
			plrRole.RoleType = players[localID].RoleType;
			plrRole.Desc = players[localID].Desc;
		}
		Debug.Log("[AssignRoles] Debuffs applied to character.");
	}

	public static void RemoveDebuffs()
	{
		Role plrRole = Character.localCharacter.GetComponent<Role>();
		Object.Destroy(plrRole);

		Debug.Log("[AssignRoles] Debuffs removed from character.");
	}
}
