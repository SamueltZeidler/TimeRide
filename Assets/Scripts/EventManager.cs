using UnityEngine;

public class EventManager : MonoBehaviour
{
	public GameObject targetObject;

	private bool lastActiveState = false;

	void Start()
	{
		if (targetObject != null)
		{
			lastActiveState = targetObject.activeSelf;
			UpdateOrientation(lastActiveState);
		}
	}

	void Update()
	{
		if (targetObject != null && targetObject.activeSelf != lastActiveState)
		{
			lastActiveState = targetObject.activeSelf;
			UpdateOrientation(lastActiveState);
		}
	}

	void UpdateOrientation(bool isActive)
	{
#if UNITY_ANDROID || UNITY_IOS
        // Automatische Drehung komplett deaktivieren
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.orientation = ScreenOrientation.AutoRotation; // muss gesetzt werden, bevor Fix kommt

        if (isActive)
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            UnityEngine.Debug.Log("Querformat aktiviert");
        }
        else
        {
            Screen.orientation = ScreenOrientation.Portrait;
            UnityEngine.Debug.Log("Hochformat aktiviert");
        }
#else
		UnityEngine.Debug.Log("Orientierungswechsel nur auf Mobilger�ten wirksam.");
#endif
	}
}
