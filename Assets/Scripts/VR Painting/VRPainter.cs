using UnityEngine;

public class VRPainter : MonoBehaviour
{
    public Transform rightControllerAnchor;
    public BrushSettings brush;
    public LayerMask paintableLayer;

    LineRenderer _line;

    void Awake()
    {
        // Crée le LineRenderer automatiquement sur ce GameObject
        _line = gameObject.AddComponent<LineRenderer>();
        _line.positionCount = 2;
        _line.startWidth = 0.004f;
        _line.endWidth = 0.001f;     // effilé vers la pointe
        _line.useWorldSpace = true;
        _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        // Material simple sans ombre
        _line.material = new Material(Shader.Find("Sprites/Default"));
        _line.startColor = new Color(1f, 1f, 1f, 0.8f);
        _line.endColor = new Color(1f, 0.3f, 0.3f, 1f); // rouge à la pointe
    }

    void Update()
    {
        Ray ray = new Ray(rightControllerAnchor.position, rightControllerAnchor.forward);

        bool trigger = OVRInput.Get(
            OVRInput.Button.PrimaryIndexTrigger,
            OVRInput.Controller.RTouch
        );

        if (Physics.Raycast(ray, out RaycastHit hit, 10f, paintableLayer))
        {
            // Ligne du contrôleur jusqu'au point de contact
            _line.SetPosition(0, ray.origin);
            _line.SetPosition(1, hit.point);

            // Couleur de la ligne = couleur du brush quand on appuie
            _line.startColor = trigger
             ? new Color(brush.brushColor.r, brush.brushColor.g, brush.brushColor.b, 0.9f)
             : new Color(brush.brushColor.r, brush.brushColor.g, brush.brushColor.b, 0.4f);
            _line.endColor = brush.brushColor;

            // Peindre si gâchette enfoncée
            if (trigger && hit.collider.CompareTag("Paintable"))
            {
                var surface = hit.collider.GetComponent<PaintableSurface>();
                if (surface != null)
                    surface.Paint(hit.textureCoord, brush);
            }
        }
        else
        {
            // Rien touché — ligne droite de longueur fixe
            _line.SetPosition(0, ray.origin);
            _line.SetPosition(1, ray.origin + ray.direction * 3f);
            _line.startColor = new Color(brush.brushColor.r, brush.brushColor.g, brush.brushColor.b, 0.4f);
            _line.endColor   = new Color(brush.brushColor.r, brush.brushColor.g, brush.brushColor.b, 0.8f);
        }
    }

    void OnEnable()  => _line.enabled = true;
void OnDisable() => _line.enabled = false;
}