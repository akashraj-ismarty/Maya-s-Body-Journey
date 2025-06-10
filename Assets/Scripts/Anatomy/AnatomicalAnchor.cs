using UnityEngine;

[RequireComponent(typeof(Collider))] // Ensures a collider is present for raycasting
public class AnatomicalAnchor : MonoBehaviour
{
    [Tooltip("The ScriptableObject containing data for this anatomical part.")]
    public AnchorData anchorData;

    void Start()
    {
        if (anchorData == null)
        {
            Debug.LogWarning($"AnatomicalAnchor on '{gameObject.name}' is missing its AnchorData ScriptableObject.", this);
        }

        // Optional: Set the layer of this anchor to a specific layer for targeted raycasting
        // gameObject.layer = LayerMask.NameToLayer("AnatomyAnchors");
    }
}
