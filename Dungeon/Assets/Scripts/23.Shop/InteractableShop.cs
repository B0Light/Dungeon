using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class InteractableShop : Interactable
{
    [SerializeField] private bool purchaseWithItem = false;
    
    [Header("Sale Item")] 
    [SerializeField] private ItemType saleItemType;
    [SerializeField] private List<int> itemList = new List<int>();
    [ReadOnly] public List<ItemInfo> saleItemList = new List<ItemInfo>();
    
    // 상점 초기화 상태 관리
    private ShopInitializationState initState = ShopInitializationState.NotInitialized;
    private int currentLevel = -1; // -1은 레벨이 설정되지 않음을 의미
    
    private enum ShopInitializationState
    {
        NotInitialized,
        InitializedWithItemList,
        InitializedWithLevel
    }

    public override void Interact(PlayerManager player)
    {
        base.Interact(player);
        
        // 상점이 준비되지 않았다면 초기화
        if (!IsShopReady())
        {
            InitializeShop();
        }
        
        EnterShop();
    }

    /// <summary>
    /// 상점이 사용할 준비가 되었는지 확인
    /// </summary>
    /// <returns>상점 사용 가능 여부</returns>
    private bool IsShopReady()
    {
        return initState != ShopInitializationState.NotInitialized && saleItemList.Count > 0;
    }

    /// <summary>
    /// 현재 상태에 따라 적절한 방식으로 상점 초기화
    /// </summary>
    protected virtual void InitializeShop()
    {
        
        if (currentLevel >= 0)
        {
            // 레벨이 설정되어 있다면 레벨 기준으로 초기화
            InitShopByLevel(currentLevel);
            Debug.LogWarning("INIT SHOP : " + currentLevel);
        }
        else
        {
            // 레벨이 설정되지 않았다면 아이템 리스트 기준으로 초기화
            InitShopByItemList();
            Debug.LogWarning("INIT SHOP ItemList");
        }
    }

    /// <summary>
    /// 아이템 리스트를 기반으로 상점 초기화
    /// </summary>
    private void InitShopByItemList()
    {
        ClearSaleItems();
        
        foreach (int itemId in itemList)
        {
            ItemInfo originalItem = WorldDatabase_Item.Instance.GetItemByID(itemId);
            if (originalItem != null)
            {
                ItemInfo shopItem = CreateShopItem(originalItem);
                saleItemList.Add(shopItem);
            }
            else
            {
                Debug.LogWarning($"[InteractableShop] Item with ID {itemId} not found in database!");
            }
        }
        
        initState = ShopInitializationState.InitializedWithItemList;
        Debug.Log($"[InteractableShop] Initialized with item list. Items count: {saleItemList.Count}");
    }

    /// <summary>
    /// 레벨을 기반으로 상점 초기화
    /// </summary>
    /// <param name="level">상점 레벨</param>
    private void InitShopByLevel(int level)
    {
        ClearSaleItems();
        
        var availableItems = WorldDatabase_Item.Instance.GetItemsByTypeAndTierRange(
            saleItemType, 
            ItemTier.Common, 
            (ItemTier)Mathf.Clamp(level, 0, Enum.GetValues(typeof(ItemTier)).Length - 1)
        );
        
        foreach (var originalItem in availableItems)
        {
            ItemInfo shopItem = CreateShopItem(originalItem);
            saleItemList.Add(shopItem);
        }
        
        initState = ShopInitializationState.InitializedWithLevel;
        Debug.Log($"[InteractableShop] Initialized with level {level}. Items count: {saleItemList.Count}");
    }

    /// <summary>
    /// 원본 아이템을 기반으로 상점용 아이템 생성
    /// </summary>
    /// <param name="originalItem">원본 아이템</param>
    /// <returns>상점용 아이템</returns>
    private ItemInfo CreateShopItem(ItemInfo originalItem)
    {
        ItemInfo shopItem = Instantiate(originalItem);
        
        if (purchaseWithItem)
        {
            SetupItemPurchaseWithItem(shopItem);
        }
        
        return shopItem;
    }

    /// <summary>
    /// 아이템으로 구매하는 경우의 비용 설정
    /// </summary>
    /// <param name="item">설정할 아이템</param>
    private void SetupItemPurchaseWithItem(ItemInfo item)
    {
        item.purChaseWithItem = true;
        item.costItemList.Clear();
        
        int costItemCount = Random.Range(1, 4); // 1~3개의 비용 아이템
        
        for (int i = 0; i < costItemCount; i++)
        {
            // 현재 아이템 티어보다 낮은 티어의 잡화 아이템을 비용으로 설정
            int maxTierValue = Mathf.Max(0, (int)item.itemTier - 1);
            ItemTier costItemTier = (ItemTier)Random.Range(0, maxTierValue + 1);
            
            ItemInfo costItem = WorldDatabase_Item.Instance.GetRandomItemByTier<ItemInfoMisc>(costItemTier);
            if (costItem != null)
            {
                item.costItemList.Add(costItem.itemCode);
            }
        }
    }

    /// <summary>
    /// 판매 아이템 리스트 초기화
    /// </summary>
    private void ClearSaleItems()
    {
        saleItemList.Clear();
    }

    /// <summary>
    /// 상점 진입
    /// </summary>
    protected virtual void EnterShop()
    {
        GUIController.Instance.OpenShop(saleItemList, this);
    }

    /// <summary>
    /// 특정 레벨로 상점 설정 및 초기화
    /// </summary>
    /// <param name="level">설정할 레벨</param>
    public override void SetToSpecificLevel(int level)
    {
        if (level < 0)
        {
            Debug.LogWarning($"[InteractableShop] Invalid level: {level}. Level should be 0 or higher.");
            return;
        }
        
        currentLevel = level;
        
        // 이미 초기화된 상점이라면 레벨 기준으로 재초기화
        if (initState != ShopInitializationState.NotInitialized)
        {
            InitShopByLevel(level);
        }
    }
}