using UnityEngine;
using TMPro;

public class ColorSwitcher : MonoBehaviour
{
    public BrushSettings brush;
    public TextMeshPro colorLabel; // optional, 3D text showing current color

    // Green=low, Yellow=medium-low, Orange=medium-high, Red=high
    private Color[] colors = new Color[]
    {
        new Color(0.2f, 0.8f, 0.2f), // green
        new Color(1f,   0.9f, 0f),   // yellow
        new Color(1f,   0.5f, 0f),   // orange
        new Color(0.9f, 0.1f, 0.1f)  // red
    };

    private string[] labels = { "Low", "Medium-Low", "Medium-High", "High" };
    private int _current = 0;

    void Start() => ApplyColor();

    void Update()
    {
        // B button on right controller to cycle
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            _current = (_current + 1) % colors.Length;
            ApplyColor();
        }
    }

    void ApplyColor()
    {
        brush.brushColor = colors[_current];
        if (colorLabel != null)
            colorLabel.text = labels[_current];
    }
}