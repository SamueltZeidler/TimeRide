using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ZXing;
using System.Collections;

public class QRCodeScanner : MonoBehaviour
{
	public RawImage cameraView;
	public TextMeshProUGUI resultText;

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

		cameraView.texture = webcamTexture;

		// duplicate the material to avoid modifying the original
		cameraView.material = new Material(cameraView.material);
		cameraView.material.mainTexture = webcamTexture;

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
						Debug.Log("QR-Code gefunden: " + result.Text);

						resultText.text = $"<link={lastScannedText}><u><color=blue>{lastScannedText}</color></u></link>";
					}
				}
				catch (System.Exception ex)
				{
					Debug.LogWarning("Scan-Fehler: " + ex.Message);
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

	// Klick auf Link abfangen
	public void OnLinkClicked()
	{
		if (!string.IsNullOrEmpty(lastScannedText))
		{
			string urlToOpen = lastScannedText;

			if (!urlToOpen.StartsWith("http"))
			{
				urlToOpen = "https://" + urlToOpen;
			}

			Debug.Log("Öffne URL: " + urlToOpen);
			Application.OpenURL(urlToOpen);
		}
	}

}
