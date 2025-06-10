using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System;

public enum PlacementState
{
    ARPlaneDetection,
    AvatarPlaced
}

public class AvatarPlacementController : MonoBehaviour
{
    public GameObject avatarPrefab; // Assign your 3D avatar prefab here
    public GameObject uterusPrefab; // Assign your uterus prefab here
    public float offsetFromAvatar = 0.5f; // Adjust this value to control distance from avatar

    public ARRaycastManager arRaycastManager;
    public ARPlaneManager arPlaneManager;
    public GameObject placementIndicatorPrefab;
    public GameObject uiSystem;
    public InteractiveAnatomyUI interactiveAnatomySystem;
    public GameObject uiSystemGameObject;


    private PlacementState currentState;// = new PlacementState();
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject placedAvatar;
    private GameObject placedUterus; // To keep track of the placed uterus
    private GameObject placementIndicator;
    public GameObject placementPrompt;

    public Animator PlacedUterusAnimator { get; private set; }

    void Start()
    {
        if (arRaycastManager == null)
        {
            arRaycastManager = FindObjectOfType<ARRaycastManager>();
            if (arRaycastManager == null)
            {
                Debug.LogError("ARRaycastManager not found! Please add it to your AR Session.");
            }
        }
        if (arPlaneManager == null)
        {
            arPlaneManager = FindObjectOfType<ARPlaneManager>();
            if (arPlaneManager == null)
            {
                Debug.LogError("ARPlaneManager not found! Please add it to your AR Session.");
            }
        }
        if (placementIndicatorPrefab == null)
        {
            Debug.LogError("PlacementIndicatorPrefab not found!");
            enabled = false;
            return;
        }
        placementIndicator = Instantiate(placementIndicatorPrefab);
        placementIndicator.SetActive(false);

        if (uiSystemGameObject == null)
        {
            Debug.LogError("UI System GameObject not found");

        }

        if (interactiveAnatomySystem == null)
        {
            interactiveAnatomySystem = FindObjectOfType<InteractiveAnatomyUI>();
            if (interactiveAnatomySystem == null)
            {
                Debug.LogWarning("InteractiveAnatomySystem not found. Interactive highlighting will not work.");
            }
        }
        EnterARPlaneDetectionState();
            
    }


    void Update()
    {
        switch (currentState)
        {
            case PlacementState.ARPlaneDetection:
                UpdateARPlaneDetection();
                break;
            case PlacementState.AvatarPlaced:
                UpdateAvatarPlaced();
                break;
        }
    }


    private void EnterARPlaneDetectionState()
    {
        currentState = PlacementState.ARPlaneDetection;
        Debug.Log("Entering ARPlaneDetection State");
        arRaycastManager.enabled = true;
        if (arPlaneManager != null)
        {
            arPlaneManager.enabled = true;
            SetPlaneTrackablesActive(true);
        }
        placementIndicator.SetActive(true);
        placementPrompt.SetActive(true);



        if (uiSystemGameObject != null)
        {
            uiSystemGameObject.SetActive(false);
            uiSystem.SetActive(false);
        }
        
        if (interactiveAnatomySystem != null)
        {
            interactiveAnatomySystem.DeactivateSystem();
        }

        DestroyPlacedObjects();

    }



    void EnterAvatarPlacedState()
    {
        currentState = PlacementState.AvatarPlaced;
        Debug.Log("Entering AvatarPlaced State");

        arRaycastManager.enabled = false;
        if (arPlaneManager != null)
        {
            SetPlaneTrackablesActive(false); // hide exiting planes
            arPlaneManager.enabled = false; //stop detecting new planes
        }
        placementIndicator.SetActive(false);
        placementPrompt.SetActive(false);


        if (uiSystemGameObject != null)
        {
            uiSystemGameObject.SetActive(true);
            uiSystem.SetActive(true);
        }
        
        if (interactiveAnatomySystem != null && placedUterus != null)
        {
            interactiveAnatomySystem.ActivateSystem(placedUterus.transform);
        }
    }

    private void UpdateARPlaneDetection()
    {
        // Check if the user is looking at a plane
        UpdatePlacementIndicatorVisuals();

        if (placementIndicator.activeSelf && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Pose placementPose = new Pose(placementIndicator.transform.position, placementIndicator.transform.rotation);
            PlaceObjects(placementPose);
            EnterAvatarPlacedState();
        }
    }

    private void UpdatePlacementIndicatorVisuals()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        if (arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(hits[0].pose.position, hits[0].pose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }
    private void PlaceObjects(Pose placementPose)
    {
        DestroyPlacedObjects(); // Destroy previous objects if any

        // 1. Instantiate the Avatar
        placedAvatar = Instantiate(avatarPrefab, placementPose.position, Quaternion.identity); // Use placement pose rotation or identity

        // 2. Calculate the position for the Uterus relative to the avatar
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0; // Ignore vertical component
        cameraForward.Normalize();
        Vector3 rightDirection = Quaternion.Euler(0, 90, 0) * cameraForward;
        Vector3 uterusPosition = placedAvatar.transform.position + (rightDirection * offsetFromAvatar);

        // 3. Instantiate the Uterus
        placedUterus = Instantiate(uterusPrefab, uterusPosition, Quaternion.identity);

        // 4. Trigger Uterus Animation (Optional)
        PlacedUterusAnimator = placedUterus.GetComponent<Animator>();
        if (PlacedUterusAnimator != null)
        {
            // Assuming you have an animation trigger named "PlayAnimation"
            // uterusAnimator.SetTrigger("PlayAnimation"); // Uncomment and set your trigger
            Debug.Log("Uterus animation would be triggered here if set up.");
        }
        else
        {
            Debug.LogWarning("Uterus Prefab does not have an Animator component.");
        }

        Debug.Log("Avatar and Uterus placed at: " + placementPose.position);
    }

    private void UpdateAvatarPlaced()
    {
        // --- Make the avatar and uterus always face the camera ---
        if (Camera.main != null)
        {
            Vector3 cameraPosition = Camera.main.transform.position;
            if (placedAvatar != null)
            {
                Vector3 avatarPosition = placedAvatar.transform.position;
                Vector3 lookDirectionAvatar = cameraPosition - avatarPosition;
                lookDirectionAvatar.y = 0;
                if (lookDirectionAvatar != Vector3.zero)
                {
                    placedAvatar.transform.rotation = Quaternion.LookRotation(lookDirectionAvatar);
                }
            }

            if (placedUterus != null)
            {
                Vector3 uterusPosition = placedUterus.transform.position;
                Vector3 lookDirectionUterus = cameraPosition - uterusPosition;
                lookDirectionUterus.y = 0;
                if (lookDirectionUterus != Vector3.zero)
                {
                    Quaternion baseRotation = Quaternion.LookRotation(lookDirectionUterus);
                    Quaternion correctiveRotation = Quaternion.Euler(0, -120f, 0); // Keep your corrective rotation
                    placedUterus.transform.rotation = baseRotation * correctiveRotation;
                }
            }
        }
    }
    private void DestroyPlacedObjects()
    {
        if (placedAvatar != null)
        {
            Destroy(placedAvatar);
            placedAvatar = null;
        }
        if (placedUterus != null)
        {
            Destroy(placedUterus);
            placedUterus = null;
        }
        PlacedUterusAnimator = null;
    }

    private void SetPlaneTrackablesActive(bool isActive)
    {
        if (arPlaneManager != null)
        {
            foreach (var plane in arPlaneManager.trackables)
            {
                plane.gameObject.SetActive(isActive);
            }
        }
    }

    /// <summary>
    /// Public method to reset the AR experience.
    /// Call this from a UI button or other game logic.
    /// </summary>
    public void ResetExperience()
    {
        Debug.Log("Resetting Experience");
        DestroyPlacedObjects();
        EnterARPlaneDetectionState();
    }
}