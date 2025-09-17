using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerEquipmentManger : CharacterEquipmentMangaer
{
    PlayerManager player;

    [SerializeField] private List<GameObject> armorList;
    [SerializeField] private ModelInstantiateSlot helmetSlot;
    [SerializeField] private ModelInstantiateSlot backpackSlot;
    [SerializeField] private WeaponModelInstantiateSlot rightHandSlot;
    [SerializeField] private WeaponModelInstantiateSlot leftChainsawSlot;
    private IWeaponManager _weaponManager;
    
    [HideInInspector] public EquipmentItemInfoWeapon currentEquippedInfoWeapon;
    [HideInInspector] public EquipmentItemInfoHelmet currentEquippedInfoHelmet;
    [HideInInspector] public EquipmentItemInfoArmor currentEquippedInfoArmor;

    private GameObject _helmetModel;
    private GameObject _backpackModel;
    private GameObject _weaponModel;

    protected void Awake()
    {
        player = GetComponent<PlayerManager>();
        InitializeWeaponSlots();
    }
    
    private void InitializeWeaponSlots()
    {
        WeaponModelInstantiateSlot[] weaponSlots = GetComponentsInChildren<WeaponModelInstantiateSlot>();

        foreach (var weaponSlot in weaponSlots)
        {
            switch (weaponSlot.weaponSlot)
            {
                case WeaponModelSlot.RightHand:
                    rightHandSlot = weaponSlot;
                    break;
                case WeaponModelSlot.LeftHand:
                    break;
                case WeaponModelSlot.LeftChainsaw:
                    leftChainsawSlot = weaponSlot;
                    break;
                default:
                    break;
            }
        }
    }

    public void LoadRightWeapon()
    {
        if (currentEquippedInfoWeapon == null)
        {
            rightHandSlot.UnloadModel();
            leftChainsawSlot.UnloadModel();
            return;
        }
        
        rightHandSlot.UnloadModel();
        leftChainsawSlot.UnloadModel();
        
        _weaponModel = Instantiate(currentEquippedInfoWeapon.itemModel);
        rightHandSlot.LoadModel(_weaponModel);
        _weaponManager = _weaponModel.GetComponent<IWeaponManager>();
        _weaponManager.SetWeapon(player, currentEquippedInfoWeapon);
        player.playerAnimatorManager.UpdateAnimatorController(currentEquippedInfoWeapon.weaponAnimator);
    }

    public void LoadHelmet()
    {
        helmetSlot.UnloadModel();
        if(currentEquippedInfoHelmet == null) return;
        _helmetModel = Instantiate(currentEquippedInfoHelmet.itemModel);
        helmetSlot.LoadModel(_helmetModel);
    }

    public void LoadBackpack()
    {
        backpackSlot.UnloadModel();
        if(currentEquippedInfoArmor == null || currentEquippedInfoArmor.itemModel == null) return;
        _backpackModel = Instantiate(currentEquippedInfoArmor.itemModel);
        backpackSlot.LoadModel(_backpackModel);
    }
    
    public void SetArmor()
    {
        if(armorList == null || armorList.Count == 0) return; 
        foreach (var armor in armorList)
        {
            armor.SetActive(false);
        }

        var armorIndex = currentEquippedInfoArmor ? currentEquippedInfoArmor.itemCode % 10 : 0;
        
        armorList[armorIndex].SetActive(true);
    }
    
    #region AnimationEvent
    
    public void LoadChainsaw()
    {
        if(currentEquippedInfoWeapon == null) return;
        
        rightHandSlot.UnloadModel();
        leftChainsawSlot.UnloadModel();
        
        _weaponModel = Instantiate(currentEquippedInfoWeapon.itemModel);
        leftChainsawSlot.LoadModel(_weaponModel);
        _weaponManager = _weaponModel.GetComponent<IWeaponManager>();
        _weaponManager.SetWeapon(player, currentEquippedInfoWeapon);
        player.playerAnimatorManager.UpdateAnimatorController(currentEquippedInfoWeapon.weaponAnimator);
    }
    
    public void OpenDamageCollider()
    {
        _weaponManager.OpenDamageCollider();
        player.playerSoundFXManager.PlaySoundFX(WorldSoundFXManager.Instance.ChooseSwordSwingSfx());
    }

    public override void CloseDamageCollider()
    {
        _weaponManager?.CloseDamageCollider();
    }

    public void OpenBlock()
    {
        player.playerVariableManager.isBlock.Value = true;
    }
    
    public void CloseBlock()
    {
        player.playerVariableManager.isBlock.Value = false;
    }

    public void OpenParring()
    {
        player.playerVariableManager.isParring.Value = true;
    }
    
    public void CloseParring()
    {
        player.playerVariableManager.isParring.Value = false;
    }

    #endregion
    
}
