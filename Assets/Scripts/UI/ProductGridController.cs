using UnityEngine;

public class ProductGridController : MonoBehaviour
{
    [SerializeField] private ProductCardView productCardPrefab;
    [SerializeField] private ProductManager productManager;
    [SerializeField] private Transform SpawnParent;

    void OnEnable()
    {
        productManager.ProductsLoaded += RefreshProducts;
    }

    void OnDisable()
    {
        productManager.ProductsLoaded -= RefreshProducts;
    }

    public void RefreshProducts()
    {
        foreach(ProductData product in productManager.allProducts)
        {
            ProductCardView card= Instantiate(productCardPrefab, SpawnParent);
            card.SetData(product);
        }
    }
}
