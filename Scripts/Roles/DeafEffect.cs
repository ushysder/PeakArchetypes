using UnityEngine;

namespace KomiChallenge.Scripts.Roles;
public class DeafEffect : MonoBehaviour
{
	void OnEnable()
	{
		AudioListener.volume = 0;
		Debug.Log("[DeafEffect] Deaf effect enabled.");
	}

	void OnDisable()
	{
		AudioListener.volume = 1;
		Debug.Log("[DeafEffect] Deaf effect disabled.");
	}
}

