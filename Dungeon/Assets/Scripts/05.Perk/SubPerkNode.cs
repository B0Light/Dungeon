using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SubPerkNode : PerkNode, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private bool _isActivate = false;
    public void OnPointerEnter(PointerEventData eventData)
    {
        perkGUIManager.ShowTooltip(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(_isActivate) return;
        StartCoroutine(WaitForSecond());
        if(!perk) return;
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (perk.AcquirePerk())
            {
                perkGUIManager.ActiveEffect();
            }
            
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (perk.RefundPerk())
            {
                perkGUIManager.RefundEffect();
            }
        }
    }

    private IEnumerator WaitForSecond()
    {
        _isActivate = true;
        yield return new WaitForSecondsRealtime(1f);
        _isActivate = false;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        perkGUIManager.HideTooltip();
    }
}
