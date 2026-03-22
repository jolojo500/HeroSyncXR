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

    [Header("Carousel Scales")]
    public float centerScale = 1.4f;
    public float sideScale = 0.85f;

    [Header("Animation Speed")]
    public float scaleSpeed = 8f;

    private int centerIndex = 0;
    private bool isScrolling = false;

    // Target scales for smooth animation
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

        if (input > 0.5f && !isScrolling)
        {
            GoRight();
            isScrolling = true;
        }
        else if (input < -0.5f && !isScrolling)
        {
            GoLeft();
            isScrolling = true;
        }
        else if (Mathf.Abs(input) < 0.2f)
        {
            isScrolling = false;
        }

        // Right controller trigger to confirm
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            ConfirmSelection();
        }
    }

    void AnimateScales()
    {
        // Smoothly lerp all three slots to their target scales
        leftSlot.localScale = Vector3.Lerp(
            leftSlot.localScale, targetLeftScale, Time.deltaTime * scaleSpeed);
        centerSlot.localScale = Vector3.Lerp(
            centerSlot.localScale, targetCenterScale, Time.deltaTime * scaleSpeed);
        rightSlot.localScale = Vector3.Lerp(
            rightSlot.localScale, targetRightScale, Time.deltaTime * scaleSpeed);
    }

    public void GoLeft()
    {
        if (centerIndex > 0)
        {
            centerIndex--;
            UpdateCarousel();
        }
    }

    public void GoRight()
    {
        if (centerIndex < heroSprites.Length - 1)
        {
            centerIndex++;
            UpdateCarousel();
        }
    }

    void UpdateCarousel()
    {
        // --- Center slot (always visible) ---
        centerHeroImage.sprite = heroSprites[centerIndex];
        heroNameText.text = heroNames[centerIndex];
        centerBorder.color = selectedColor;
        targetCenterScale = Vector3.one * centerScale;

        // --- Left slot ---
        if (centerIndex - 1 >= 0)
        {
            leftHeroImage.sprite = heroSprites[centerIndex - 1];
            leftSlot.gameObject.SetActive(true);
            leftBorder.color = unselectedColor;
            targetLeftScale = Vector3.one * sideScale;
        }
        else
        {
            leftSlot.gameObject.SetActive(false);
        }

        // --- Right slot ---
        if (centerIndex + 1 < heroSprites.Length)
        {
            rightHeroImage.sprite = heroSprites[centerIndex + 1];
            rightSlot.gameObject.SetActive(true);
            rightBorder.color = unselectedColor;
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
        // Add scene transition here later
    }
}