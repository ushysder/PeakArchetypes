using System.ComponentModel;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.UI;
using static KomiChallenge.Scripts.AssignRoles;

namespace KomiChallenge.Comps
{

    public class Role : MonoBehaviour
    {
        public string roleName { get; set; }
        public string desc { get; set; }
        public RoleType roleType { get; set; }

        public Recorder rec;
        public GameObject blackScreen;
        private static bool blinded = false;

        public Role(string roleName, string desc, RoleType roleType)
        {
            this.roleName = roleName;
            this.desc = desc;
            this.roleType = roleType;

        }

        private void Start()
        {
            blinded = false;
        }

        private void Update()
        {
            ActivateDebuff();
        }

        private void OnDestroy()
        {
            if (blackScreen != null)
            {
                Destroy(blackScreen);
                blinded = false;
            }
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
            if (blinded) return;
            GameObject parent = GameObject.Find("GAME/GUIManager/Canvas_HUD");
            blackScreen = new GameObject("blackScreen");
            blackScreen.transform.SetParent(parent.transform, false);
            blackScreen.AddComponent<Image>().color = Color.black;

            blackScreen.transform.SetAsFirstSibling();

            RectTransform rect = blackScreen.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(7200, 7200);


            blinded = true;
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
