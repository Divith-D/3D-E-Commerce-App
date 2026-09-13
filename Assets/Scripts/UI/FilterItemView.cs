using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FilterItemView : MonoBehaviour
{
    [SerializeField] private RawImage thumbnail;
    [SerializeField] private TMP_Text productNameText;
    [SerializeField] private Toggle selectionToggle;

    private string productId;


    public void Setup(
        ProductData product,
        ThumbnailCacheService thumbnailCache,
        bool isSelected,
        Action<string, bool> onSelectionChanged)
    {
        productId = product.productId;

        productNameText.text =
            product.productName;

        selectionToggle.SetIsOnWithoutNotify(
            isSelected
        );

        selectionToggle.onValueChanged.RemoveAllListeners();

        selectionToggle.onValueChanged.AddListener(
            selected =>
            {
                onSelectionChanged?.Invoke(
                    productId,
                    selected
                );
            }
        );


        StartCoroutine(
            thumbnailCache.GetThumbnail(
                product.ThumbnailUrl,
                texture =>
                {
                    if (texture != null)
                    {
                        thumbnail.texture = texture;
                    }
                }
            )
        );
    }
}