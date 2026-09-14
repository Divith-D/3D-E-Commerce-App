using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FilterItemView : MonoBehaviour
{
    [SerializeField] private RawImage thumbnail;
    [SerializeField] private TMP_Text productNameText;
    [SerializeField] private TMP_Text productMetaText;

    [Header("Selection")]
    [SerializeField] private Toggle selectionToggle;
    [SerializeField] private Image checkboxImage;
    [SerializeField] private Sprite uncheckedSprite;
    [SerializeField] private Sprite checkedSprite;

    private string productId;
    private Action<string, bool> selectionCallback;


    public void Setup(
        ProductData product,
        ThumbnailCacheService thumbnailCache,
        bool isSelected,
        Action<string, bool> onSelectionChanged)
    {
        productId = product.productId;
        selectionCallback = onSelectionChanged;


        productNameText.text =
            product.productName;

        productMetaText.text =
            product.category +
            " • " +
            product.subcategory;


        // ---------------------------------------
        // IMPORTANT
        // Clear old listeners before setting state
        // ---------------------------------------

        selectionToggle
            .onValueChanged
            .RemoveAllListeners();


        // ---------------------------------------
        // Restore remembered state
        // WITHOUT generating an event
        // ---------------------------------------

        selectionToggle
            .SetIsOnWithoutNotify(
                isSelected
            );


        UpdateCheckboxVisual(
            isSelected
        );


        // ---------------------------------------
        // Add ONE listener
        // ---------------------------------------

        selectionToggle
            .onValueChanged
            .AddListener(
                OnToggleChanged
            );


        // ---------------------------------------
        // Thumbnail
        // ---------------------------------------

        if (!string.IsNullOrEmpty(
                product.ThumbnailUrl))
        {
           string requestedUrl =
    product.ThumbnailUrl;

thumbnailCache.RequestThumbnail(
    requestedUrl,
    texture =>
    {
        // This filter row may already have
        // been destroyed during a refresh.
        if (this == null)
            return;

        if (texture != null &&
            thumbnail != null)
        {
            thumbnail.texture =
                texture;
        }
    }
);
        }
    }


    private void OnToggleChanged(
        bool selected)
    {
        UpdateCheckboxVisual(
            selected
        );


        selectionCallback?.Invoke(
            productId,
            selected
        );
    }


    private void UpdateCheckboxVisual(
        bool isOn)
    {
        if (checkboxImage == null)
            return;


        checkboxImage.sprite =
            isOn
                ? checkedSprite
                : uncheckedSprite;
    }


    // Called BEFORE destroying/rebuilding rows.
    //
    // Prevents an old Toggle from modifying
    // our remembered selection state while
    // Unity is destroying the object.
    public void Detach()
    {
        if (selectionToggle != null)
        {
            selectionToggle
                .onValueChanged
                .RemoveAllListeners();
        }


        selectionCallback = null;
    }
}