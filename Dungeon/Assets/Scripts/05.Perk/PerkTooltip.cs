using UnityEngine;
using TMPro;
public class PerkTooltip : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    
    [SerializeField] private TextMeshProUGUI perkName;
    [SerializeField] private TextMeshProUGUI perkTier;
    [SerializeField] private TextMeshProUGUI perkCost;
    [SerializeField] private TextMeshProUGUI perkDescription;

    public void Init(Perk perk)
    {
        if(!perk) return;
        perkName.text = perk.perkName;
        perkTier.text = $"Tier {perk.PerkTier}";
        perkCost.text = perk.cost.ToString();
        perkDescription.text = perk.perkDescription;
    }

    public void ToggleTooltip(bool isActive)
    {
        canvasGroup.alpha = isActive ? 1 : 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

}
