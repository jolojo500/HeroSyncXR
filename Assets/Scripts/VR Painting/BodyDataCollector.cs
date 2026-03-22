using System.Collections.Generic;
using UnityEngine;

public class BodyDataCollector : MonoBehaviour
{
    public BodyPaintData CollectData()
    {
        var paintData = new BodyPaintData();
        var parts = FindObjectsOfType<PaintBlock>();
        Debug.Log($"[Collector] Found {parts.Length} PaintBlocks");

        foreach (var part in parts)
        {
            var surface = part.GetComponent<PaintableSurface>();
            Debug.Log($"[Collector] {part.gameObject.name} — surface: {surface != null} — painted: {(surface != null ? surface.IsPainted().ToString() : "N/A")}");
            if (surface == null || !surface.IsPainted()) continue;

            Color dominant = surface.GetDominantColor();
            paintData.zones.Add(new BodyZone
            {
                zone        = part.gameObject.name,
                symptomType = ColorToIntensity(dominant),
                intensity   = ColorToFloat(dominant)
            });
        }
        return paintData;
    }

    string ColorToIntensity(Color c)
    {
        if (c.g > 0.6f && c.r < 0.5f) return "low";
        if (c.r > 0.8f && c.g > 0.7f) return "medium-low";
        if (c.r > 0.8f && c.g > 0.3f && c.g < 0.7f) return "medium-high";
        return "high";
    }

    float ColorToFloat(Color c)
    {
        if (c.g > 0.6f && c.r < 0.5f) return 0.25f;
        if (c.r > 0.8f && c.g > 0.7f) return 0.5f;
        if (c.r > 0.8f && c.g > 0.3f) return 0.75f;
        return 1f;
    }
}