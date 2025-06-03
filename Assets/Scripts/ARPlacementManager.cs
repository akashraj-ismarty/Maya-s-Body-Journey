using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacementManager : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    public GameObject avatarPrefab;
    public GameObject placementIndicatorPrefab;
    private GameObject placementIndicator;
    private GameObject placedAvatar;
    public Camera arCamera; // Drag your AR Camera here.
    public MenstruationExplanation menstruationExplanation; // Drag your MenstruationExplanation script here.


    void Start()
    {
        // Ensure that the necessary AR components are present
        if (raycastManager == null || planeManager == null || avatarPrefab == null || placementIndicatorPrefab == null)
        {
            Debug.LogError("AR components not assigned in ARPlacementManager!");
            enabled = false; // Disable this script if components are missing
            return;
        }

        // Instantiate the placement indicator
        placementIndicator = Instantiate(placementIndicatorPrefab);
        placementIndicator.SetActive(false); // Hide it initially
    }

    void Update()
    {
        UpdatePlacementIndicator();

        if (isPlacementValid() && Input.GetMouseButtonDown(0))
        {
            PlaceAvatar();
        }
    }

    bool isPlacementValid()
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f); // Raycast from center

        raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon);

        return hits.Count > 0;
    }

    void UpdatePlacementIndicator()
    {
       List<ARRaycastHit> hits = new List<ARRaycastHit>();
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f); // Raycast from center

        raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon);

        if (hits.Count > 0)
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(hits[0].pose.position, hits[0].pose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    void PlaceAvatar()
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f); // Raycast from center

        raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon);

        if (hits.Count > 0)
        {
            if (placedAvatar == null)
            {
                placedAvatar = Instantiate(avatarPrefab, hits[0].pose.position, hits[0].pose.rotation);
                // After placing the avatar, pass a reference to the VideoController to the MenstruationExplation script
                if (menstruationExplanation != null)
                {
                    // menstruationExplanation.videoController = GameObject.FindObjectOfType<VideoController>();
                    // if(menstruationExplanation.videoController == null){
                    //    Debug.LogError("Video Controller not found");
                    // }
                }
            }
            else
            {
                placedAvatar.transform.position = hits[0].pose.position; //move the avatar.
            }
            planeManager.SetTrackablesActive(false); // Disable plane detection after placing
            placementIndicator.SetActive(false); // Hide the indicator
        }
    }

     public void RemoveAvatar()
    {
        if (placedAvatar != null)
        {
            Destroy(placedAvatar);
            placedAvatar = null;
            planeManager.SetTrackablesActive(true); // Re-enable plane detection.
        }
    }
}
