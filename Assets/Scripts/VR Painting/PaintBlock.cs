using UnityEngine;

/// <summary>
/// Marks a GameObject as a valid paint target.
/// Requires a Collider with "Paintable" tag and a PaintableSurface component.
/// </summary>
[RequireComponent(typeof(Collider), typeof(PaintableSurface))]
public class PaintBlock : MonoBehaviour
{
    void Awake()
    {
        gameObject.tag = "Paintable";
    }
}