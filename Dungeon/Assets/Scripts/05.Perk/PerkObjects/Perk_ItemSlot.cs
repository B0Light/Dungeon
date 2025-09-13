using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Perks/ItemSlot")]
public class Perk_ItemSlot : Perk
{
    [SerializeField] private Vector2Int extraGridSize = new Vector2Int(1, 1);
    private ItemGrid _targetInventory;

    public override void Init(PerkNode setPerkNode)
    {
        base.Init(setPerkNode);
        SetTargetInventory();
    }

    public override bool AcquirePerk()
    {
        if (!base.AcquirePerk()) return false; // 해당 특전의 조건이 불충족 
        Vector2Int originGridSize = _targetInventory.GetCurItemGridSize();
        _targetInventory.UpdateItemGridSize(originGridSize + extraGridSize);
        return true;
    }

    public override bool RefundPerk()
    {
        if (!base.RefundPerk()) return false;
        Vector2Int originGridSize = _targetInventory.GetCurItemGridSize();
        _targetInventory.UpdateItemGridSize(originGridSize - extraGridSize);
        return true;
    }

    private void SetTargetInventory()
    {
        int targetValue = perkId / 100;

        switch (targetValue)
        {
            case 1 :
                _targetInventory = WorldPlayerInventory.Instance.GetWeaponInventory();
                break;
            case 2 :
                _targetInventory = WorldPlayerInventory.Instance.GetHelmetInventory();
                break;
            case 3 :
                _targetInventory = WorldPlayerInventory.Instance.GetArmorInventory();
                break;
            case 4:
                _targetInventory = perkId % 10 == 0 ? WorldPlayerInventory.Instance.GetSafeInventory() : WorldPlayerInventory.Instance.GetInventory();
                break;
            default:
                break;
        }
    }
}
