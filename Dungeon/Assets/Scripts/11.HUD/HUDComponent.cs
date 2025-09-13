using UnityEngine;

public class HUDComponent : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    protected virtual void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void ActiveHUD()
    {
        ToggleHUD(true);
    }

    public virtual void DeactivateHUD()
    {
        ToggleHUD(false);
    }

    private void ToggleHUD(bool value)
    {
        _canvasGroup.alpha = value ? 1 : 0;
        _canvasGroup.interactable = value;
        _canvasGroup.blocksRaycasts = value;
    }
}
