using UnityEngine;
using UnityEngine.Serialization;

public class PlayerEquipmentManger : CharacterEquipmentMangaer
{
    PlayerManager player;

    [SerializeField] private ModelInstantiateSlot helmetSlot;
    [SerializeField] private ModelInstantiateSlot backpackSlot;
    [SerializeField] private WeaponModelInstantiateSlot rightHandSlot;
    [SerializeField] private WeaponModelInstantiateSlot leftChainsawSlot;
    private IWeaponManager _weaponManager;

    private GameObject _helmetModel;
    private GameObject _backpackModel;
    private GameObject _weaponModel;

    protected void Awake()
    {
        player = GetComponent<PlayerManager>();
        //InitializeWeaponSlots();
    }

    // 기존 item slot을 코드에서 자동으로 찾아 활성화 하는 코드 : but player 하나에만 적용 + 바인딩 할게 2개 뿐이라 그냥 인스펙터에서 관리 
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
        if (player.playerInventoryManager.currentEquippedInfoWeapon == null)
        {
            rightHandSlot.UnloadModel();
            leftChainsawSlot.UnloadModel();
            return;
        }
        
        rightHandSlot.UnloadModel();
        leftChainsawSlot.UnloadModel();
        
        _weaponModel = Instantiate(player.playerInventoryManager.currentEquippedInfoWeapon.itemModel);
        rightHandSlot.LoadModel(_weaponModel);
        _weaponManager = _weaponModel.GetComponent<IWeaponManager>();
        _weaponManager.SetWeapon(player, player.playerInventoryManager.currentEquippedInfoWeapon);
        player.playerAnimatorManager.UpdateAnimatorController(player.playerInventoryManager.currentEquippedInfoWeapon.weaponAnimator);
    }

    public void LoadHelmet()
    {
        helmetSlot.UnloadModel();
        if(player.playerInventoryManager.currentEquippedInfoHelmet == null) return;
        _helmetModel = Instantiate(player.playerInventoryManager.currentEquippedInfoHelmet.itemModel);
        helmetSlot.LoadModel(_helmetModel);
    }

    public void LoadBackpack()
    {
        backpackSlot.UnloadModel();
        if(player.playerInventoryManager.currentEquippedInfoArmor == null ||
           player.playerInventoryManager.currentEquippedInfoArmor.itemModel == null) return;
        _backpackModel = Instantiate(player.playerInventoryManager.currentEquippedInfoArmor.itemModel);
        backpackSlot.LoadModel(_backpackModel);
    }
    
    #region AnimationEvent
    
    public void LoadChainsaw()
    {
        if(player.playerInventoryManager.currentEquippedInfoWeapon == null) return;
        
        rightHandSlot.UnloadModel();
        leftChainsawSlot.UnloadModel();
        
        _weaponModel = Instantiate(player.playerInventoryManager.currentEquippedInfoWeapon.itemModel);
        leftChainsawSlot.LoadModel(_weaponModel);
        _weaponManager = _weaponModel.GetComponent<IWeaponManager>();
        _weaponManager.SetWeapon(player, player.playerInventoryManager.currentEquippedInfoWeapon);
        player.playerAnimatorManager.UpdateAnimatorController(player.playerInventoryManager.currentEquippedInfoWeapon.weaponAnimator);
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
