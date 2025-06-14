using UnityEngine;
using ZXing;
using System.Collections;

public class QRCodeScanner : MonoBehaviour
{
	[HideInInspector]
	public string lastScannedText = "";

	private WebCamTexture webcamTexture;
	private IBarcodeReader barcodeReader = new BarcodeReader();

	void Start()
	{
		StartCoroutine(RequestCameraPermissionAndStart());
	}

	IEnumerator RequestCameraPermissionAndStart()
	{
#if UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
            yield return new WaitForSeconds(1f); // kleine Pause
        }
#endif
		StartCamera();
		StartCoroutine(ScanQRCode());
	}

	void StartCamera()
	{
		// H�here Aufl�sung f�r bessere Erkennung
		webcamTexture = new WebCamTexture(1280, 720);
		if (WebCamTexture.devices.Length > 0)
		{
			webcamTexture.deviceName = WebCamTexture.devices[0].name;
		}
		webcamTexture.Play();
		UnityEngine.Debug.Log("Kamera gestartet mit " + webcamTexture.width + "x" + webcamTexture.height);
	}

	IEnumerator ScanQRCode()
	{
		while (true)
		{
			if (webcamTexture.width > 100 && webcamTexture.height > 100)
			{
				try
				{
					Color32[] pixels = webcamTexture.GetPixels32();
					int width = webcamTexture.width;
					int height = webcamTexture.height;

					UnityEngine.Debug.Log($"Bildgr��e: {width}x{height}");

					var result = barcodeReader.Decode(pixels, width, height);

					if (result != null)
					{
						UnityEngine.Debug.Log("Decode erfolgreich: " + result.Text);

						if (result.Text != lastScannedText)
						{
							lastScannedText = result.Text;
							//string urlToOpen = lastScannedText;
							//if (!urlToOpen.StartsWith("http"))
							//{
								//urlToOpen = "https://" + urlToOpen;
							//}

							//StartCoroutine(OpenURLWithDelay(urlToOpen));
						}
					}
					else
					{
						UnityEngine.Debug.Log("Kein QR-Code erkannt.");
					}
				}
				catch (System.Exception ex)
				{
					UnityEngine.Debug.LogWarning("Scan-Fehler: " + ex.Message);
				}
			}

			yield return new WaitForSeconds(0.5f);
		}
	}

	IEnumerator OpenURLWithDelay(string url)
	{
		UnityEngine.Debug.Log("�ffne URL in 0.5 Sekunden: " + url);
		yield return new WaitForSeconds(0.5f);
		UnityEngine.Application.OpenURL(url);
	}

	void OnDestroy()
	{
		if (webcamTexture != null && webcamTexture.isPlaying)
		{
			webcamTexture.Stop();
		}
	}
}
