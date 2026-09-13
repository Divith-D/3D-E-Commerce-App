using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductCardView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI productNameText;
    [SerializeField] private TextMeshProUGUI productMetaText;
    [SerializeField] private RawImage thumbnailRenderer;
    [SerializeField] private Button cardButton;

    private ProductData currentProduct;
    private ProductDetailController detailController;
    private string currentThumbnailUrl;


    private void Awake()
    {
        cardButton.onClick.AddListener(OnCardClicked);
    }


    public void SetData(
        ProductData data,
        ThumbnailCacheService thumbnailCacheService,
        ProductDetailController productDetailController)
    {
        currentProduct = data;
        detailController = productDetailController;

        productNameText.text = data.productName;
        productMetaText.text =
            data.category + " • " + data.subcategory;

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
                    if (currentThumbnailUrl != requestedUrl)
                        return;

                    if (texture != null)
                        thumbnailRenderer.texture = texture;
                }
            )
        );
    }


    private void OnCardClicked()
    {
        if (currentProduct == null ||
            detailController == null)
            return;

        detailController.OpenPanel(currentProduct);
    }


    public void ClearView()
    {
        currentProduct = null;
        currentThumbnailUrl = null;

        productNameText.text = "";
        productMetaText.text = "";
        thumbnailRenderer.texture = null;
    }
}