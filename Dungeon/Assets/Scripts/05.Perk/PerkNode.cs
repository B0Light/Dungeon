using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PerkNode : MonoBehaviour
{
    [SerializeField] protected int perkId;
    protected Perk perk;
    [SerializeField] private Image icon;
    [SerializeField] private Image selectedFrame;
    [SerializeField] private Color selectedColor;
    [FormerlySerializedAs("perkHUDManager")] [SerializeField] protected PerkGUIManager perkGUIManager;
    
    public IEnumerator Init(int id, PerkGUIManager manager)
    {
        perkId = id;
        perkGUIManager = manager;
        yield return StartCoroutine(WaitForDataLoad());
        // id에 해당하는 특전이 없음 
        if (!WorldDatabase_Perk.Instance.PerkDict.TryGetValue(id, out perk))
        {
            this.gameObject.SetActive(false);
            yield break;
        }
        SetNode();
    }
    
    protected virtual void SetNode()
    {
        icon.sprite = perk.perkIcon;
        perk.Init(this);
    }
    
    protected IEnumerator WaitForDataLoad()
    {
        // 데이터가 로드될 때까지 대기
        while (!WorldDatabase_Perk.Instance.IsDataLoaded)
        {
            yield return null; // 한 프레임 대기
        }
    }
    

    public void SetSelect(bool isSelected)
    {
        selectedFrame.color = isSelected ? selectedColor : Color.white;
    }
    
    public Perk GetPerk() => perk;
}
