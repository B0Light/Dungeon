using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractableResource : Interactable
{
    [SerializeField] private int interactableCount = 3;
    [SerializeField] private List<DropItem> _dropItemList = new List<DropItem>();
    [SerializeField] private bool removeOnInteractionComplete = false;
    
    public override void Interact(PlayerManager player)
    {
        if(interactableCount <= 0) return;
        base.Interact(player);

        interactableCount--;
        DropItemOnDamage();
        
        if(interactableCount > 0)
            ResetInteraction();
        else if (removeOnInteractionComplete)
        {
            gameObject.SetActive(false);
        }
    }
    
    private void DropItemOnDamage()
    {
        // 드롭할 아이템 리스트가 비어있으면 아이템 드롭을 수행하지 않음
        if (_dropItemList.Count == 0) return;

        // 확률 기반으로 드롭할 아이템 선택
        int selectedItemCode = GetRandomItemByChance();
        
        // 선택된 아이템이 없으면 (확률에 걸리지 않았으면) 드롭하지 않음
        if (selectedItemCode == -1) return;

        Vector3 spawnPos = GameManager.Instance.GetPlayer().transform.position + new Vector3(0, 1, 0.5f);
        GameObject item = Instantiate(WorldDatabase_Item.Instance.emptyInteractItemPrefab, spawnPos, Quaternion.identity);
        InteractableItem interactableItem = item.GetComponentInChildren<InteractableItem>();
        
        interactableItem.SetItemCode(selectedItemCode);
    }

    private int GetRandomItemByChance()
    {
        // 모든 아이템의 가중치 합계 계산
        float totalWeight = 0f;
        foreach (var dropItem in _dropItemList)
        {
            totalWeight += dropItem.dropRate;
        }

        // 가중치가 0 이하면 드롭 없음
        if (totalWeight <= 0f) return -1;

        // 0부터 전체 가중치 범위에서 랜덤 값 생성
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        
        // 누적 가중치로 아이템 선택
        float cumulativeWeight = 0f;
        foreach (var dropItem in _dropItemList)
        {
            cumulativeWeight += dropItem.dropRate;
            if (randomValue <= cumulativeWeight)
            {
                return dropItem.itemID;
            }
        }

        // 혹시 모를 경우를 대비해 마지막 아이템 반환
        return _dropItemList[_dropItemList.Count - 1].itemID;
    }
}