using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ThumbnailCacheService : MonoBehaviour
{
    private Dictionary<string, Texture2D> thumbnailCache = new Dictionary<string, Texture2D>();
    // check the url for the image in product catalog json file and find if that already exists in the project

    public IEnumerator GetThumbnail(string url, Action<Texture2D> OnLoaded)
    {
        if (string.IsNullOrEmpty(url))
        {
            OnLoaded?.Invoke(null);
            yield break;
        }

        if (thumbnailCache.TryGetValue(url, out Texture2D cachedTexture))
        {
            OnLoaded?.Invoke(cachedTexture);
            yield break;
        }

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to load thumbnail from {url}: {request.error}");
            OnLoaded?.Invoke(null);
            request.Dispose();
            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);
        thumbnailCache[url] = texture;
        OnLoaded?.Invoke(texture);
        request.Dispose();
    }
}
