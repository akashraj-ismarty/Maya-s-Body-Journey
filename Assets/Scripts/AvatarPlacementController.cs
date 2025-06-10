using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public enum PlacementState
{
    ARPlaneDetection,
    AvatarPlaced
}

public class AvatarPlacementController : MonoBehaviour
{
    #region Public Fields & properities
    [Header("Object Prefabs")]
    public GameObject avatarPrefab; // Assign your 3D avatar prefab here
    public GameObject uterusPrefab; // Assign your uterus prefab here
    public GameObject placementIndicatorPrefab;

    [Header("Placement Settings")]
    public float offsetFromAvatar = 0.5f; // Adjust this value to control distance from avatar

    [Header("AR Components")]
    public ARRaycastManager arRaycastManager;
    public ARPlaneManager arPlaneManager;

    [Header("UI Systems")]
    public GameObject uiSystem;
    public InteractiveAnatomyUI interactiveAnatomySystem;
    public GameObject menstruationController;
    [SerializeField] private GameObject _placementPrompt;

    /// <summary>
    /// Animator component for the placed uterus.
    /// </summary>
    public Animator PlacedUterusAnimator { get; private set; }
    #endregion

    #region Private Fields
    private PlacementState _currentState;// = new PlacementState();
    private readonly List<ARRaycastHit> _hits = new List<ARRaycastHit>();
    private GameObject _placedAvatar;
    private GameObject _placedUterus; // To keep track of the placed uterus
    private GameObject _placementIndicatorInstance;
    #endregion

    #region Unity Lifecycle Methods
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
        _placementIndicatorInstance = Instantiate(placementIndicatorPrefab);
        _placementIndicatorInstance.SetActive(false);

        if (menstruationController == null)
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
        switch (_currentState)
        {
            case PlacementState.ARPlaneDetection:
                UpdateARPlaneDetection();
                break;
            case PlacementState.AvatarPlaced:
                UpdateAvatarPlaced();
                break;
        }
    }
    #endregion

    #region State Management
    private void EnterARPlaneDetectionState()
    {
        _currentState = PlacementState.ARPlaneDetection;
        Debug.Log("Entering ARPlaneDetection State");
        arRaycastManager.enabled = true;
        if (arPlaneManager != null)
        {
            arPlaneManager.enabled = true;
            SetPlaneTrackablesActive(true);
        }
        _placementIndicatorInstance.SetActive(true);
        _placementPrompt.SetActive(true);



        if (menstruationController != null)
        {
            menstruationController.SetActive(false);
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
        _currentState = PlacementState.AvatarPlaced;
        Debug.Log("Entering AvatarPlaced State");

        arRaycastManager.enabled = false;
        if (arPlaneManager != null)
        {
            SetPlaneTrackablesActive(false); // hide exiting planes
            arPlaneManager.enabled = false; //stop detecting new planes
        }
        _placementIndicatorInstance.SetActive(false);
        _placementPrompt.SetActive(false);


        if (menstruationController != null)
        {
            menstruationController.SetActive(true);
            uiSystem.SetActive(true);
        }

        if (interactiveAnatomySystem != null && _placedUterus != null)
        {
            interactiveAnatomySystem.ActivateSystem(_placedUterus.transform);
        }
    }
    #endregion

    #region Core Logic
    private void UpdateARPlaneDetection()
    {
        UpdatePlacementIndicatorVisuals();

        if (_placementIndicatorInstance.activeSelf && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Pose placementPose = new Pose(_placementIndicatorInstance.transform.position, _placementIndicatorInstance.transform.rotation);
            PlaceObjects(placementPose);
            EnterAvatarPlacedState();
        }
    }

    private void UpdatePlacementIndicatorVisuals()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        if (arRaycastManager.Raycast(screenCenter, _hits, TrackableType.PlaneWithinPolygon))
        {
            _placementIndicatorInstance.SetActive(true);
            _placementIndicatorInstance.transform.SetPositionAndRotation(_hits[0].pose.position, _hits[0].pose.rotation);
        }
        else
        {
            _placementIndicatorInstance.SetActive(false);
        }
    }
    private void PlaceObjects(Pose placementPose)
    {
        DestroyPlacedObjects(); // Destroy previous objects if any

        // 1. Instantiate the Avatar
        _placedAvatar = Instantiate(avatarPrefab, placementPose.position, Quaternion.identity); // Use placement pose rotation or identity

        // 2. Calculate the position for the Uterus relative to the avatar
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0; // Ignore vertical component
        cameraForward.Normalize();
        Vector3 rightDirection = Quaternion.Euler(0, 90, 0) * cameraForward;
        Vector3 uterusPosition = _placedAvatar.transform.position + (rightDirection * offsetFromAvatar);

        // 3. Instantiate the Uterus
        _placedUterus = Instantiate(uterusPrefab, uterusPosition, Quaternion.identity);

        // 4. Trigger Uterus Animation (Optional)
        PlacedUterusAnimator = _placedUterus.GetComponent<Animator>();
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
            if (_placedAvatar != null)
            {
                Vector3 avatarPosition = _placedAvatar.transform.position;
                Vector3 lookDirectionAvatar = cameraPosition - avatarPosition;
                lookDirectionAvatar.y = 0;
                if (lookDirectionAvatar != Vector3.zero)
                {
                    _placedAvatar.transform.rotation = Quaternion.LookRotation(lookDirectionAvatar);
                }
            }

            if (_placedUterus != null)
            {
                Vector3 uterusPosition = _placedUterus.transform.position;
                Vector3 lookDirectionUterus = cameraPosition - uterusPosition;
                lookDirectionUterus.y = 0;
                if (lookDirectionUterus != Vector3.zero)
                {
                    Quaternion baseRotation = Quaternion.LookRotation(lookDirectionUterus);
                    Quaternion correctiveRotation = Quaternion.Euler(0, -120f, 0); // Keep your corrective rotation
                    _placedUterus.transform.rotation = baseRotation * correctiveRotation;
                }
            }
        }
    }
    #endregion

    #region Helper Methods
    private void DestroyPlacedObjects()
    {
        if (_placedAvatar != null)
        {
            Destroy(_placedAvatar);
            _placedAvatar = null;
        }
        if (_placedUterus != null)
        {
            Destroy(_placedUterus);
            _placedUterus = null;
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
    #endregion

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