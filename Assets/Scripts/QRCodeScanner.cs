using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZXing;
using ZXing.QrCode;
using Debug = UnityEngine.Debug;

public class QRCodeScanner : MonoBehaviour
{
	[Header("Objekt mit Animator")]
	public GameObject animatedObject;

	[Header("Animator Bool Parametername")]
	public string animatorBoolParameter = "check";

	private WebCamTexture webcamTexture;
	private string lastScannedText = "";
	private bool isLocked = false;

	void Start()
	{
		if (animatedObject == null)
		{
			Debug.LogError("❌ Kein AnimatedObject zugewiesen!");
			return;
		}

		StartCoroutine(RequestCameraPermissionAndStart());
	}

	IEnumerator RequestCameraPermissionAndStart()
	{
#if UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
            while (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
            {
                yield return null;
            }
        }
#endif
		StartCoroutine(StartCameraWithDelay());
	}

	IEnumerator StartCameraWithDelay()
	{
		Debug.Log("⏳ Warte 1 Sekunde vor dem Kamera-Start...");
		yield return new WaitForSeconds(1f);
		StartCamera();
	}

	void StartCamera()
	{
		WebCamDevice[] devices = WebCamTexture.devices;

		foreach (var device in devices)
		{
			if (!device.isFrontFacing)
			{
				webcamTexture = new WebCamTexture(device.name, 320, 240);
				break;
			}
		}

		if (webcamTexture == null && devices.Length > 0)
		{
			webcamTexture = new WebCamTexture(devices[0].name, 320, 240);
		}

		if (webcamTexture != null)
		{
			webcamTexture.Play();
			Debug.Log("📷 Kamera gestartet.");
			StartCoroutine(WaitForCameraReady());
		}
		else
		{
			Debug.LogError("❌ Keine Kamera verfügbar.");
		}
	}

	IEnumerator WaitForCameraReady()
	{
		Debug.Log("⏳ Warten auf gültiges Kamerabild ...");

		float timeout = 5f;
		float timer = 0f;

		while ((webcamTexture.width < 100 || webcamTexture.height < 100) && timer < timeout)
		{
			yield return new WaitForSeconds(0.1f);
			timer += 0.1f;
		}

		if (webcamTexture.width >= 100)
		{
			Debug.Log($"✅ Kamera bereit: {webcamTexture.width}x{webcamTexture.height}");
			StartCoroutine(ScanLoop());
		}
		else
		{
			Debug.LogError("❌ Kamera liefert keine gültige Auflösung.");
		}
	}

	IEnumerator ScanLoop()
	{
		QRCodeReader reader = new QRCodeReader();

		while (true)
		{
			yield return new WaitForSeconds(0.5f);

			if (isLocked || webcamTexture == null || !webcamTexture.isPlaying)
				continue;

			int width = webcamTexture.width;
			int height = webcamTexture.height;

			if (width < 100 || height < 100)
			{
				Debug.Log("⏳ Kamera noch nicht bereit. Größe: " + width + "x" + height);
				continue;
			}

			Color32[] pixels = null;
			try
			{
				pixels = webcamTexture.GetPixels32();
			}
			catch (System.Exception ex)
			{
				Debug.LogWarning("❌ GetPixels32 fehlgeschlagen: " + ex.Message);
				continue;
			}

			if (pixels == null || pixels.Length == 0)
			{
				Debug.LogWarning("❌ Leere Pixel-Daten, überspringe Frame");
				continue;
			}

			try
			{
				var source = new ZXing.Color32LuminanceSource(pixels, width, height);
				var binarizer = new ZXing.Common.HybridBinarizer(source);
				var bitmap = new ZXing.BinaryBitmap(binarizer);
				var result = reader.decode(bitmap);

				if (result != null && result.Text != lastScannedText)
				{
					Debug.Log("✅ QR erkannt: " + result.Text);
					lastScannedText = result.Text;
					PlayAnimation();
				}
				else
				{
					Debug.Log("⚠️ Kein QR erkannt in diesem Frame.");
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogWarning("❌ QR-Scan fehlgeschlagen: " + ex.Message);
			}
		}
	}

	void PlayAnimation()
	{
		Animator animator = animatedObject.GetComponent<Animator>();
		if (animator == null || animator.runtimeAnimatorController == null)
		{
			Debug.LogError("❌ Kein gültiger Animator oder Controller!");
			return;
		}

		animator.SetBool(animatorBoolParameter, true);
		isLocked = true;
		StartCoroutine(ResetAfterDelay(animator, 3.5f));
	}

	IEnumerator ResetAfterDelay(Animator animator, float delay)
	{
		yield return new WaitForSeconds(delay);

		if (animator != null && animator.runtimeAnimatorController != null)
		{
			animator.SetBool(animatorBoolParameter, false);
		}

		isLocked = false;
		lastScannedText = "";
		Debug.Log("🔄 Animation zurückgesetzt, Scanner wieder bereit");
	}
}
