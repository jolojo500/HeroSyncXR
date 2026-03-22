using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroSelector : MonoBehaviour
{
    [Header("Hero Data")]
    public Sprite[] heroSprites;
    public string[] heroNames;

    [Header("UI Slot Images")]
    public Image leftHeroImage;
    public Image centerHeroImage;
    public Image rightHeroImage;

    [Header("Borders")]
    public Image leftBorder;
    public Image centerBorder;
    public Image rightBorder;

    [Header("Slot Transforms (for scaling)")]
    public RectTransform leftSlot;
    public RectTransform centerSlot;
    public RectTransform rightSlot;

    [Header("Name Label")]
    public TextMeshProUGUI heroNameText;

    [Header("Colors")]
    public Color selectedColor = Color.red;
    public Color unselectedColor = new Color(0.4f, 0.4f, 0.4f);
    public Color lockedTint = new Color(0.4f, 0.4f, 0.4f, 1f);
    public Color selectedTint = Color.white;

    [Header("Carousel Scales")]
    public float centerScale = 1.4f;
    public float sideScale = 0.85f;

    [Header("Animation Speed")]
    public float scaleSpeed = 8f;

    private int centerIndex = 0;
    private bool isScrolling = false;

    private Vector3 targetLeftScale;
    private Vector3 targetCenterScale;
    private Vector3 targetRightScale;

    void Start()
    {
        centerIndex = 0;
        targetLeftScale = Vector3.one * sideScale;
        targetCenterScale = Vector3.one * centerScale;
        targetRightScale = Vector3.one * sideScale;
        UpdateCarousel();
    }

    void Update()
    {
        HandleInput();
        AnimateScales();
    }

    void HandleInput()
    {
        float input = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;

        // Keyboard fallback for testing in editor
        if (Input.GetKeyDown(KeyCode.LeftArrow)) GoLeft();
        if (Input.GetKeyDown(KeyCode.RightArrow)) GoRight();
        if (Input.GetKeyDown(KeyCode.Return)) ConfirmSelection();

        if (input > 0.5f && !isScrolling) { GoRight(); isScrolling = true; }
        else if (input < -0.5f && !isScrolling) { GoLeft(); isScrolling = true; }
        else if (Mathf.Abs(input) < 0.2f) { isScrolling = false; }

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)) ConfirmSelection();
    }

    void AnimateScales()
    {
        leftSlot.localScale = Vector3.Lerp(
            leftSlot.localScale, targetLeftScale, Time.deltaTime * scaleSpeed);
        centerSlot.localScale = Vector3.Lerp(
            centerSlot.localScale, targetCenterScale, Time.deltaTime * scaleSpeed);
        rightSlot.localScale = Vector3.Lerp(
            rightSlot.localScale, targetRightScale, Time.deltaTime * scaleSpeed);
    }

    public void GoLeft()
    {
        if (centerIndex > 0) { centerIndex--; UpdateCarousel(); }
    }

    public void GoRight()
    {
        if (centerIndex < heroSprites.Length - 1) { centerIndex++; UpdateCarousel(); }
    }

    void UpdateCarousel()
    {
        // Center slot
        centerHeroImage.sprite = heroSprites[centerIndex];
        centerHeroImage.color = selectedTint;
        centerBorder.color = selectedColor;
        heroNameText.text = heroNames[centerIndex];
        targetCenterScale = Vector3.one * centerScale;

        // Left slot
        if (centerIndex - 1 >= 0)
        {
            leftHeroImage.sprite = heroSprites[centerIndex - 1];
            leftHeroImage.color = lockedTint;
            leftBorder.color = unselectedColor;
            leftSlot.gameObject.SetActive(true);
            targetLeftScale = Vector3.one * sideScale;
        }
        else
        {
            leftSlot.gameObject.SetActive(false);
        }

        // Right slot
        if (centerIndex + 1 < heroSprites.Length)
        {
            rightHeroImage.sprite = heroSprites[centerIndex + 1];
            rightHeroImage.color = lockedTint;
            rightBorder.color = unselectedColor;
            rightSlot.gameObject.SetActive(true);
            targetRightScale = Vector3.one * sideScale;
        }
        else
        {
            rightSlot.gameObject.SetActive(false);
        }
    }

    public void ConfirmSelection()
    {
        Debug.Log("Hero selected: " + heroNames[centerIndex]);
    }
}