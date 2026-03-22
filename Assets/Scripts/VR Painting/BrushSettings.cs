using UnityEngine;

[CreateAssetMenu(fileName = "BrushSettings", menuName = "VR Painting/Brush Settings")]
public class BrushSettings : ScriptableObject
{
    [Range(0.01f, 0.2f)] public float brushSize = 0.05f;
    [Range(0f, 1f)]       public float opacity  = 1f;
    public Color          brushColor            = Color.red;
    public Texture2D      brushShape;           // optional soft-edge stamp
}