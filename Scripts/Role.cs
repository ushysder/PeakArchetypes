using KomiChallenge.Scripts.Roles;
using System;
using System.Collections.Generic;
using UnityEngine;
using static KomiChallenge.Scripts.RoleManager;

namespace KomiChallenge.Scripts;

public class Role : MonoBehaviour
{
	static readonly Dictionary<RoleType, Type> roleComponentMap = new()
	{
		{ RoleType.blind, typeof(BlindEffect) },
		{ RoleType.deaf, typeof(DeafEffect) },
		{ RoleType.mute, typeof(MuteEffect) },
		{ RoleType.clumsy, typeof(ClumsyEffects) },
		{ RoleType.drunk, typeof(DrunkEffects) },
		{ RoleType.drugs, typeof(DrugsEffects) },
		{ RoleType.narcoleptic, typeof(NarcolepticEffect) },
		{ RoleType.oneEyed, typeof(OneEyedEffects) },
		{ RoleType.medic, typeof(MedicEffects) }
		// RoleType.nothing is intentionally excluded
	};

	public Role(string roleName, string desc, RoleType roleType)
	{
		RoleName = roleName;
		Desc = desc;
		RoleType = roleType;
	}

	public string Desc { get; set; }
	public string RoleName { get; set; }
	public RoleType RoleType { get; set; }

	#region Unity Methods

	void OnDestroy() => ApplyRoleEffect(false);

	void Start() => ApplyRoleEffect(true);

	#endregion Unity Methods

	#region Role Methods

	void ApplyRoleEffect(bool add)
	{
		var character = Character.localCharacter;
		if (character == null) return;

		var characterGO = character.gameObject;

		// Skip if role has no effect mapped
		if (!roleComponentMap.TryGetValue(RoleType, out var componentType) || componentType == null)
			return;

		var existing = characterGO.GetComponent(componentType);

		if (add)
		{
			if (existing == null)
				characterGO.AddComponent(componentType);	
		}
		else
		{
			if (existing != null)
				Destroy(existing);
		}
	}

	#endregion Role Methods
}
