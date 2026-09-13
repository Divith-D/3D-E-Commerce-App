using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ProductDataLoader : MonoBehaviour
{
    [SerializeField]
    private string jsonFileName = "products.json";

    public IEnumerator LoadProducts(
        Action<ProductCatalogJson> onLoaded)
    {
        string path = Path.Combine(
            Application.streamingAssetsPath,
            jsonFileName
        );

#if UNITY_ANDROID && !UNITY_EDITOR
        string url = path;
#else
        string url = new Uri(path).AbsoluteUri;
#endif

        Debug.Log(
            "[DATA] Loading products from: " + url
        );

        using UnityWebRequest request =
            UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result !=
            UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                "[DATA] Failed to load products: "
                + request.error
            );

            yield break;
        }

        string json =
            request.downloadHandler.text;

        Debug.Log(
            "[DATA] JSON downloaded. Characters: "
            + json.Length
        );

        ProductCatalogJson catalog =
            JsonUtility.FromJson<ProductCatalogJson>(
                json
            );

        if (catalog == null ||
            catalog.products == null)
        {
            Debug.LogError(
                "[DATA] JSON parsed but product catalogue is invalid."
            );

            yield break;
        }

        Debug.Log(
            "[DATA] Parsed products: "
            + catalog.products.Count
        );

        onLoaded?.Invoke(catalog);
    }
}