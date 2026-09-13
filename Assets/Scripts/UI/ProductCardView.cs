using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductCardView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI productNameText;
    [SerializeField] private TextMeshProUGUI productMetaText;
    [SerializeField] private RawImage thumbnailRenderer;

    private string currentThumbnailUrl;


    public void SetData(
        ProductData data,
        ThumbnailCacheService thumbnailCacheService)
    {
        productNameText.text = data.productName;
        productMetaText.text =
            data.category + " • " + data.subcategory;

        // A recycled card must never show the previous product's image
        // while the next thumbnail is loading.
        thumbnailRenderer.texture = null;

        currentThumbnailUrl = data.ThumbnailUrl;

        if (string.IsNullOrEmpty(currentThumbnailUrl))
            return;

        string requestedUrl = currentThumbnailUrl;

        StartCoroutine(
            thumbnailCacheService.GetThumbnail(
                requestedUrl,
                texture =>
                {
                    // The same card may have been recycled for another
                    // product while this request was still in progress.
                    if (currentThumbnailUrl != requestedUrl)
                        return;

                    if (texture != null)
                        thumbnailRenderer.texture = texture;
                }
            )
        );
    }


    public void ClearView()
    {
        // Invalidates any thumbnail callback still in flight.
        currentThumbnailUrl = null;

        productNameText.text = string.Empty;
        productMetaText.text = string.Empty;
        thumbnailRenderer.texture = null;
    }
}
