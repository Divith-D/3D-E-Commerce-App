using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProductViewerManager : MonoBehaviour
{
    [Serializable]
    public class ModelEntry
    {
        public string modelCategory;
        public GameObject prefab;

        [Header("Model Transform")]
        public Vector3 localPosition;
        public Vector3 localEulerAngles;
        public Vector3 localScale = Vector3.one;
    }


    [Header("Model")]
    [SerializeField] private Transform modelRoot;
    [SerializeField] private ModelEntry[] modelEntries;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI productNameText;
    [SerializeField] private TextMeshProUGUI productMetaText;
    [SerializeField] private Button backButton;

    [Header("Interaction")]
    [SerializeField] private ModelGestureController gestureController;

    [Header("Scene")]
    [SerializeField] private string mainSceneName = "Main-Scene";


    private GameObject currentModel;


    private void Start()
    {
        backButton.onClick.AddListener(BackToCatalogue);

        LoadSelectedProduct();
    }


    private void LoadSelectedProduct()
    {
        ProductData product =
            ProductSelectionContext.SelectedProduct;

        if (product == null)
        {
            Debug.LogError(
                "[VIEWER] Selected product is null."
            );

            return;
        }


        productNameText.text =
            product.productName;

        productMetaText.text =
            product.category + " • " + product.subcategory;


        foreach (ModelEntry entry in modelEntries)
        {
            if (!string.Equals(
                    entry.modelCategory,
                    product.modelCategory,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }


            currentModel =
                Instantiate(
                    entry.prefab,
                    modelRoot
                );


            Transform modelTransform =
                currentModel.transform;

            modelTransform.localPosition =
                entry.localPosition;

            modelTransform.localRotation =
                Quaternion.Euler(
                    entry.localEulerAngles
                );

            modelTransform.localScale =
                entry.localScale;


            gestureController.SetTarget(
                modelTransform
            );


            Debug.Log(
                "[VIEWER] Loaded model category: "
                + entry.modelCategory
            );

            return;
        }


        Debug.LogError(
            "[VIEWER] No model found for category: "
            + product.modelCategory
        );
    }


    public void BackToCatalogue()
    {
        ProductSelectionContext.Clear();

        SceneManager.LoadScene(mainSceneName);
    }
}