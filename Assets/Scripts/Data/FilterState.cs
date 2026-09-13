using System;
using System.Collections.Generic;

[Serializable]
public class FilterState
{
    public List<string> selectedCategories = new List<string>();
    public List<string> selectedSubcategories = new List<string>();
    public List<string> selectedProductIds = new List<string>();


    public bool IsEmpty()
    {
        return selectedCategories.Count == 0 &&
               selectedSubcategories.Count == 0 &&
               selectedProductIds.Count == 0;
    }


    public void Clear()
    {
        selectedCategories.Clear();
        selectedSubcategories.Clear();
        selectedProductIds.Clear();
    }


    public FilterState Clone()
    {
        FilterState copy = new FilterState();

        copy.selectedCategories =
            new List<string>(selectedCategories);

        copy.selectedSubcategories =
            new List<string>(selectedSubcategories);

        copy.selectedProductIds =
            new List<string>(selectedProductIds);

        return copy;
    }
}
