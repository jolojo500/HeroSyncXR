using UnityEngine;

public class PaintableSurface : MonoBehaviour
{
    public int textureSize = 1024;

    Texture2D _canvas;
    Renderer  _renderer;

    static readonly int BaseMap   = Shader.PropertyToID("_BaseMap");
    static readonly int MainTex   = Shader.PropertyToID("_MainTex");
    static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer == null) return;

        _canvas = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[textureSize * textureSize];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        _canvas.SetPixels(pixels);
        _canvas.Apply();

        Material mat = _renderer.material;

        // Essaie toutes les propriétés possibles
        if (mat.HasProperty(BaseMap))
            mat.SetTexture(BaseMap, _canvas);
        
        if (mat.HasProperty(MainTex))
            mat.SetTexture(MainTex, _canvas);

        // Remet la couleur à blanc sans casser le shader
        if (mat.HasProperty(BaseColor))
            mat.SetColor(BaseColor, Color.white);
        else
            mat.color = Color.white;
    }

    public void Paint(Vector2 uv, BrushSettings brush)
    {
        if (_canvas == null) return;

        int cx = Mathf.RoundToInt(uv.x * textureSize);
        int cy = Mathf.RoundToInt(uv.y * textureSize);
        int r  = Mathf.Max(2, Mathf.RoundToInt(brush.brushSize * textureSize * 0.5f));

        for (int x = cx - r; x <= cx + r; x++)
        for (int y = cy - r; y <= cy + r; y++)
        {
            if (x < 0 || x >= textureSize || y < 0 || y >= textureSize) continue;
            if (Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy)) > r) continue;
            _canvas.SetPixel(x, y, Color.Lerp(
                _canvas.GetPixel(x, y), brush.brushColor, brush.opacity));
        }

        _canvas.Apply();
    }

    void OnDestroy()
    {
        if (_canvas) Destroy(_canvas);
    }

    // Returns the dominant non-white color painted, or Color.clear if untouched
    public Color GetDominantColor()
    {
        if (_canvas == null) return Color.clear;

        Color[] pixels = _canvas.GetPixels();
        float r = 0, g = 0, b = 0;
        int painted = 0;

        foreach (var p in pixels)
        {
            // skip white (unpainted) pixels
            if (p.r > 0.9f && p.g > 0.9f && p.b > 0.9f) continue;
            r += p.r; g += p.g; b += p.b;
            painted++;
        }

        if (painted == 0) return Color.clear; // untouched
        return new Color(r / painted, g / painted, b / painted);
    }

    public bool IsPainted()
    {
        return GetDominantColor() != Color.clear;
    }
}