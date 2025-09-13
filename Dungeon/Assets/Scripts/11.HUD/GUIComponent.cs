using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class GUIComponent : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    protected virtual void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void OpenGUI()
    {
        PlayerInputManager.Instance.SetControlActive(false);
        ToggleGUI(true);
    }

    public virtual void CloseGUI()
    {
        PlayerInputManager.Instance.SetControlActive(true);
        ToggleGUI(false);
    }

    private void ToggleGUI(bool value)
    {
        _canvasGroup.alpha = value ? 1 : 0;
        _canvasGroup.interactable = value;
        _canvasGroup.blocksRaycasts = value;
    }

    public virtual void SelectNextGUI() { }
}
