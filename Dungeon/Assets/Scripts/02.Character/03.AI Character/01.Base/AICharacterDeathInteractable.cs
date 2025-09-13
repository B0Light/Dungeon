using System.Collections.Generic;
using UnityEngine;

public class AICharacterDeathInteractable : Interactable
{
    [Header("Character Reference")]
    private AICharacterManager _aiCharacterManager;
    
    [Header("Loot Box Configuration")]
    [SerializeField] private int boxWidth = 5;
    [SerializeField] private int boxHeight = 5;
    
    [Header("Generated Loot (Runtime)")]
    [SerializeField] private List<int> generatedItemIds = new List<int>();
    
    #region Unity Lifecycle
    
    private void Start()
    {
        InitializeComponents();
        GenerateLootItems();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeComponents()
    {
        _aiCharacterManager = GetComponentInParent<AICharacterManager>();
        
        if (_aiCharacterManager == null)
        {
            Debug.LogError($"AICharacterManager not found in parent of {gameObject.name}");
            return;
        }
        
        // 캐릭터가 살아있는 동안에는 상호작용 불가
        SetInteractableState(false);
    }
    
    private void GenerateLootItems()
    {
        if (_aiCharacterManager == null || WorldCharacterDropItem.Instance == null)
            return;
            
        if (!WorldCharacterDropItem.Instance.dropItemDic.ContainsKey(_aiCharacterManager.characterID))
        {
            Debug.LogWarning($"No drop items found for character ID: {_aiCharacterManager.characterID}");
            return;
        }
        
        var dropItems = WorldCharacterDropItem.Instance.dropItemDic[_aiCharacterManager.characterID];
        generatedItemIds.Clear();
        
        foreach (var dropItem in dropItems)
        {
            if (ShouldDropItem(dropItem))
            {
                AddItemsToLoot(dropItem);
            }
        }
    }
    
    #endregion
    
    #region Loot Generation Logic
    
    private bool ShouldDropItem(DropItem dropItem)
    {
        return dropItem.isGuaranteed || Random.value <= dropItem.dropRate;
    }
    
    private void AddItemsToLoot(DropItem dropItem)
    {
        int dropCount = Random.Range(dropItem.minCount, dropItem.maxCount + 1);
        
        for (int i = 0; i < dropCount; i++)
        {
            generatedItemIds.Add(dropItem.itemID);
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// 캐릭터가 죽었을 때 호출되어 상호작용을 활성화합니다.
    /// </summary>
    public void PerformDeath()
    {
        SetInteractableState(true);
    }
    
    public override void Interact(PlayerManager player)
    {
        base.Interact(player);
        
        GUIController.Instance.WaitToInteraction(OpenLootBox);
    }
    
    #endregion
    
    #region Private Methods
    
    private void SetInteractableState(bool isInteractable)
    {
        if (interactableCollider != null)
        {
            interactableCollider.enabled = isInteractable;
        }
    }
    
    private void OpenLootBox()
    {
        if (GUIController.Instance == null)
        {
            Debug.LogError("GUIController instance not found");
            return;
        }
        
        GUIController.Instance.OpenInteractableBox(boxWidth, boxHeight, generatedItemIds, this);
    }
    
    #endregion
    
    #region Debug Methods
    
    [ContextMenu("Debug Generated Items")]
    private void DebugGeneratedItems()
    {
        Debug.Log($"Generated {generatedItemIds.Count} items: [{string.Join(", ", generatedItemIds)}]");
    }
    
    #endregion
}