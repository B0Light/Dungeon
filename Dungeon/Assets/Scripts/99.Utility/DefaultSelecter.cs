using UnityEngine;
using UnityEngine.EventSystems;

public class DefaultSelecter : MonoBehaviour
{
    [SerializeField] private GameObject buttonToSelect;

    void Start()
    {
        EventSystem.current.SetSelectedGameObject(buttonToSelect);
    }
}
