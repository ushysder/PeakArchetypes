using UnityEngine;

namespace PeakArchetypes.Scripts.Roles;
public class Deaf : MonoBehaviour
{
	void Start()
	{
		AudioListener.volume = 0;
		Debug.Log("[Deaf] Deaf effect started.");
	}

	void OnDestroy()
	{
		AudioListener.volume = 1;
		Debug.Log("[Deaf] Deaf effect destroyed.");
	}
}

