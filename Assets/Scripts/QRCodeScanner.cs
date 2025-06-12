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
		StartCamera();
		StartCoroutine(ScanQRCode());
	}

	void StartCamera()
	{
		webcamTexture = new WebCamTexture();
		webcamTexture.Play();
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

					var result = barcodeReader.Decode(pixels, width, height);
					if (result != null && result.Text != lastScannedText)
					{
						lastScannedText = result.Text;
						UnityEngine.Debug.Log("QR-Code gefunden: " + result.Text);

						string urlToOpen = lastScannedText;
						if (!urlToOpen.StartsWith("http"))
						{
							urlToOpen = "https://" + urlToOpen;
						}

						UnityEngine.Application.OpenURL(urlToOpen);
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

	void OnDestroy()
	{
		if (webcamTexture != null && webcamTexture.isPlaying)
		{
			webcamTexture.Stop();
		}
	}
}
