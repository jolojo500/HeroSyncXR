using UnityEngine;

/// <summary>
/// Quand on appuie sur la gâchette droite → signe Spider-Man
/// Index + auriculaire ouverts, majeur + annulaire fermés, pouce ouvert
/// </summary>
public class SpiderManGesture : MonoBehaviour
{
    [Header("Main à animer (Right)")]
    [Tooltip("Bone du majeur (doigt du milieu)")]
    public Transform middleFinger;
    [Tooltip("Bone de l'annulaire")]
    public Transform ringFinger;

    [Header("Rotation de fermeture des doigts")]
    [Tooltip("Rotation appliquée quand le doigt se ferme")]
    public Vector3 closedRotation = new Vector3(90f, 0f, 0f);

    [Header("Vitesse d'animation")]
    public float speed = 10f;

    private Quaternion _openRot;
    private Quaternion _closedRot;
    private bool _initialized = false;

    void Start()
    {
        // Sauvegarde la rotation initiale (ouverte)
        if (middleFinger != null && ringFinger != null)
        {
            _openRot   = middleFinger.localRotation;
            _closedRot = _openRot * Quaternion.Euler(closedRotation);
            _initialized = true;
        }
        else
        {
            Debug.LogWarning("[SpiderManGesture] Assigne les bones middleFinger et ringFinger !");
        }
    }

    void Update()
    {
        if (!_initialized) return;

        // Gâchette droite → ferme les doigts
        bool triggerPressed = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0.5f  // manette gauche
                           || OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.5f; // manette droite

        Quaternion target = triggerPressed ? _closedRot : _openRot;

        // Interpolation fluide
        middleFinger.localRotation = Quaternion.Lerp(middleFinger.localRotation, target, Time.deltaTime * speed);
        ringFinger.localRotation   = Quaternion.Lerp(ringFinger.localRotation,   target, Time.deltaTime * speed);
    }
}