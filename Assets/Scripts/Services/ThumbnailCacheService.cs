using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ThumbnailCacheService : MonoBehaviour
{
    private readonly Dictionary<string, Texture2D> thumbnailCache =
        new Dictionary<string, Texture2D>();

    private readonly Dictionary<string, List<Action<Texture2D>>> pendingRequests =
        new Dictionary<string, List<Action<Texture2D>>>();


    public bool TryGetCached(
        string url,
        out Texture2D texture)
    {
        return thumbnailCache.TryGetValue(
            url,
            out texture
        );
    }


    public IEnumerator GetThumbnail(
        string url,
        Action<Texture2D> onLoaded)
    {
        if (string.IsNullOrEmpty(url))
        {
            onLoaded?.Invoke(null);
            yield break;
        }


        // Already downloaded
        if (thumbnailCache.TryGetValue(
                url,
                out Texture2D cachedTexture))
        {
            onLoaded?.Invoke(cachedTexture);
            yield break;
        }


        // Same URL is already downloading.
        // Just wait for that request.
        if (pendingRequests.TryGetValue(
                url,
                out List<Action<Texture2D>> callbacks))
        {
            callbacks.Add(onLoaded);
            yield break;
        }


        // First request for this URL
        pendingRequests[url] =
            new List<Action<Texture2D>>
            {
                onLoaded
            };


        using (
            UnityWebRequest request =
                UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();


            Texture2D texture = null;


            if (request.result ==
                UnityWebRequest.Result.Success)
            {
                texture =
                    DownloadHandlerTexture.GetContent(
                        request
                    );

                thumbnailCache[url] =
                    texture;
            }
            else
            {
                Debug.LogWarning(
                    "[THUMBNAIL] Failed: " +
                    url +
                    "\n" +
                    request.error
                );
            }


            if (pendingRequests.TryGetValue(
                    url,
                    out List<Action<Texture2D>> waitingCallbacks))
            {
                foreach (
                    Action<Texture2D> callback
                    in waitingCallbacks)
                {
                    callback?.Invoke(texture);
                }
            }


            pendingRequests.Remove(url);
        }
    }


    public IEnumerator PrewarmProducts(
        List<ProductData> products,
        int maxUniqueImages = 12)
    {
        if (products == null ||
            products.Count == 0)
        {
            yield break;
        }


        HashSet<string> uniqueUrls =
            new HashSet<string>();


        foreach (ProductData product in products)
        {
            string url =
                product.ThumbnailUrl;


            if (string.IsNullOrEmpty(url))
                continue;


            if (!uniqueUrls.Add(url))
                continue;


            if (!thumbnailCache.ContainsKey(url))
            {
                yield return GetThumbnail(
                    url,
                    texture => { }
                );
            }


            if (uniqueUrls.Count >= maxUniqueImages)
                yield break;
        }
    }
    public void RequestThumbnail(
    string url,
    Action<Texture2D> onLoaded)
{
    StartCoroutine(
        GetThumbnail(
            url,
            onLoaded
        )
    );
}
}