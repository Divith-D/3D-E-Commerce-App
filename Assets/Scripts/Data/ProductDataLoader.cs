using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;

public class ProductDataLoader : MonoBehaviour
{

    public IEnumerator LoadProducts(Action<ProductCatalogJson> OnLoaded)
    {
        string path = Application.streamingAssetsPath + "/products.json";

#if UNITY_EDITOR && Unity_Android
        string uri = path;
#else
         string uri = "file://" + path;
#endif

        UnityWebRequest request = UnityWebRequest.Get(uri);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            print("Product JSON Load Error: " + request.error);
            yield break;
        }

        string jsontext = request.downloadHandler.text;

        ProductCatalogJson catalog = JsonUtility.FromJson<ProductCatalogJson>(jsontext);

        if (catalog == null || catalog.products == null)
        {
            print("Product JSON Load Error: Invalid JSON format.");
            yield break;
        }

        Debug.Log("Product JSON Loaded Successfully. Total Products: " + catalog.products.Count);
        OnLoaded?.Invoke(catalog);
    }
}
