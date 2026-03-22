using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Attache ce script + un Button sur chaque HeroImage.
/// Implémente les bonnes interfaces EventSystem pour que le raycast VR fonctionne.
/// </summary>
public class HeroClickable : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Tooltip("Nom EXACT de la scène à charger (doit être dans Build Settings)")]
    public string sceneToLoad;

    [Tooltip("Couleur de surbrillance quand le rayon pointe dessus")]
    public Color hoverColor = Color.yellow;

    private Image _image;
    private Color _originalColor;

    void Awake()
    {
        _image = GetComponent<Image>();
        if (_image != null)
            _originalColor = _image.color;
        else
            Debug.LogError($"[HeroClickable] Pas d'Image trouvée sur {gameObject.name} !");

        if (string.IsNullOrEmpty(sceneToLoad))
            Debug.LogWarning($"[HeroClickable] 'Scene To Load' est vide sur {gameObject.name} !");
    }

    // ── Interfaces EventSystem ──────────────────────────────────────────────
    // Ces méthodes DOIVENT avoir la signature exacte avec PointerEventData
    // sinon Unity ne les appelle JAMAIS, même avec un EventSystem actif.

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_image != null)
            _image.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_image != null)
            _image.color = _originalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        LoadScene();
    }

    // ── Appelé aussi par le composant Button (OnClick) ──────────────────────
    public void OnClick()
    {
        LoadScene();
    }

    void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("[HeroClickable] sceneToLoad est vide !");
            return;
        }
        Debug.Log($"[HeroClickable] Chargement : {sceneToLoad}");
        SceneManager.LoadScene(sceneToLoad);
    }
}
    