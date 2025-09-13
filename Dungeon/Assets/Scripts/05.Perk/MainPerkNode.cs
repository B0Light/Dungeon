using System.Collections.Generic;
using UnityEngine;

public class MainPerkNode : SubPerkNode
{
    [SerializeField] private List<GameObject> subPerks;
    
    protected override void SetNode()
    {
        base.SetNode();

        SetSubNode();
    }

    private void SetSubNode()
    {
        // 보조 특전유뮤 확인 
        if (!WorldDatabase_Perk.Instance.SubPerkDict.TryGetValuesByKey(perkId, out var values)) return;
        // 보조 특전 활성화
        var i = 0;
        foreach (var subPerkId in values)
        {
            subPerks[i].SetActive(true);
            StartCoroutine(subPerks[i].GetComponentInChildren<SubPerkNode>().Init(subPerkId, perkGUIManager));
            i++;
        }
    }
}
