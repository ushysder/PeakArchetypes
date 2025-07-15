using System.ComponentModel;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;
using static KomiChallenge.Scripts.AssignRoles;

namespace KomiChallenge.Comps
{

    public class Role : MonoBehaviour
    {
        public string roleName { get; set; }
        public string desc { get; set; }
        public RoleType roleType { get; set; }

        public Recorder rec;

        public Role(string roleName, string desc, RoleType roleType)
        {
            roleName = this.roleName;
            desc = this.desc;
            roleType = this.roleType;

        }

        private void Update()
        {
            ActivateDebuff();
        }

        public void ActivateDebuff()
        {
            switch (roleType)
            {
                case RoleType.blind:
                    BlindUser();
                    break;
                case RoleType.deaf:
                    DeafUser();
                    break;
                case RoleType.mute:
                    MuteUser();
                    break;
            }
        }



        private void BlindUser()
        {
            MainCamera.instance.transform.GetComponent<Camera>().enabled = false;
        }

        private void DeafUser()
        {
            AudioListener.volume = 0;
        }


        private void MuteUser()
        {
            rec = Character.localCharacter.GetComponent<PhotonVoiceView>().RecorderInUse;
            rec.TransmitEnabled = false;
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
