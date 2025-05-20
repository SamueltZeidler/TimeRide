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
	[SerializeField] private AudioClip milestoneSound;   // Sound for milestone
	private AudioSource audioSource;

	[Header("Milestone")]
	[SerializeField] private int milestoneThreshold = 7; // Trigger value
	[SerializeField] private GameObject milestoneObject;  // Object to activate


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

	// Called when the milestone threshold is reached
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


	// UI shake animation by rotating on Z-axis
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
}

public class ClickableState : MonoBehaviour { }
