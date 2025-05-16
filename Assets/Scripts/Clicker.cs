using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class Clicker : MonoBehaviour
{
	[SerializeField] private GraphicRaycaster uiRaycaster;
	[SerializeField] private EventSystem eventSystem;
	[SerializeField] private TextMeshProUGUI counterText;

	private int clickCount = 0;

	void Start()
	{
		if (uiRaycaster == null)
			uiRaycaster = FindObjectOfType<GraphicRaycaster>();

		if (eventSystem == null)
			eventSystem = FindObjectOfType<EventSystem>();

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

				UpdateCounterText();
				break;
			}
		}
	}

	void UpdateCounterText()
	{
		if (counterText != null)
		{
			counterText.text = "Clicks: " + clickCount;
		}
	}
}

public class ClickableState : MonoBehaviour { }
