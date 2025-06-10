using UnityEngine;

[CreateAssetMenu(fileName = "New AnchorData", menuName = "Anatomy/Anchor Data")]
public class AnchorData : ScriptableObject
{
    [Tooltip("The name of the anatomical part.")]
    public string partName = "Part Name";

    [TextArea(3, 10)]
    [Tooltip("A detailed description of the anatomical part.")]
    public string description = "Detailed description of the part goes here.";

    // You could add more fields here, like an icon, specific colors, etc.
    // public Sprite partIcon;
}