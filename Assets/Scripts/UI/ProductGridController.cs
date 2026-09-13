using UnityEngine;

public class ProductGridController : MonoBehaviour
{
    [SerializeField]
    private ProductCardView productCardPrefab;

    [SerializeField]
    private ProductManager productManager;

    [SerializeField]
    private Transform spawnParent;

    [SerializeField]
    private ThumbnailCacheService thumbnailCacheService;


    private void OnEnable()
    {
        productManager.ProductsChanged += RefreshProducts;
    }


    private void OnDisable()
    {
        productManager.ProductsChanged -= RefreshProducts;
    }


    private void Start()
    {
        // Handles the case where products loaded
        // before this controller subscribed.
        if (productManager.CurrentProducts.Count > 0)
        {
            RefreshProducts();
        }
    }


    public void RefreshProducts()
    {
        Debug.Log(
            "[GRID] Refreshing with "
            + productManager.CurrentProducts.Count
            + " products"
        );

        // TEMPORARY until pooling is implemented.
        for (int i = spawnParent.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                spawnParent.GetChild(i).gameObject
            );
        }


        foreach (
            ProductData product
            in productManager.CurrentProducts)
        {
            ProductCardView card =
                Instantiate(
                    productCardPrefab,
                    spawnParent
                );

            card.SetData(
                product,
                thumbnailCacheService
            );
        }
    }
}