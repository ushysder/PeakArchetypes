using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;

namespace PeakArchetypes.Scripts.Roles;

public class Mute : MonoBehaviour
{
	Recorder recorder;

	void Start()
	{
		recorder = Character.localCharacter.GetComponent<PhotonVoiceView>()?.RecorderInUse;
		if (recorder != null)
			recorder.TransmitEnabled = false;
		Debug.Log("[Mute] Mute effect started.");
	}

	void OnDestroy()
	{
		if (recorder != null)
			recorder.TransmitEnabled = true;
		Debug.Log("[Mute] Mute effect destroyed.");
	}
}
