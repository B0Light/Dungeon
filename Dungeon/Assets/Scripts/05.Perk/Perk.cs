using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Perks/BasePerk")]
public class Perk : ScriptableObject
{
    public int perkId;
    public Sprite perkIcon;
    
    public String perkName;
    public String perkDescription;
    public int cost = 1;

    [SerializeField] private List<int> costItemIdList = new List<int>();

    private int _perkTier = -1; // 초기값 -1 (계산되지 않음)
    public int PerkTier
    {
        get
        {
            if (_perkTier == -1) // 아직 계산되지 않았다면
            {
                _perkTier = perkId / 10 % 10; // 10의 자리수 계산
            }
            return _perkTier;
        }
    }


    private bool _isSetRequiredId = false;
    private int _requiredPerkId = 0;
    public int RequiredPerkId
    {
        get
        {
            if (!_isSetRequiredId)
            {
                _requiredPerkId = GetRequiredPerkId(); // 값 계산 후 저장
                _isSetRequiredId = true; // 플래그 업데이트
            }
            return _requiredPerkId;
        }
    }
    private PerkNode _perkNode;

    public virtual void Init(PerkNode setPerkNode)
    {
        _perkNode = setPerkNode;
        GetRequiredPerkId();
        _perkNode.SetSelect(WorldSaveGameManager.Instance.currentGameData.unlockPerkList.Get(perkId));
    }
    
    private int GetRequiredPerkId()
    {
        // requiredPerk ID
        int onesDigit = perkId % 10; 
        _requiredPerkId = perkId % 10 == 0 ? perkId - 10 : perkId - onesDigit;
        if (_requiredPerkId / 10 % 10 == 0) _requiredPerkId = 0;
        _isSetRequiredId = true;
        return _requiredPerkId;
    }

    public virtual bool AcquirePerk()
    {
        // 해당 id에 해당하는 특전이 이미 해금 된 경우 리턴
        if(WorldSaveGameManager.Instance.currentGameData.unlockPerkList.Get(perkId))
        {
            GUIController.Instance.playerUIPopUpManager.SendShopMessagePopUp("이미 해금된 특전입니다.");
            return false;
        }
        // 해당 특전의 선행 특전의 활성화 상태 확인 
        if (WorldSaveGameManager.Instance.currentGameData.unlockPerkList.Get(RequiredPerkId) == false)
        {
            GUIController.Instance.playerUIPopUpManager.SendShopMessagePopUp("선행 특전이 해금되지 않았습니다.");
            return false;
        }
        if(BuyProcess() == false)
        {
            GUIController.Instance.playerUIPopUpManager.SendShopMessagePopUp("특전 해금에 필요한 재료가 없습니다.");
            return false;
        }
        
        // 현재 저장 데이터에 바로 unlock를 씀
        WorldSaveGameManager.Instance.currentGameData.unlockPerkList.Set(perkId, true);
        _perkNode.SetSelect(true);
        return true;
    }
    

    private bool BuyProcess()
    {
        ItemGrid itemGrid = WorldPlayerInventory.Instance.GetInventory();

        return itemGrid.RemoveItems(costItemIdList);
    }
}
