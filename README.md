# KiXR Unity Developer Assignment
## 3D E-Commerce Product Filter & Preview System

**Developer:** Divith M J  
**Engine:** Unity 6.3 LTS  
**Target:** Android (API 29+)  

---

## Overview

This project is a Unity-based Android application that demonstrates a runtime-driven product catalogue, dynamic filtering, product detail viewing, and interactive 3D product preview.

The implementation focuses on clean separation of data, UI, filtering, performance, and 3D interaction logic.

## Implemented Features

- Runtime product loading from `StreamingAssets/products.json`
- Category, subcategory, and item-level filtering
- Apply / Reset / Close filter workflow with independent filter state
- Scrollable product catalogue with pooled and virtualized product cards
- Lazy thumbnail loading using `UnityWebRequest`
- In-memory thumbnail cache using `Dictionary<string, Texture2D>`
- Product detail panel with image, metadata, description, and 3D entry point
- Dedicated 3D viewer scene
- Representative 3D model mapping by product category
- Single-finger / mouse drag rotation
- Two-finger pinch scaling with clamped limits
- Double-tap / double-click smooth reset
- Android build tested on device

## Architecture Summary

```text
StreamingAssets/products.json
        ↓
ProductDataLoader
        ↓
ProductManager
   ├── FilterState
   ├── ProductGridController
   └── FilterPanelController

Thumbnail URLs
        ↓
ThumbnailCacheService
        ↓
Product Cards / Detail Panel

Product Card
        ↓
ProductDetailController
        ↓
ProductSelectionContext
        ↓
ProductViewerScene
   ├── ProductViewerManager
   └── ModelGestureController
```

The catalogue data is owned by `ProductManager`, while filter state is stored separately from the UI. UI controllers consume the current product state and do not own the source data.

## Performance

The catalogue uses virtualization and object pooling so the number of instantiated card GameObjects is based on the visible viewport rather than the total number of products.

Thumbnail textures are requested only when cards become visible or are rebound, and successfully downloaded textures are cached for the session.

## 3D Interaction

### Android
- One-finger drag → Rotate
- Two-finger pinch → Scale
- Double tap → Smooth reset

### Unity Editor
- Mouse drag → Rotate
- Mouse wheel → Scale
- Double click → Smooth reset

## Project Structure

- `Main-Scene` — Catalogue, filtering, and product detail UI
- `ProductViewerScene` — 3D preview environment and gestures
- `Assets/StreamingAssets/products.json` — Runtime product data
- `CREDITS.txt` — Third-party asset references

## Running the Project

1. Open the project in Unity 6.3 LTS.
2. Open `Main-Scene`.
3. Ensure Android Build Support is installed.
4. Include both `Main-Scene` and `ProductViewerScene` in the build scene list.
5. Build for Android API 29+.

Internet access is required for runtime thumbnail downloads.

## Known Limitations

- One representative 3D model is used per product category.
- Search and custom sorting bonus tasks are not included.
- Minor UI refinement may still be required for some uncommon screen aspect ratios.

## Runtime Thumbnail Hosting

Product catalogue data is loaded locally from:

`Assets/StreamingAssets/products.json`

Product thumbnail images are loaded at runtime from a lightweight static image host deployed on Netlify:

`https://kixr-ecom.netlify.app`

The Netlify site is used only to host the assignment thumbnail assets required by the product JSON. Product data, filtering logic, catalogue state, and 3D model mappings remain inside the Unity application.

### Runtime Flow

products.json
→ ThumbnailURL
→ Netlify static image host
→ UnityWebRequest
→ ThumbnailCacheService
→ Texture2D cache
→ Product Card / Detail View

Downloaded textures are cached in memory for the current application session to avoid requesting the same URL repeatedly.

---