using System.Collections;
using UnityEngine;

public class EventManager : MonoBehaviour
{
	public GameObject targetObject1;
	public GameObject targetObject2;

	private bool lastActiveState1 = false;
	private bool lastActiveState2 = false;

	void Start()
	{
		if (targetObject1 != null)
			lastActiveState1 = targetObject1.activeSelf;
		if (targetObject2 != null)
			lastActiveState2 = targetObject2.activeSelf;

		UpdateOrientation(lastActiveState1 || lastActiveState2);
	}

	void Update()
	{
		bool currentState1 = targetObject1 != null && targetObject1.activeSelf;
		bool currentState2 = targetObject2 != null && targetObject2.activeSelf;

		if (currentState1 != lastActiveState1 || currentState2 != lastActiveState2)
		{
			lastActiveState1 = currentState1;
			lastActiveState2 = currentState2;
			UpdateOrientation(currentState1 || currentState2);
		}
	}

	void UpdateOrientation(bool anyActive)
	{
#if UNITY_ANDROID || UNITY_IOS
        StopAllCoroutines(); // Beende laufende Änderungen, um Konflikte zu vermeiden
        StartCoroutine(SafeOrientationChange(anyActive));
#else
		UnityEngine.Debug.Log("Orientierungswechsel nur auf Mobilgeräten wirksam.");
#endif
	}

	IEnumerator SafeOrientationChange(bool anyActive)
	{
		yield return new WaitForEndOfFrame(); // Warten bis der Frame fertig ist
		Screen.orientation = ScreenOrientation.AutoRotation;
		yield return null; // Einen Frame warten für Stabilität

		if (anyActive)
		{
			Screen.orientation = ScreenOrientation.LandscapeLeft;
			UnityEngine.Debug.Log("Querformat aktiviert (Coroutine)");
		}
		else
		{
			Screen.orientation = ScreenOrientation.Portrait;
			UnityEngine.Debug.Log("Hochformat aktiviert (Coroutine)");
		}
	}
}
