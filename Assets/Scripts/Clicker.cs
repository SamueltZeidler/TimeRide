using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Clicker : MonoBehaviour
{
	[Header("UI & Systems")]
	[SerializeField] private GraphicRaycaster uiRaycaster;
	[SerializeField] private EventSystem eventSystem;
	[SerializeField] private TextMeshProUGUI counterText;

	[Header("Audio")]
	[SerializeField] private AudioClip clickSound;
	[SerializeField] private AudioClip milestoneSound;
	private AudioSource audioSource;

	[Header("Milestone")]
	[SerializeField] private int milestoneThreshold = 10;
	[SerializeField] private GameObject milestoneObject;

	[Header("Orientation Control")]
	[SerializeField] private GameObject portraitTrigger;
	[SerializeField] private GameObject landscapeTrigger;

	private int clickCount = 0;

	void Start()
	{
		if (uiRaycaster == null)
			uiRaycaster = FindObjectOfType<GraphicRaycaster>();

		if (eventSystem == null)
			eventSystem = FindObjectOfType<EventSystem>();

		audioSource = GetComponent<AudioSource>();
		if (audioSource == null)
			audioSource = gameObject.AddComponent<AudioSource>();

		UpdateCounterText();
	}

	void Update()
	{
		HandleOrientation();

		Vector2 clickPosition = Vector2.zero;

		if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
			clickPosition = Mouse.current.position.ReadValue();
		else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
			clickPosition = Touchscreen.current.primaryTouch.position.ReadValue();
		else
			return;

		PointerEventData pointerData = new PointerEventData(eventSystem)
		{
			position = clickPosition
		};

		var results = new System.Collections.Generic.List<RaycastResult>();
		uiRaycaster.Raycast(pointerData, results);

		foreach (var result in results)
		{
			GameObject go = result.gameObject;

			if (go.CompareTag("Clickable") && go.GetComponent<ClickableState>() == null)
			{
				go.AddComponent<ClickableState>();
				clickCount++;
				UnityEngine.Debug.Log("Click count: " + clickCount);

				var graphic = go.GetComponent<Graphic>();
				if (graphic != null)
					graphic.color = Color.green;

				if (clickCount == milestoneThreshold)
					MilestoneReached();
				else if (clickSound != null && audioSource != null)
					audioSource.PlayOneShot(clickSound);

				StartCoroutine(ShakeUI(go.transform));
				UpdateCounterText();
				break;
			}
		}
	}

	void UpdateCounterText()
	{
		if (counterText != null)
		{
			counterText.text = "Kölsch Flaschen: " + clickCount;
		}
	}

	void MilestoneReached()
	{
		if (milestoneSound != null && audioSource != null)
		{
			audioSource.PlayOneShot(milestoneSound);
			UnityEngine.Debug.Log("Milestone reached!");
		}

		if (milestoneObject != null)
		{
			milestoneObject.SetActive(true);
			UnityEngine.Debug.Log("Milestone object activated.");
		}
	}

	IEnumerator ShakeUI(Transform target)
	{
		float duration = 0.3f;
		float elapsed = 0f;
		float angle = 10f;

		Quaternion originalRotation = target.rotation;

		while (elapsed < duration)
		{
			float z = Mathf.Sin(elapsed * 40f) * angle;
			target.rotation = originalRotation * Quaternion.Euler(0f, 0f, z);
			elapsed += Time.deltaTime;
			yield return null;
		}

		target.rotation = originalRotation;
	}

	// Automatically switch orientation based on active GameObjects
	void HandleOrientation()
	{
		if (portraitTrigger != null && portraitTrigger.activeInHierarchy)
		{
			if (Screen.orientation != ScreenOrientation.Portrait)
			{
				Screen.orientation = ScreenOrientation.Portrait;
				UnityEngine.Debug.Log("Switched to Portrait");
			}
		}
		else if (landscapeTrigger != null && landscapeTrigger.activeInHierarchy)
		{
			if (Screen.orientation != ScreenOrientation.LandscapeLeft)
			{
				Screen.orientation = ScreenOrientation.LandscapeLeft;
				UnityEngine.Debug.Log("Switched to Landscape");
			}
		}
	}
}

public class ClickableState : MonoBehaviour { }
