using System;
using System.Collections.Generic;
using System.Text;
using MonoMod.Cil;
using UnityEngine;

namespace KomiChallenge.Comps
{
    public enum RoleType
    {
        blind,
        deaf,
        mute
    }

    public class Role : MonoBehaviour
    {
        public string roleName { get; }
        public string desc { get; }
        public RoleType roleType { get; }

        public Role(string roleName, string desc, RoleType roleType)
        {
            roleName = this.roleName;
            desc = this.desc;
            roleType = this.roleType;

        }

        public void ActivateDebuff()
        {


            switch (roleType)
            {
                case RoleType.blind:
                    break;
            }
        }

        public Sprite GetIcon(Sprite Icon)
        {
            switch (roleType)
            {
                case RoleType.blind:
                    return Icon;
                default:
                    return Icon;
            }
        }
    }
}
