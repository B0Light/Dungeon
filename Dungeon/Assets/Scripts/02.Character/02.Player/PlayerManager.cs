using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerManager : CharacterManager
{
    [HideInInspector] public PlayerAnimatorManager playerAnimatorManager;
    [HideInInspector] public PlayerLocomotionManager playerLocomotionManager;
    [HideInInspector] public PlayerVariableManager playerVariableManager;
    [HideInInspector] public PlayerStatsManager playerStatsManager;
    [HideInInspector] public PlayerEquipmentManger playerEquipmentManger;
    [HideInInspector] public PlayerCombatManager playerCombatManager;
    [HideInInspector] public PlayerSoundFXManager playerSoundFXManager;
    [HideInInspector] public PlayerInteractionManager playerInteractionManager;
    [HideInInspector] public PlayerItemConsumeManager playerItemConsumeManager;
    
    [HideInInspector] public CharacterController characterController;
    
    [SerializeField] private Vector3 initialPosition = new Vector3(0,10,0);
    
    private bool _isApplicationQuitting = false;
    
    protected override void Awake()
    {
        base.Awake();
        
        Debug.Log("Player Ready");
        
        // DO MORE STUFF, ONLY FOR THE PLAYER

        playerLocomotionManager = characterLocomotionManager as PlayerLocomotionManager;
        playerAnimatorManager = characterAnimatorManager as PlayerAnimatorManager;
        playerVariableManager = characterVariableManager as PlayerVariableManager;
        playerStatsManager = characterStatsManager as PlayerStatsManager;
        playerEquipmentManger = characterEquipmentManager as PlayerEquipmentManger;
        playerCombatManager =  characterCombatManager as PlayerCombatManager;
        playerSoundFXManager = characterSoundFXManager as PlayerSoundFXManager;
        playerInteractionManager = GetComponent<PlayerInteractionManager>();
        playerItemConsumeManager = GetComponent<PlayerItemConsumeManager>();
        
        characterController = GetComponent<CharacterController>();
    }

    protected override void Start()
    {
        base.Start();
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    protected override void Update()
    {
        base.Update();

        // REGEN STAMINA
        playerStatsManager.RegenerateStamina();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        PlayerCameraController.Instance.SetPlayer(this);
        PlayerInputManager.Instance.playerManager = this;
        
        isDead.OnValueChanged += playerVariableManager.OnPlayerDeath;
        // Updates ui stat bars when a stat changes (health or stamina)
        playerVariableManager.health.OnSetMaxValue += playerVariableManager.SetNewMaxHealthPoint;
        playerVariableManager.actionPoint.OnSetMaxValue += playerVariableManager.SetNewMaxActionPoint;
        
        playerVariableManager.health.OnValueChanged  += playerStatsManager.SetNewHealthPoint;
        playerVariableManager.actionPoint.OnValueChanged += playerStatsManager.SetNewActionPoint;
        
        playerVariableManager.hungerLevel.OnValueChanged += playerStatsManager.OnHungryLevelChange;
        
        //Control
        playerVariableManager.canControl.OnValueChanged += playerVariableManager.OnControlChange;
        
        //equip
        playerVariableManager.currentEquippedWeaponID.OnValueChanged += playerVariableManager.CurrentEquippedWeaponIDChange;
        playerVariableManager.currentHelmetID.OnValueChanged += playerVariableManager.CurrentEquippedHelmetIDChange;
        playerVariableManager.currentArmorID.OnValueChanged += playerVariableManager.CurrentEquippedArmorIDChange;
            
        playerVariableManager.currentQuickSlotIDList.OnItemAdded += playerVariableManager.OnAddQuickSlotItem;
        playerVariableManager.currentQuickSlotIDList.OnItemRemoved += playerVariableManager.OnRemoveQuickSlotItem;
        playerVariableManager.currentQuickSlotIDList.OnListCleared += playerVariableManager.OnQuickSlotClear;
        playerVariableManager.currentSelectQuickSlotItem.OnValueChanged += playerVariableManager.OnSelectQuickSlotItemChange;

        playerVariableManager.perkHealByFullness.OnValueChanged += playerVariableManager.OnHealByFullness;
            
        playerVariableManager.perkExtraAttack.OnValueChanged += playerVariableManager.OnExtraAttackChange;
        playerVariableManager.perkExtraDefence.OnValueChanged += playerVariableManager.OnExtraDefenceChange;
        playerVariableManager.perkExtraMagicGuard.OnValueChanged += playerVariableManager.OnExtraMagicGuardChange;
        playerVariableManager.perkExtraHealthPoint.OnValueChanged += playerVariableManager.OnExtraHealthPointChange;
        playerVariableManager.perkExtraActionPoint.OnValueChanged += playerVariableManager.OnExtraActionPointChange;

        playerVariableManager.perkExtraWeight.OnValueChanged += playerVariableManager.OnExtraWeight;

        playerVariableManager.perkSafeActive.OnValueChanged += GUIController.Instance.inventoryGUIManager.ActiveSafe;
        
        WorldPlayerInventory.Instance.itemWeight.OnValueChanged += playerVariableManager.UpdatePlayerWeight;

        playerVariableManager.UpdatePlayerWeight(WorldPlayerInventory.Instance.itemWeight.Value);
    }

    void OnApplicationQuit()
    {
        _isApplicationQuitting = true;
    }

    protected override void OnDisable()
    {
        if(_isApplicationQuitting || !playerVariableManager) return;
        base.OnDisable();
        
        // Updates ui stat bars when a stat changes (health or stamina)
        playerVariableManager.health.OnSetMaxValue -= playerVariableManager.SetNewMaxHealthPoint;
        playerVariableManager.actionPoint.OnSetMaxValue -= playerVariableManager.SetNewMaxActionPoint;
        
        playerVariableManager.health.OnValueChanged  -= playerStatsManager.SetNewHealthPoint;
        playerVariableManager.actionPoint.OnValueChanged -= playerStatsManager.SetNewActionPoint;
        
        playerVariableManager.canControl.OnValueChanged -= playerVariableManager.OnControlChange;
        
        //equip
        playerVariableManager.currentEquippedWeaponID.OnValueChanged -= playerVariableManager.CurrentEquippedWeaponIDChange;
        playerVariableManager.currentHelmetID.OnValueChanged -= playerVariableManager.CurrentEquippedHelmetIDChange;
        playerVariableManager.currentArmorID.OnValueChanged -= playerVariableManager.CurrentEquippedArmorIDChange;
        
        playerVariableManager.currentQuickSlotIDList.OnItemAdded -= playerVariableManager.OnAddQuickSlotItem;
        playerVariableManager.currentQuickSlotIDList.OnItemRemoved -= playerVariableManager.OnRemoveQuickSlotItem;
        playerVariableManager.currentQuickSlotIDList.OnListCleared -= playerVariableManager.OnQuickSlotClear;
        playerVariableManager.currentSelectQuickSlotItem.OnValueChanged -= playerVariableManager.OnSelectQuickSlotItemChange;
        
        playerVariableManager.perkExtraAttack.OnValueChanged -= playerVariableManager.OnExtraAttackChange;
        playerVariableManager.perkExtraDefence.OnValueChanged -= playerVariableManager.OnExtraDefenceChange;
        playerVariableManager.perkExtraMagicGuard.OnValueChanged -= playerVariableManager.OnExtraMagicGuardChange;
        playerVariableManager.perkExtraHealthPoint.OnValueChanged -= playerVariableManager.OnExtraHealthPointChange;
        playerVariableManager.perkExtraActionPoint.OnValueChanged -= playerVariableManager.OnExtraActionPointChange;
        
        WorldPlayerInventory.Instance.itemWeight.OnValueChanged -= playerVariableManager.UpdatePlayerWeight;
    }

    protected override IEnumerator ProcessDeathEvent()
    {
        GUIController.Instance.playerUIPopUpManager.SendYouDiedPopUp();
        
        PlayerInputManager.Instance.SetControlActive(false);
        
        yield return new WaitForFixedUpdate();
        characterAnimatorManager.PlayTargetActionAnimation("Dead_01", true);
    }
    
    public void SaveGameDataToCurrentCharacterData(ref SaveGameData currentGameData)
    {
        currentGameData.characterName = playerVariableManager.characterName.Value;
        
         currentGameData.balance = WorldPlayerInventory.Instance.balance.Value;

         currentGameData.curHealthPercent = (float)playerVariableManager.health.Value / playerVariableManager.health.MaxValue;
         
         currentGameData.secondsPlayed = WorldTimeManager.Instance.GetPlayedDate(); 
         
         // 아이템 저장 
         currentGameData.backpackItems.Clear();
         currentGameData.inventoryItems.Clear(); 
         currentGameData.safeItems.Clear();
         currentGameData.quickSlotConsumableItems.Clear();
         
         // Dictionary 직렬화 
         
         foreach (var pair in WorldPlayerInventory.Instance.GetConsumableInventory().GetCurItemDictById())
         {
             currentGameData.quickSlotConsumableItems.Add(pair.Key, pair.Value);
         }
         
         foreach (var pair in WorldPlayerInventory.Instance.GetSafeInventory().GetCurItemDictById())
         {
             currentGameData.safeItems.Add(pair.Key, pair.Value);
         }
         
         foreach (var pair in WorldPlayerInventory.Instance.GetInventory().GetCurItemDictById())
         {
             currentGameData.inventoryItems.Add(pair.Key, pair.Value);
         }
         
         foreach (var pair in WorldPlayerInventory.Instance.GetBackpackInventory().GetCurItemDictById())
         {
             currentGameData.backpackItems.Add(pair.Key, pair.Value);
         }
         
         // 장비 
         currentGameData.weaponItemCode = playerVariableManager.currentEquippedWeaponID.Value;
         currentGameData.helmetItemCode = playerVariableManager.currentHelmetID.Value;
         currentGameData.armorItemCode = playerVariableManager.currentArmorID.Value;
         
         // 저장공간 저장
         currentGameData.weaponBoxSize = WorldPlayerInventory.Instance.GetWeaponInventory().GetCurItemGridSize();
         currentGameData.helmetBoxSize = WorldPlayerInventory.Instance.GetHelmetInventory().GetCurItemGridSize();
         currentGameData.armorBoxSize = WorldPlayerInventory.Instance.GetArmorInventory().GetCurItemGridSize();
         currentGameData.consumableBoxSize = WorldPlayerInventory.Instance.GetConsumableInventory().GetCurItemGridSize();
         currentGameData.backpackSize = WorldPlayerInventory.Instance.GetBackpackInventory().GetCurItemGridSize();
         currentGameData.inventoryBoxSize = WorldPlayerInventory.Instance.GetInventory().GetCurItemGridSize();
         currentGameData.safeBoxSize = WorldPlayerInventory.Instance.GetSafeInventory().GetCurItemGridSize();
         
         // 마지막 플레이 타임 저장 
         currentGameData.lastPlayTime = DateTime.Now.ToString("o");
    }
    
    public void LoadGameDataFromCurrentCharacterDataFirst(ref SaveGameData currentGameData)
    {
        // 게임 시간 설정 : 처음 로드시 Shelter에서 스폰 (shelter는 스폰시 다음 시간대로 넘어감 )
        WorldTimeManager.Instance.LoadGameDate(currentGameData.secondsPlayed -1);
        // First Frame에서는 싱글톤에 저장되는 내용들 ex) ui로 표시되는 아이템
        WorldPlayerInventory.Instance.balance.Value = currentGameData.balance;
        // 공유 인벤토리 
        WorldPlayerInventory.Instance.GetShareInventory().UpdateItemGridSize(currentGameData.shareBoxSize);
        foreach (KeyValuePair<int,int> item in currentGameData.shareInventoryItems)
        {
            for (int i = 0; i < item.Value; i++)
            {
                ItemInfo itemInfoData = WorldDatabase_Item.Instance.GetItemByID(item.Key);
                if (!WorldPlayerInventory.Instance.ReloadItemShareBox(itemInfoData))
                {
                    Debug.LogWarning("Reload Error");
                }
            }
        }
        
        // 일반 인벤토리 
        WorldPlayerInventory.Instance.GetInventory().UpdateItemGridSize(currentGameData.inventoryBoxSize);
        foreach (KeyValuePair<int,int> item in currentGameData.inventoryItems)
        {
            for (int i = 0; i < item.Value; i++)
            {
                ItemInfo itemInfoData = WorldDatabase_Item.Instance.GetItemByID(item.Key);
                if (!WorldPlayerInventory.Instance.ReloadItemInventory(itemInfoData))
                {
                    Debug.LogWarning("Reload Error");
                }
            }
        }
        // 가방 인벤토리 
        WorldPlayerInventory.Instance.GetBackpackInventory().UpdateItemGridSize(currentGameData.backpackSize);
        foreach (KeyValuePair<int,int> item in currentGameData.backpackItems)
        {
            for (int i = 0; i < item.Value; i++)
            {
                ItemInfo itemInfoData = WorldDatabase_Item.Instance.GetItemByID(item.Key);
                if (!WorldPlayerInventory.Instance.ReloadItemBackpack(itemInfoData))
                {
                    Debug.LogWarning("Reload Error");
                }
            }
        }
        // 금고 인벤토리 
        WorldPlayerInventory.Instance.GetSafeInventory().UpdateItemGridSize(currentGameData.safeBoxSize);
        foreach (KeyValuePair<int,int> item in currentGameData.safeItems)
        {
            for (int i = 0; i < item.Value; i++)
            {
                ItemInfo itemInfoData = WorldDatabase_Item.Instance.GetItemByID(item.Key);
                if (!WorldPlayerInventory.Instance.ReloadItemSafe(itemInfoData))
                {
                    Debug.LogWarning("Reload Error");
                }
            }
        }
        // QuickSlot 인벤토리 
        WorldPlayerInventory.Instance.GetConsumableInventory().UpdateItemGridSize(currentGameData.consumableBoxSize);
        foreach (KeyValuePair<int,int> item in currentGameData.quickSlotConsumableItems)
        {
            for (int i = 0; i < item.Value; i++)
            {
                if (!WorldPlayerInventory.Instance.ReloadItemQuickSlot(WorldDatabase_Item.Instance.GetItemByID(item.Key)))
                {
                    Debug.LogError("Reload Error");
                }
            }
        }

        // 장비
        WorldPlayerInventory.Instance.GetWeaponInventory().UpdateItemGridSize(currentGameData.weaponBoxSize);
        ItemInfo weaponItemInfoData = WorldDatabase_Item.Instance.GetItemByID(currentGameData.weaponItemCode);
        if (!WorldPlayerInventory.Instance.ReloadItemWeapon(weaponItemInfoData))
        {
            Debug.LogError("Reload Error");
        }
        WorldPlayerInventory.Instance.GetHelmetInventory().UpdateItemGridSize(currentGameData.helmetBoxSize);
        ItemInfo helmetItemInfoData = WorldDatabase_Item.Instance.GetItemByID(currentGameData.helmetItemCode);
        if (!WorldPlayerInventory.Instance.ReloadItemHelmet(helmetItemInfoData))
        {
            Debug.LogError("Reload Error");
        }
        WorldPlayerInventory.Instance.GetArmorInventory().UpdateItemGridSize(currentGameData.armorBoxSize);
        ItemInfo armorItemInfoData = WorldDatabase_Item.Instance.GetItemByID(currentGameData.armorItemCode);
        if (!WorldPlayerInventory.Instance.ReloadItemArmor(armorItemInfoData))
        {
            Debug.LogError("Reload Error");
        }
        
    }

    public void LoadGameDataFromCurrentCharacterDataSceneChange(ref SaveGameData currentGameData)
    {
        playerVariableManager.characterName.Value = currentGameData.characterName;
        
        var curHealthValue = (int)(playerVariableManager.health.MaxValue * currentGameData.curHealthPercent);
        playerVariableManager.health.Value = curHealthValue;
        playerStatsManager.SetNewHealthPoint(curHealthValue); // 처음 로드시 ui 갱신
        
        playerVariableManager.currentEquippedWeaponID.Value = currentGameData.weaponItemCode;
        playerVariableManager.currentHelmetID.Value = currentGameData.helmetItemCode;
        playerVariableManager.currentArmorID.Value = currentGameData.armorItemCode;
        
        
        playerVariableManager.currentQuickSlotIDList.Clear();
        foreach (KeyValuePair<int,int> item in currentGameData.quickSlotConsumableItems)
        {
            for(var i = 0; i < item.Value; i++)
                playerVariableManager.currentQuickSlotIDList.Add(item.Key);
        }
        
        LoadPerkData(ref currentGameData);
        playerVariableManager.ResetStatus(); // 로드시 헬멧 없으면 actionPoint 초기화 안되는 문제 발생 
    }

    public void LoadPerkData(ref SaveGameData currentGameData)
    {
        // PERK : Hand
        playerVariableManager.perkThirdCombo.Value = currentGameData.unlockPerkList.Get(110);
        playerVariableManager.perkPerryWithCriticalAttack.Value = currentGameData.unlockPerkList.Get(120);
        playerVariableManager.perkJumpAttack.Value = currentGameData.unlockPerkList.Get(122);
        playerVariableManager.perkBackStepAttack.Value = currentGameData.unlockPerkList.Get(123);
        playerVariableManager.perkExtraAttack.Value = currentGameData.unlockPerkList.Get(130);
        // PERK : Head
        playerVariableManager.perkHealByFullness.Value = currentGameData.unlockPerkList.Get(210);
        playerVariableManager.perkLastSpurt.Value = currentGameData.unlockPerkList.Get(220);
        playerVariableManager.perkExtraActionPoint.Value = currentGameData.unlockPerkList.Get(230);
        playerVariableManager.perkExtraHealthPoint.Value = currentGameData.unlockPerkList.Get(232);
        // PERK : Body
        playerVariableManager.perkDoubleJump.Value = currentGameData.unlockPerkList.Get(310);
        playerVariableManager.perkExtraDefence.Value = currentGameData.unlockPerkList.Get(320);
        playerVariableManager.perkExtraMagicGuard.Value = currentGameData.unlockPerkList.Get(330);
        // PERK : Inventory
        playerVariableManager.perkSafeActive.Value = currentGameData.unlockPerkList.Get(410);
        playerVariableManager.perkExtraWeight.Value = currentGameData.unlockPerkList.Get(422);
    }

    protected override void ActivateTrail()
    {
        playerVariableManager.isTrailActive.Value = playerVariableManager.isInvulnerable.Value ||
            (characterController.velocity.magnitude >= playerVariableManager.CLVM.sprintSpeed);
    }
}