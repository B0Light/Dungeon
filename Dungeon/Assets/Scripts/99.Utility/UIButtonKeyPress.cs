using UnityEngine;
using UnityEngine.UI;

public class UIButtonKeyPress : MonoBehaviour
{
    [SerializeField] private KeyCode bindKey;
    [SerializeField] private Button _targetButton;

    private void Start()
    {
        _targetButton = GetComponent<Button>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(bindKey)) 
        {
            if (_targetButton != null && _targetButton.interactable)
            {
                _targetButton.onClick.Invoke();
            }
            else
            {
                Debug.LogWarning("Not Interactable");
            }
        }
    }
}