using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductCardView : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI productNameText;
    //[SerializeField] private TextMeshProUGUI productDescriptionText;
    [SerializeField] private TextMeshProUGUI productMetaText;
    [SerializeField] private RawImage thumbnailRenderer;

    private string currentThumbnailUrl;

    [SerializeField] private string productName;
    [SerializeField] private string productDescription;
    [SerializeField] private string category;
    [SerializeField] private string subcategory;
    [SerializeField] private string ThumbnailUrl;

    public void SetData(ProductData Data, ThumbnailCacheService thumbnailCacheService)
    {
        productNameText.text = Data.productName;
        productMetaText.text = Data.category + " • " + Data.subcategory;
        currentThumbnailUrl = Data.ThumbnailUrl;

        if (string.IsNullOrEmpty(currentThumbnailUrl))
        {
            return;
        }

        string requestedUrl = currentThumbnailUrl;

                StartCoroutine(thumbnailCacheService.GetThumbnail(requestedUrl, texture =>
                {
                    if (currentThumbnailUrl != requestedUrl) return;

                    if (texture != null)
                    {
                        thumbnailRenderer.texture = texture;
                    }
                }
            )
        );
    }
}
