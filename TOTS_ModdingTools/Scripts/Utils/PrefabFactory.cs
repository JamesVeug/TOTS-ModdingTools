using System.Collections.Generic;
using TOTS_ModdingTools;
using TOTS_ModdingTools.Helpers;
using TotS;
using TotS.Items;
using TotS.Streaming;
using UnityEngine;
using UnityEngine.AI;

public static class PrefabFactory
{
    public static MaterialHighlighterConfig MaterialHighlighterConfig;
    public static KineticConfig KineticConfig;
    
    private static Dictionary<string, GameObject> _cachedPrefabs = new Dictionary<string, GameObject>();
    private static Dictionary<string, GameObject> _cachedMealPrefabs = new Dictionary<string, GameObject>();
    
    public static GameObject CreatePrefab(ItemType itemType, Sprite image, Vector3 scale, string cachedName, float[] primaryColors, float[] secondaryColors)
    {
        if (!string.IsNullOrEmpty(cachedName) && _cachedPrefabs.TryGetValue(cachedName, out var cachedPrefab))
        {
            APILogger.LogVerbose("Using cached prefab for " + cachedName);
            return cachedPrefab;
        }

        scale *= 0.25f;
        
        // Parent GameObject
        // - Width (Sphere collider component)
        // - ArtRoot
        // - - Square (Sprite Renderer component)
        APILogger.LogVerbose("Creating new prefab for " + itemType.name);
        GameObject prefab = new GameObject(itemType.name);
        prefab.transform.SetParent(ModdingToolsPlugin.Instance.PrefabContainer);
        ItemComponent itemComponent = prefab.AddComponent<ItemComponent>();
        itemComponent.m_ItemType = itemType;
        
        APILogger.LogVerbose("Applying ColorInfo");
        ColorInfo colorInfo = prefab.AddComponent<ColorInfo>();
        colorInfo.m_PrimaryColor = new Color(primaryColors.SafeGet(0, 255f)/255f, primaryColors.SafeGet(1, 255f)/255f, primaryColors.SafeGet(2, 255f)/255f, 1);
        colorInfo.m_SecondaryColor = new Color(secondaryColors.SafeGet(0, 255f)/255f, secondaryColors.SafeGet(1, 255f)/255f, secondaryColors.SafeGet(2, 255f)/255f, 1);
        
        APILogger.LogVerbose("Applying Collider");
        GameObject width = new GameObject("Collider");
        width.transform.SetParent(prefab.transform);
        SphereCollider sphereCollider = width.AddComponent<SphereCollider>(); // Not used in-game
        sphereCollider.radius = 0.13f;
        
        GameObject artRoot = new GameObject("ArtRoot");
        artRoot.transform.SetParent(prefab.transform);
        artRoot.transform.localPosition = new Vector3(0, 0, 0);
        
        APILogger.LogVerbose("Setting up sprite");
        GameObject square = new GameObject("Square");
        square.transform.SetParent(artRoot.transform);

        float pixelsPerUnit = image.pixelsPerUnit;
        float visualHeight = (image.rect.height / pixelsPerUnit) * scale.y;
        square.transform.transform.localPosition = new Vector3(0, visualHeight/2f, 0);
        
        
        
        square.transform.transform.localScale = scale;
        SpriteRenderer spriteRenderer = square.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = image;

        if (!string.IsNullOrEmpty(cachedName))
        {
            _cachedPrefabs[cachedName] = prefab;
        }

        
        APILogger.LogVerbose("Finished creating prefab");
        return prefab;
    }
    
    public static GameObject CreatePlaceableMealPrefab(ItemType itemType, Sprite image, Vector3 scale, string cachedName, float[] primaryColors, float[] secondaryColors)
    {
        if (!string.IsNullOrEmpty(cachedName) && _cachedMealPrefabs.TryGetValue(cachedName, out var cachedPrefab))
        {
            APILogger.LogVerbose("Using cached meal prefab for " + cachedName);
            return cachedPrefab;
        }
        
        
        GameObject prefab = CreatePrefab(itemType, image, scale, "", primaryColors, secondaryColors);
        ItemComponent itemComponent = prefab.GetComponent<ItemComponent>();
        
        APILogger.LogVerbose("Adding new components");
        Rigidbody rigidbody = prefab.AddComponent<Rigidbody>();
        NavMeshObstacle navMeshObstacle = prefab.AddComponent<NavMeshObstacle>();
        Placeable placeable = prefab.AddComponent<Placeable>();
        Kinetic kinetic = prefab.AddComponent<Kinetic>();
        DynamicStreamedItem dynamicStreamedItem = prefab.AddComponent<DynamicStreamedItem>();
        InteractableHub interactableHub = prefab.AddComponent<InteractableHub>();
        DisplayStateMeal displayStateMeal = prefab.AddComponent<DisplayStateMeal>();
        
        // RigidBody
        APILogger.LogVerbose("Initializing rigidbody");
        rigidbody.isKinematic = true;
        rigidbody.freezeRotation = true;
        rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        
        // NavMeshObstacle
        APILogger.LogVerbose("Initializing navmesh obstacle");
        navMeshObstacle.carving = false;
        navMeshObstacle.carveOnlyStationary = true;
        navMeshObstacle.carvingMoveThreshold = 0.1f;
        navMeshObstacle.carvingTimeToStationary = 0.5f;
        navMeshObstacle.center = new Vector3(-0.0058f, 0.0514f, -0.0048f);
        navMeshObstacle.height = 0.0607f;
        navMeshObstacle.radius = 0.1956f;
        navMeshObstacle.shape = NavMeshObstacleShape.Box;
        navMeshObstacle.size = new Vector3(0.3913f, 0.1215f, 0.5642f);
        
        // InteractableHub
        APILogger.LogVerbose("Initializing interactable");
        interactableHub.m_ItemComponent = itemComponent;
        interactableHub.m_Kinetic = kinetic;
        interactableHub.m_Placeble = placeable;
        
        // Kinetic
        APILogger.LogVerbose("Initializing kinetic");
        kinetic.m_Config = KineticConfig;
        kinetic.m_Hub = interactableHub;
        kinetic.m_RigidBody = rigidbody;
        
        // DynamicStreamedItem
        APILogger.LogVerbose("Initializing dynamic streamed item");
        dynamicStreamedItem.m_Hub = interactableHub;
        
        // Placeable
        APILogger.LogVerbose("Initializing placeable");
        placeable.m_Hub = interactableHub;
        placeable.m_Stackable = null;
        placeable.m_DisplayState = displayStateMeal;
        placeable.m_Rigidbody = rigidbody;
        placeable.m_ArtPrefabs = new List<PipelineMetadata>();
        placeable.m_PlaceHandle = null;
        placeable.m_ShowFrontIcon = interactableHub;
        placeable.m_FrontIconPos = Vector3.zero;
        placeable.m_ArtPrefabDateTimes = new int[]{-934317238, 398095401};
        
        // DisplayStateMeal
        APILogger.LogVerbose("Initializing display state meal");
        displayStateMeal.m_ItemComponent = itemComponent;

        APILogger.LogVerbose("Setting up art");
        GameObject artRoot = prefab.transform.FindChildRecursive("ArtRoot", true).gameObject;
        GameObject meal = artRoot.transform.FindChildRecursive("Square", true).gameObject;
        InteractableRenderer interactableRenderer = meal.AddComponent<InteractableRenderer>();
        MaterialHighlighter materialHighlighter = meal.AddComponent<MaterialHighlighter>();
        
        SphereCollider sphereCollider = meal.AddComponent<SphereCollider>(); // Not used in-game
        sphereCollider.radius = 2f;
        PlaceableCollider placeableCollider = meal.AddComponent<PlaceableCollider>(); // Not used in-game
        placeableCollider.m_Collider = sphereCollider;
        placeableCollider.m_Hub = interactableHub;
        placeableCollider.m_Placeable = placeable;
        
        interactableRenderer.m_Renderer = meal.GetComponent<Renderer>();
        interactableRenderer.m_Interactable = null;
        interactableRenderer.m_Placeable = placeable;
        interactableRenderer.m_StylingFeature = null;
        
        materialHighlighter.m_Interactable = null;
        materialHighlighter.m_Placeable = placeable;
        materialHighlighter.m_InteractableRenderer = interactableRenderer;
        materialHighlighter.m_StylingFeature = null;
        materialHighlighter.m_Config = MaterialHighlighterConfig;
        
        displayStateMeal.m_Meal = meal;
        displayStateMeal.m_Meal.name = "Meal";
        // displayStateMeal.m_Meal.transform.localPosition = new Vector3(0, 1.3f, 0);
        
        displayStateMeal.m_Portion = GameObject.Instantiate(displayStateMeal.m_Meal, displayStateMeal.m_Meal.transform.parent);
        displayStateMeal.m_Portion.name = "Portion";
        // displayStateMeal.m_Portion.transform.localPosition = new Vector3(0, 1.3f, 0);
        
        displayStateMeal.m_Cut = GameObject.Instantiate(displayStateMeal.m_Meal, displayStateMeal.m_Meal.transform.parent);
        displayStateMeal.m_Cut.name = "cut";
        // displayStateMeal.m_Cut.transform.localPosition = new Vector3(0, 1.3f, 0);
        


        if (!string.IsNullOrEmpty(cachedName))
        {
            _cachedMealPrefabs[cachedName] = prefab;
        }

        APILogger.LogVerbose("Finished creating meal prefab");
        return prefab;
    }
}