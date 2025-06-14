using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

[RequireComponent(typeof(ARTrackedImageManager))]
public class QRCodeAnim : MonoBehaviour
{
    public GameObject prefabToSpawn;

    private ARTrackedImageManager trackedImageManager;
    private Dictionary<TrackableId, GameObject> spawned = new Dictionary<TrackableId, GameObject>();

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void Update()
    {
        foreach (var trackedImage in trackedImageManager.trackables)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                if (!spawned.ContainsKey(trackedImage.trackableId))
                {
                    GameObject go = Instantiate(prefabToSpawn, trackedImage.transform.position, trackedImage.transform.rotation);
                    spawned[trackedImage.trackableId] = go;

                    Animator animator = go.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.Play("Animation", 0, 0f);
                    }
                }
                else
                {
                    GameObject go = spawned[trackedImage.trackableId];
                    go.transform.SetPositionAndRotation(trackedImage.transform.position, trackedImage.transform.rotation);
                    go.SetActive(true);
                }
            }
            else
            {
                if (spawned.TryGetValue(trackedImage.trackableId, out GameObject go))
                {
                    go.SetActive(false);
                }
            }
        }
    }
}
