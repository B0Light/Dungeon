using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class PlayerUIWeaponSlotManager : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private Image rightWeaponQuickSlotIcon;
    [SerializeField] private Image weaponDurabilityEffect;
    
    [Header("Default Image")] 
    [SerializeField] private Sprite defaultSprite;
    
    public void SetRightWeaponQuickSlotIcon(int weaponID)
    {
        EquipmentItemInfoWeapon weaponItemInfo = (EquipmentItemInfoWeapon)WorldDatabase_Item.Instance.GetItemByID(weaponID);

        if (weaponItemInfo == null)
        {
            Debug.Log("ITEM IS NULL");
            rightWeaponQuickSlotIcon.enabled = false;
            rightWeaponQuickSlotIcon.sprite = defaultSprite;
            return;
        }

        if(weaponItemInfo.itemIcon == null)
        {
            Debug.Log("ITEM HAS NO ICON");
            rightWeaponQuickSlotIcon.enabled = false;
            rightWeaponQuickSlotIcon.sprite = defaultSprite;
            return;
        }

        rightWeaponQuickSlotIcon.enabled = true;
        rightWeaponQuickSlotIcon.sprite = weaponItemInfo.weaponEquipSprite;
    }

    public void GlowWeaponSlot()
    {
        Color targetColor = new Color(238/255f,88/255f,86/255f, 255);
        
        // 코루틴 실행
        StopAllCoroutines(); // 기존 효과 중단
        StartCoroutine(GlowEffect(targetColor, 1f));
    }

    private IEnumerator GlowEffect(Color targetColor, float time)
    {
        // 투명한 색상
        Color transparent = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);

        // 타겟 색상으로 변경 (투명 -> 타겟)
        float elapsedTime = 0f;
        while (elapsedTime < time / 2)
        {
            weaponDurabilityEffect.color = Color.Lerp(transparent, targetColor, elapsedTime / (time / 5));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 정확히 타겟 색상으로 설정
        weaponDurabilityEffect.color = targetColor;

        // 다시 투명으로 변경 (타겟 -> 투명)
        elapsedTime = 0f;
        while (elapsedTime < time / 2)
        {
            weaponDurabilityEffect.color = Color.Lerp(targetColor, transparent, elapsedTime / (time / 2));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 최종적으로 투명 색상으로 설정
        weaponDurabilityEffect.color = transparent;
    }
}
