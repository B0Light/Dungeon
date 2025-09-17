using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerVariableManager : CharacterVariableManager
{
    private PlayerManager _playerManager;

    public Variable<string> characterName = new Variable<string>("Character");
    [Header("Controls")] 
    public Variable<bool> canControl = new Variable<bool>(true);

    [Header("Status")] 
    public ClampedVariable<int> hungerLevel;
    public Variable<bool> isHungry = new Variable<bool>(false);
    public float moveCoefficientByHungry = 1f;

    public float playerWeight = 0;
    public float moveCoefficientByWeight = 1f;
        
    [Header("Equipment_Weapon")] 
    public Variable<int> currentEquippedWeaponID = new Variable<int>(0);
    
    [Header("Equipment_Helmet")]
    public Variable<int> currentHelmetID = new Variable<int>(0);
    
    [Header("Equipment_Armor")]
    public Variable<int> currentArmorID = new Variable<int>(0);
    
    [Header("Equipment_QuickSlot")]
    public VariableList<int> currentQuickSlotIDList = new VariableList<int>();
    public Variable<int> currentSelectQuickSlotItem = new Variable<int>(0);
    
    [Header("Perk Unlock Safe")] 
    public Variable<bool> perkSafeActive = new Variable<bool>(false);
    
    [Header("Perk Unlock Actions")] 
    public Variable<bool> perkThirdCombo = new Variable<bool>(false);
    public Variable<bool> perkHealByFullness = new Variable<bool>(false);
    private Coroutine _regenCoroutine;
    
    public Variable<bool> perkLastSpurt = new Variable<bool>(false);
    private bool _enableLastSpurt = false;

    public Variable<bool> perkPerryWithCriticalAttack = new Variable<bool>(false);
    public Variable<bool> perkJumpAttack = new Variable<bool>(false);
    public Variable<bool> perkBackStepAttack = new Variable<bool>(false);
    public Variable<bool> perkDoubleJump = new Variable<bool>(false);
    
    [Header("Perk System : extra stat")] 
    public Variable<bool> perkExtraAttack = new Variable<bool>(false);
    public Variable<bool> perkExtraDefence = new Variable<bool>(false);
    public Variable<bool> perkExtraMagicGuard = new Variable<bool>(false);
    public Variable<bool> perkExtraActionPoint = new Variable<bool>(false);
    public Variable<bool> perkExtraHealthPoint = new Variable<bool>(false);
    public Variable<bool> perkExtraWeight = new Variable<bool>(false);
        
    private readonly int _isEquippedHash = Animator.StringToHash("CurEquippedWeapon");
    private readonly int _attackSpeedHash = Animator.StringToHash("AttackSpeed");
    protected override void Awake()
    {
        base.Awake();

        _playerManager = GetComponent<PlayerManager>();
    }

    public override void InitVariable()
    {
        base.InitVariable();
        hungerLevel = new ClampedVariable<int>(100);
    }
    
    // hp == 0
    public override void DeathProcess(int newValue)
    {
        if (perkLastSpurt.Value && _enableLastSpurt)
        {
            _enableLastSpurt = false;
            health.Value = health.MaxValue / 2;
            hungerLevel.Value = (int)(hungerLevel.MaxValue * 0.6f);
        }
        else
        {
            base.DeathProcess(newValue);
        }
        
    }

    public void OnControlChange(bool newValue)
    {
        PlayerCameraController.Instance.SetCameraControllerEnable(newValue);
    }
    // isDead = true
    public void OnPlayerDeath(bool newValue)
    {
        Debug.LogWarning("PLAYER DEATH");
        if(!newValue) return; // 사망 -> 생존
        GUIController.Instance.CloseGUI();
        currentEquippedWeaponID.Value = 0;
        currentHelmetID.Value = 0;
        currentArmorID.Value = 0;
        currentQuickSlotIDList.Clear();
        WorldPlayerInventory.Instance.GetWeaponInventory().ResetItemGrid();
        WorldPlayerInventory.Instance.GetHelmetInventory().ResetItemGrid();
        WorldPlayerInventory.Instance.GetArmorInventory().ResetItemGrid();
        WorldPlayerInventory.Instance.GetConsumableInventory().ResetItemGrid();
        WorldPlayerInventory.Instance.GetInventory().ResetItemGrid();

        health.MaxValue = initialMaxHealth;
        health.Value = 5;
        actionPoint.MaxValue = initialActionPoint;
        WorldSaveGameManager.Instance.SaveGame();
    }

    public void SetNewMaxHealthPoint(int newValue)
    {
        // 실제 채력을 변경하지 않는 이유는 탈출맵에서 갑옷을 바꿔 끼는 행위로 채력변동을 허용하지 않음 
        GUIController.Instance.playerUIHudManager.playerUIStatusManager.SetMaxHealthValue(newValue);
        GUIController.Instance.playerUIHudManager.playerUIStatusManager.SetNewHealthValue(health.Value);
    }

    public void SetNewMaxActionPoint(int newValue)
    {
        GUIController.Instance.playerUIHudManager.playerUIStatusManager.SetMaxActionPoint(newValue);
    }

    public float GetMoveCoefficient()
    {
        return moveCoefficientByHungry * moveCoefficientByWeight;
    }

    public void UpdatePlayerWeight(float newValue)
    {
        playerWeight = newValue;
        CalculateWeightCoefficient();
    }

    public void CalculateWeightCoefficient()
    {
        float ratio = playerWeight / WorldPlayerInventory.Instance.itemWeight.MaxValue;

        float value = (ratio <= 0.7f) 
            ? 1f 
            : 1f - 0.5f * Mathf.Clamp01((ratio - 0.7f) / 0.3f);

        moveCoefficientByWeight = Mathf.Clamp(value, 0.5f, 1f);
    }

    public void CurrentEquippedWeaponIDChange(int newValue)
    {
        EquipmentItemInfoWeapon newWeaponInfo = Instantiate((EquipmentItemInfoWeapon)WorldDatabase_Item.Instance.GetItemByID(newValue));
        _playerManager.playerEquipmentManger.currentEquippedInfoWeapon = newWeaponInfo;
        _playerManager.playerEquipmentManger.LoadRightWeapon();
        _playerManager.animator.SetInteger(_isEquippedHash, newValue);
        _playerManager.animator.SetFloat(_attackSpeedHash, newWeaponInfo.attackSpeed);

        _playerManager.playerStatsManager.blockingPhysicalAbsorption = newWeaponInfo.physicalDamageAbsorption;
        _playerManager.playerStatsManager.blockingMagicalAbsorption = newWeaponInfo.magicalDamageAbsorption;
        _playerManager.playerStatsManager.blockingStability = newWeaponInfo.stability;
        
        GUIController.Instance.playerUIHudManager.playerUIWeaponSlotManager.SetRightWeaponQuickSlotIcon(newValue);
    }

    public void CurrentEquippedHelmetIDChange(int newValue)
    {
        EquipmentItemInfoHelmet newHelmetInfo = 
            newValue != 0 ? Instantiate((EquipmentItemInfoHelmet)WorldDatabase_Item.Instance.GetItemByID(newValue)) : null;
        _playerManager.playerEquipmentManger.currentEquippedInfoHelmet = newHelmetInfo;
        _playerManager.playerEquipmentManger.LoadHelmet();
        
        ResetActionPoint(newHelmetInfo);
    }

    // ResetActionPoint: actionPoint를 초기 상태로 리셋
    private void ResetActionPoint(EquipmentItemInfoHelmet helmet)
    {
        actionPoint.MaxValue = initialActionPoint + (perkExtraHealthPoint.Value ? 1 : 0) + (helmet == null ? 0 : helmet.extraActionPoint);
        _playerManager.characterStatsManager.extraPhysicalAbsorption = helmet == null ? 0 : helmet.extraPhysicalAbsorption;
        _playerManager.characterStatsManager.extraMagicalAbsorption = helmet== null ? 0 : helmet.extraMagicalAbsorption;
    }
    
    public void ResetStatus()
    {
        var helmet = _playerManager.playerEquipmentManger.currentEquippedInfoHelmet;
        actionPoint.MaxValue = initialActionPoint + (perkExtraActionPoint.Value ? 1 : 0) + (helmet == null ? 0 : helmet.extraActionPoint);
        _playerManager.playerStatsManager.extraPhysicalAbsorption = helmet == null ? 0 : helmet.extraPhysicalAbsorption;
        _playerManager.playerStatsManager.extraMagicalAbsorption = helmet== null ? 0 : helmet.extraMagicalAbsorption;
        
        // var armor = _playerManager.playerInventoryManager.currentEquippedInfoArmor;
        // health.MaxValue = initialMaxHealth + (perkExtraHealthPoint.Value ? 100 : 0) + (armor == null ? 0 : armor.extraHealth);
    }
    
    public void CurrentEquippedArmorIDChange(int newValue)
    {
        float curHealthPercent = (float)health.Value / health.MaxValue;
    
        // 갑옷 정보 가져오기 (없으면 null)
        EquipmentItemInfoArmor newArmor = null;
        if (newValue != 0)
        {
            newArmor = WorldDatabase_Item.Instance.GetItemByID(newValue) as EquipmentItemInfoArmor;
        }

        _playerManager.playerEquipmentManger.currentEquippedInfoArmor = newArmor;
        _playerManager.playerEquipmentManger.LoadBackpack();
        _playerManager.playerEquipmentManger.SetArmor();
        /* MAX HEALTH */
        // 최대 체력 업데이트
        int baseHealth = initialMaxHealth + (perkExtraHealthPoint.Value ? 100 : 0);
        health.MaxValue = baseHealth + (newArmor != null ? newArmor.extraHealth : 0);
    
        // 현재 체력 비율 유지
        health.Value = (int)(health.MaxValue * curHealthPercent);
        
        /* BACKPACK */
        // GUI 활성화 
        if (newArmor != null && newArmor.backpackSize != Vector2Int.zero)
        {
            GUIController.Instance.inventoryGUIManager.ToggleBackpackInventory(true);
            GUIController.Instance.inventoryGUIManager.backpackItemGrid.UpdateItemGridSize(newArmor.backpackSize);
        }
        else
        {
            GUIController.Instance.inventoryGUIManager.backpackItemGrid.UpdateItemGridSize(Vector2Int.zero);
            GUIController.Instance.inventoryGUIManager.ToggleBackpackInventory(false);
        }
        
    }

    public void OnAddQuickSlotItem(int itemID)
    {
        GUIController.Instance.playerUIHudManager.playerUIQuickSlotManager.AddQuickSlotItem(itemID);
        // 현재 퀵슬롯 (물약 창)이 비었을 때 새로운 아이템을 넣으면 해당 아이템을 퀵슬롯에 등록한다.
        if (currentSelectQuickSlotItem.Value == 0)
        {
            currentSelectQuickSlotItem.Value = itemID;
        }
    }
    
    public void OnRemoveQuickSlotItem(int itemID)
    {
        // 플레이어 HUD 에서 해당 아이템 제거 
        GUIController.Instance.playerUIHudManager.playerUIQuickSlotManager.RemoveQuickSlotItem(itemID);
        // 현재 퀵슬롯에 등록된 아이템이 사용된 아이템인지 확인 (인벤토리에서 바로 사용할 수도 있음)
        if (currentSelectQuickSlotItem.Value == itemID)
        {
            // 퀵슬롯에 해당아이템의 여분이 남아있지 않다면 남은 퀵슬롯 아이템으로 퀵슬롯 대체
            if (!currentQuickSlotIDList.Contains(itemID))
            {
                currentSelectQuickSlotItem.Value = currentQuickSlotIDList.Count == 0 ? 0 : currentQuickSlotIDList[0];
            }
        }
    }
    
    public void OnQuickSlotClear()
    {
        GUIController.Instance.playerUIHudManager.playerUIQuickSlotManager.ClearQuickSlot();
        currentSelectQuickSlotItem.Value = 0;
    }

    public void OnSelectQuickSlotItemChange(int itemID)
    {
        GUIController.Instance.playerUIHudManager.playerUIQuickSlotManager.ChangeQuickSlotItem(itemID);
    }
    
    public void OnHealByFullness(bool value)
    {
        if(!value)
        {
            if (_regenCoroutine == null) return;
            
            StopCoroutine(HealByFullnessCoroutine());
            _regenCoroutine = null;
        }
        else
        {
            // ??= : 변수가 null 이면 대입 아니면 무시
            _regenCoroutine ??= StartCoroutine(HealByFullnessCoroutine());
        }
    }

    private IEnumerator HealByFullnessCoroutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1f);
            if (!(health.Value < health.MaxValue && !isHungry.Value)) yield break;
            
            health.Value += 1;
            hungerLevel.Value -= 1;
        }
    }

    public override void SuccessParry(CharacterManager parriedTarget)
    {
        base.SuccessParry(parriedTarget);
        if (perkPerryWithCriticalAttack.Value)
        {
            _playerManager.playerCombatManager.canCriticalAttack = true;
            _playerManager.playerCombatManager.criticalDamagedCharacter = parriedTarget;
            StartCoroutine(DisableCriticalAttack());
        }
    }

    private IEnumerator DisableCriticalAttack()
    {
        yield return new WaitForSeconds(3f);
        _playerManager.playerCombatManager.canCriticalAttack = false;
    }

    public void OnExtraAttackChange(bool isActivated)
    {
        _playerManager.playerStatsManager.extraDamage.Value += isActivated ? 30 : -30;
    }
    
    public void OnExtraDefenceChange(bool isActivated)
    {
        _playerManager.playerStatsManager.basePhysicalAbsorption += isActivated ? 10 : -10;
    }
    
    public void OnExtraMagicGuardChange(bool isActivated)
    {
        _playerManager.playerStatsManager.baseMagicalAbsorption += isActivated ? 50 : -50;
    }
    
    public void OnExtraHealthPointChange(bool isActivated)
    {
        float healthPercent = (float)health.Value / health.MaxValue;
        health.MaxValue += isActivated ? 100 : -100;
        health.Value = (int)(health.MaxValue * healthPercent);
    }
    
    public void OnExtraActionPointChange(bool isActivated)
    {
        actionPoint.MaxValue += isActivated ? 1 : -1;
    }

    public void OnExtraWeight(bool isActivated)
    {
        WorldPlayerInventory.Instance.itemWeight.MaxValue += 250;
    }
}

