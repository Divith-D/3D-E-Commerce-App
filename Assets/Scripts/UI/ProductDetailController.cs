using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ProductDetailController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup panelGroup;

    [Header("Product UI")]
    [SerializeField] private RawImage productImage;
    [SerializeField] private TextMeshProUGUI productNameText;
    [SerializeField] private TextMeshProUGUI productMetaText;
    [SerializeField] private TextMeshProUGUI productDescriptionText;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button viewIn3DButton;
    [SerializeField] private string viewerSceneName = "ProductViewerScene";

    [Header("Services")]
    [SerializeField] private ThumbnailCacheService thumbnailCacheService;

    private ProductData currentProduct;
    private string currentThumbnailUrl;


    private void Start()
    {
        closeButton.onClick.AddListener(ClosePanel);
        viewIn3DButton.onClick.AddListener(ViewIn3D);

        HideImmediately();
    }


    public void OpenPanel(ProductData product)
    {
        if (product == null)
            return;

        currentProduct = product;

        productNameText.text = product.productName;

        productMetaText.text =
            product.category + " • " + product.subcategory;

        productDescriptionText.text =
            product.productDescription;

        productImage.texture = null;

        currentThumbnailUrl =
            product.ThumbnailUrl;


        ShowPanel();


        if (!string.IsNullOrEmpty(currentThumbnailUrl))
        {
            string requestedUrl =
                currentThumbnailUrl;

            StartCoroutine(
                thumbnailCacheService.GetThumbnail(
                    requestedUrl,
                    texture =>
                    {
                        // Prevent an old request from updating
                        // the panel after another product was opened.
                        if (currentThumbnailUrl != requestedUrl)
                            return;

                        if (texture != null)
                        {
                            productImage.texture = texture;
                        }
                    }
                )
            );
        }
    }


    public void ClosePanel()
    {
        currentProduct = null;
        currentThumbnailUrl = null;

        panelGroup.alpha = 0f;
        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;
    }


    private void ShowPanel()
    {
        panelGroup.alpha = 1f;
        panelGroup.interactable = true;
        panelGroup.blocksRaycasts = true;
    }


    private void HideImmediately()
    {
        panelGroup.alpha = 0f;
        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;
    }


    private void ViewIn3D()
    {
        if (currentProduct == null)
            return;

        ProductSelectionContext.Select(currentProduct);

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            "ProductViewerScene"
        );
    }


    public ProductData GetCurrentProduct()
    {
        return currentProduct;
    }
}