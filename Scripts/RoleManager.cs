using PeakArchetypes.Scripts.Roles;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace PeakArchetypes.Scripts;

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
		narcoleptic,
		oneEyed,
		medic
	}

	public static void AppendRoles()
	{
		string lang = Localization.GetSystemLang();

		void AddRole(string key, RoleType type)
		{
			string roleName = Localization.Get(lang, key + "_Name");
			string roleDesc = Localization.Get(lang, key + "_Desc");
			defaultTypes.Add(new Role(roleName, roleDesc, type));
		}

		AddRole("Basic", RoleType.nothing);
		AddRole(nameof(Blind), RoleType.blind);
		AddRole(nameof(Deaf), RoleType.deaf);
		AddRole(nameof(Mute), RoleType.mute);
		AddRole(nameof(Drugs), RoleType.drugs);
		AddRole(nameof(Drunk), RoleType.drunk);
		AddRole(nameof(Clumsy), RoleType.clumsy);
		AddRole(nameof(Narcoleptic), RoleType.narcoleptic);
		AddRole(nameof(OneEyed), RoleType.oneEyed);
		AddRole(nameof(Medic), RoleType.medic);
	}

	public static void ApplyDebuffs()
	{
		int localID = Character.localCharacter.gameObject.GetComponent<PhotonView>().Owner.ActorNumber;
		if (players.ContainsKey(localID))
		{
			Role plrRole = Character.localCharacter.gameObject.GetComponent<Role>() ?? Character.localCharacter.gameObject.AddComponent<Role>();
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
