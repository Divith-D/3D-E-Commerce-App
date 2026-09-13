public static class ProductSelectionContext
{
    public static ProductData SelectedProduct { get; private set; }

    public static void Select(ProductData product)
    {
        SelectedProduct = product;
    }

    public static void Clear()
    {
        SelectedProduct = null;
    }
}