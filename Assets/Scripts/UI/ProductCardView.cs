using UnityEngine;
using TMPro ;

public class ProductCardView : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI productNameText;
    //[SerializeField] private TextMeshProUGUI productDescriptionText;
    //[SerializeField] private TextMeshProUGUI categoryText;
    //[SerializeField] private TextMeshProUGUI subcategoryText;
    [SerializeField] private TextMeshProUGUI productMetaText;

    [SerializeField] private string productName;
    [SerializeField] private string productDescription;
    [SerializeField] private string category;
    [SerializeField] private string subcategory;
    [SerializeField] private string ThumbnailUrl;

    public void SetData(ProductData Data)
    {
        productName = Data.productName;
        productDescription = Data.productDescription;
        category = Data.category;
        subcategory = Data.subcategory;
        ThumbnailUrl = Data.ThumbnailUrl;

        if (productNameText != null)
        {
            productNameText.text = productName;
            productMetaText.text = category + " • " + subcategory;
        }
    }
}
