using UnityEngine;
using ZXing;
using System.Collections;
using System.Collections.Generic;


public class QRCodeScanner : MonoBehaviour
{
	[HideInInspector]
	public string lastScannedText = "";


	private WebCamTexture webcamTexture;

	private IBarcodeReader barcodeReader = new BarcodeReader
	{
		AutoRotate = false,
		TryInverted = true,
		Options = new ZXing.Common.DecodingOptions
		{
			PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE },
			TryHarder = false
		}
	};

	public GameObject animatedObject;
	bool alreadyPlayed = false;

	void Start()
	{
		if (animatedObject == null)
			Debug.LogWarning("🚨 animatedObject ist beim Start NULL!");
		else
			Debug.Log("✅ animatedObject gefunden: " + animatedObject.name);
		StartCoroutine(RequestCameraPermissionAndStart());
	}

	void Awake()
	{
		if (animatedObject == null)
		{
			animatedObject = GameObject.Find("TimeRideAnim");
			if (animatedObject != null)
				Debug.Log("AnimatedObject per Code gefunden: " + animatedObject.name);
		}
	}

	IEnumerator RequestCameraPermissionAndStart()
	{
#if UNITY_ANDROID
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
            yield return new WaitForSeconds(1f); 
        }
#endif
		StartCamera();
		StartCoroutine(ScanQRCode());
	}

	void StartCamera()
	{

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

					//UnityEngine.Debug.Log($"Bildgr��e: {width}x{height}");

					var result = barcodeReader.Decode(pixels, width, height);

					if (result != null)
					{
						UnityEngine.Debug.Log("Decode erfolgreich: " + result.Text);

						if (result != null && result.BarcodeFormat == BarcodeFormat.QR_CODE)
						{

							bool isNewCode = result.Text != lastScannedText;


							if (isNewCode || !alreadyPlayed)
							{
								lastScannedText = result.Text;
								PlayAnimation();
								alreadyPlayed = true;
							}

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
						//UnityEngine.Debug.Log("Kein QR-Code erkannt.");
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

	void PlayAnimation()
	{
		if (animatedObject == null)
		{
			Debug.LogWarning("animatedObject ist null!");
			return;
		}

		Animator animator = animatedObject.GetComponent<Animator>();
		if (animator == null)
		{
			Debug.LogWarning("Animator fehlt!");
			return;
		}


		foreach (var p in animator.parameters)
			Debug.Log($"Animator-Parameter: {p.name}  ({p.type})");

		animator.SetTrigger("Play");
		Debug.Log("✅ SetTrigger(\"Play\") ausgeführt");
		alreadyPlayed = true;
		StartCoroutine(UnlockAfter(animator));





	}
	IEnumerator UnlockAfter(Animator animator)
{
    Debug.Log("⏳ UnlockAfter gestartet");

    const float clipLength = 3.5f;   //  ► Länge deines Clips + Reserve
    yield return new WaitForSeconds(clipLength);

    alreadyPlayed   = false;
    lastScannedText = "";
    Debug.Log("🔓 Scanner entsperrt (fixed wait)");
   
}







	//IEnumerator OpenURLWithDelay(string url)
	//{
	//UnityEngine.Debug.Log("�ffne URL in 0.5 Sekunden: " + url);
	//yield return new WaitForSeconds(0.5f);
	//UnityEngine.Application.OpenURL(url);
	//}

	//void OnDestroy()
	//{
	//	if (webcamTexture != null && webcamTexture.isPlaying)
	//	{
	//	webcamTexture.Stop();
	//	}
	//	}
}
