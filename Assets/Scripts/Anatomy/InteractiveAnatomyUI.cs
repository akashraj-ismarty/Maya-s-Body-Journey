using UnityEngine;
using TMPro; // For TextMeshPro

[RequireComponent(typeof(LineRenderer))]
public class InteractiveAnatomyUI : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("The AR Camera used for raycasting and billboarding.")]
    public Camera arCamera;
    [Tooltip("The layer mask to filter raycasts, should only hit AnatomicalAnchors.")]
    public LayerMask anchorLayerMask;
    [Tooltip("Prefab for the UI panel that displays information.")]
    public GameObject uiInfoPanelPrefab;

    [Header("Line Renderer Settings")]
    [Tooltip("Material for the line renderer.")]
    public Material lineMaterial;
    public float lineWidth = 0.005f;
    public Color lineColor = Color.yellow;

    [Header("UI Panel Settings")]
    [Tooltip("Offset of the UI panel from the anchor point, relative to the camera's up direction.")]
    public float uiPanelVerticalOffset = 0.1f;
    [Tooltip("Offset of the UI panel from the anchor point, relative to the camera's right direction.")]
    public float uiPanelHorizontalOffset = 0.05f;


    private LineRenderer lineRenderer;
    private GameObject currentUiPanelInstance;
    private TextMeshProUGUI partNameText;
    private TextMeshProUGUI descriptionText;

    private AnatomicalAnchor currentlyHoveredAnchor;
    private Transform activeModelRoot; // The root of the placed 3D model (e.g., placedUterus)

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();

        if (arCamera == null)
        {
            arCamera = Camera.main; // Fallback to main camera
            if (arCamera == null)
            {
                Debug.LogError("InteractiveAnatomyUI: AR Camera not assigned and Camera.main is null!", this);
                enabled = false;
                return;
            }
        }

        if (uiInfoPanelPrefab == null)
        {
            Debug.LogError("InteractiveAnatomyUI: UI Info Panel Prefab not assigned!", this);
            enabled = false;
            return;
        }
        if (anchorLayerMask == 0) // LayerMask not set
        {
            Debug.LogWarning("InteractiveAnatomyUI: Anchor Layer Mask is not set. Raycasting might not work as expected.", this);
            // Consider setting a default if you have a common layer name:
            // anchorLayerMask = LayerMask.GetMask("AnatomyAnchors");
        }
    }

    void SetupLineRenderer()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = lineMaterial; // Assign a suitable material (e.g., a simple unlit material)
        if (lineMaterial == null)
        {
            // Create a basic material if none is assigned
            lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        }
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy || activeModelRoot == null || !arCamera.enabled)
        {
            HideInteraction();
            return;
        }

        Ray ray = arCamera.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
        RaycastHit hit;

        AnatomicalAnchor hitAnchor = null;

        if (Physics.Raycast(ray, out hit, 10f, anchorLayerMask)) // Max distance 10f, adjust as needed
        {
            // Ensure the hit object is a child of our activeModelRoot
            if (hit.collider.transform.IsChildOf(activeModelRoot))
            {
                hitAnchor = hit.collider.GetComponent<AnatomicalAnchor>();
            }
        }

        if (hitAnchor != null && hitAnchor.anchorData != null)
        {
            if (currentlyHoveredAnchor != hitAnchor)
            {
                currentlyHoveredAnchor = hitAnchor;
                ShowInteraction(hitAnchor);
            }
            UpdateInteractionElements(hitAnchor);
        }
        else
        {
            if (currentlyHoveredAnchor != null)
            {
                HideInteraction();
            }
        }
    }

    void ShowInteraction(AnatomicalAnchor anchor)
    {
        if (currentUiPanelInstance == null)
        {
            currentUiPanelInstance = Instantiate(uiInfoPanelPrefab); // Instantiate as child to keep scene clean
            // Find TextMeshPro components within the instantiated panel
            partNameText = currentUiPanelInstance.transform.Find("PartNameText")?.GetComponent<TextMeshProUGUI>(); // Adjust names if different
            descriptionText = currentUiPanelInstance.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();

            if (partNameText == null || descriptionText == null)
            {
                Debug.LogError("InteractiveAnatomyUI: Could not find PartNameText or DescriptionText on the UI Panel Prefab instance.", currentUiPanelInstance);
                Destroy(currentUiPanelInstance);
                currentUiPanelInstance = null;
                enabled = false;
                return;
            }
        }

        partNameText.text = anchor.anchorData.partName;
        descriptionText.text = anchor.anchorData.description;

        currentUiPanelInstance.SetActive(true);
        lineRenderer.enabled = true;
    }

    void UpdateInteractionElements(AnatomicalAnchor anchor)
    {
        if (currentUiPanelInstance == null || anchor == null) return;

        // --- UI Panel Positioning and Billboarding ---
        Vector3 anchorPosition = anchor.transform.position;
        Vector3 panelPosition = anchorPosition +
                                (arCamera.transform.up * uiPanelVerticalOffset) +
                                (arCamera.transform.right * uiPanelHorizontalOffset);
        currentUiPanelInstance.transform.position = panelPosition;

        // Billboard: Make UI face the camera
        currentUiPanelInstance.transform.LookAt(
            currentUiPanelInstance.transform.position + arCamera.transform.rotation * Vector3.forward,
            arCamera.transform.rotation * Vector3.up
        );

        // --- Line Renderer Positioning ---
        lineRenderer.SetPosition(0, anchor.transform.position);
        lineRenderer.SetPosition(1, currentUiPanelInstance.transform.position); // Or a specific point on the panel
    }

    void HideInteraction()
    {
        if (currentlyHoveredAnchor != null)
        {
            currentlyHoveredAnchor = null;
        }
        if (currentUiPanelInstance != null && currentUiPanelInstance.activeSelf)
        {
            currentUiPanelInstance.SetActive(false);
        }
        if (lineRenderer != null && lineRenderer.enabled)
        {
            lineRenderer.enabled = false;
        }
    }

    /// <summary>
    /// Activates the interactive UI system for a given model.
    /// </summary>
    /// <param name="modelRoot">The root transform of the 3D model to interact with.</param>
    public void ActivateSystem(Transform modelRoot)
    {
        activeModelRoot = modelRoot;
        this.enabled = true; // Enable the Update loop
        Debug.Log("InteractiveAnatomyUI Activated for model: " + (modelRoot != null ? modelRoot.name : "null"));
        // Ensure line renderer and UI are initially hidden if no immediate hover
        HideInteraction();
    }

    /// <summary>
    /// Deactivates the interactive UI system.
    /// </summary>
    public void DeactivateSystem()
    {
        activeModelRoot = null;
        HideInteraction();
        this.enabled = false; // Disable the Update loop
        Debug.Log("InteractiveAnatomyUI Deactivated");
    }

    void OnDisable()
    {
        // Ensure everything is hidden when the script is disabled externally
        HideInteraction();
    }
}
