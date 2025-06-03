using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class AvatarPlacementController : MonoBehaviour
{
    public GameObject avatarPrefab; // Assign your 3D avatar prefab here
    public GameObject uterusPrefab; // Assign your uterus prefab here
    public float offsetFromAvatar = 0.5f; // Adjust this value to control distance from avatar
    public ARRaycastManager arRaycastManager;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject placedAvatar;
    private GameObject placedUterus; // To keep track of the placed uterus

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
    }

    void Update()
    {
        // Only proceed if there's a touch input for initial placement
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Ended)
            {
                if (arRaycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;

                    // Destroy previous objects if only one set is desired
                    if (placedAvatar != null)
                    {
                        Destroy(placedAvatar);
                    }
                    if (placedUterus != null)
                    {
                        Destroy(placedUterus);
                    }

                    // 1. Instantiate the Avatar
                    placedAvatar = Instantiate(avatarPrefab, hitPose.position, Quaternion.identity);

                    // 2. Calculate the position for the Uterus relative to the avatar
                    // Get the camera's forward direction on the horizontal plane
                    Vector3 cameraForward = Camera.main.transform.forward;
                    cameraForward.y = 0; // Ignore vertical component
                    cameraForward.Normalize();

                    // Calculate the right vector relative to the camera's horizontal forward
                    // This will place the uterus to the right of the avatar from the camera's perspective
                    Vector3 rightDirection = Quaternion.Euler(0, 90, 0) * cameraForward;

                    // Uterus position: avatar's position + (right direction * offset)
                    Vector3 uterusPosition = placedAvatar.transform.position + (rightDirection * offsetFromAvatar);

                    // 3. Instantiate the Uterus
                    placedUterus = Instantiate(uterusPrefab, uterusPosition, Quaternion.identity);

                    // 4. Trigger Uterus Animation
                    Animator uterusAnimator = placedUterus.GetComponent<Animator>();
                    if (uterusAnimator != null)
                    {
                        // Assuming you have an animation trigger named "PlayAnimation"
                        // Change "PlayAnimation" to the actual name of your Animator Trigger parameter
                        Debug.Log("Uterus animation triggered!");
                    }
                    else
                    {
                        Debug.LogWarning("Uterus Prefab does not have an Animator component.");
                    }

                    Debug.Log("Avatar and Uterus placed at: " + hitPose.position);
                }
            }
        }

        // --- Make the avatar always face the camera ---
        if (placedAvatar != null && Camera.main != null)
        {
            Vector3 cameraPosition = Camera.main.transform.position;
            Vector3 avatarPosition = placedAvatar.transform.position;

            Vector3 lookDirection = cameraPosition - avatarPosition;
            lookDirection.y = 0; // Crucial: Restrict rotation to the Y-axis

            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                placedAvatar.transform.rotation = targetRotation;

                // Make the uterus face the camera as well, or keep its initial rotation if desired
                // For simplicity, let's also make it face the camera horizontally
                if (placedUterus != null)
                {
                    Vector3 uterusLookDirection = cameraPosition - placedUterus.transform.position;
                    uterusLookDirection.y = 0;
                    if (uterusLookDirection != Vector3.zero)
                    {
                        // Calculate the base rotation to make the GameObject's Z-axis face the camera
                        Quaternion baseRotation = Quaternion.LookRotation(uterusLookDirection);

                        // Apply a corrective rotation. If the model's "front" is its local +X axis,
                        // a +90 degree rotation around Y will align it.
                        Quaternion correctiveRotation = Quaternion.Euler(0, -120f, 0);
                        placedUterus.transform.rotation = baseRotation * correctiveRotation;                    }
                }
            }
        }
    }
}