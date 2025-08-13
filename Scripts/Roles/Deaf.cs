using UnityEngine;

namespace KomiChallenge.Scripts.Roles;
public class Deaf : MonoBehaviour
{
	void Start()
	{
		AudioListener.volume = 0;
		Debug.Log("[DeafEffect] Deaf effect started.");
	}

	void OnDestroy()
	{
		AudioListener.volume = 1;
		Debug.Log("[DeafEffect] Deaf effect destroyed.");
	}
}

