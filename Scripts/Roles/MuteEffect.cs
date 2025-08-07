using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;

namespace KomiChallenge.Scripts.Roles;

public class MuteEffect : MonoBehaviour
{
	Recorder recorder;

	void OnEnable()
	{
		recorder = Character.localCharacter.GetComponent<PhotonVoiceView>()?.RecorderInUse;
		if (recorder != null)
			recorder.TransmitEnabled = false;
		Debug.Log("[MuteEffect] Mute effect enabled.");
	}

	void OnDisable()
	{
		if (recorder != null)
			recorder.TransmitEnabled = true;
		Debug.Log("[MuteEffect] Mute effect disabled.");
	}
}
