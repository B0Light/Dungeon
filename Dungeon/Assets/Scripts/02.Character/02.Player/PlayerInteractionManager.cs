using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionManager : MonoBehaviour
{
    private PlayerManager _player;
    public Interactable lastInteractable;
    
    private float _rayDistance = 6.5f; // 레이의 최대 거리
    [SerializeField] private LayerMask interactableLayerMask;
    private Interactable _interactable;
    private List<Interactable> currentInteractableActions;
    
    private void Awake()
    {
        _player = GetComponent<PlayerManager>();
    }

    private void Start()
    {
        currentInteractableActions = new List<Interactable>();
    }

    private void FixedUpdate()
    {
        if (_player.isDead.Value)
        {
            ResetInteraction();
        }
        else
        {
            CastRay();
            if (GUIController.Instance.currentOpenGUI == null && !GUIController.Instance.popUpWindowIsOpen)
            {
                CheckForInteractable();
            }
        }
    }
    
    private void CastRay()
    {
        if(Camera.main == null) return;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)); 
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * _rayDistance, Color.red);
        if (Physics.Raycast(ray, out hit, _rayDistance, interactableLayerMask))
        {
            _interactable = hit.collider.GetComponentInParent<Interactable>();
            if (_interactable != null)
            {
                AddInteractionToList(_interactable);
                return;
            }
        }
        else
        {
            currentInteractableActions.Clear();
        }

        if (_interactable != null)
        {
            RemoveInteractionFromList(_interactable);
            _interactable = null;
        }

        RefreshInteractionList();
        GUIController.Instance.playerUIPopUpManager.CloseAllPopUpWindows();
    }

    private void CheckForInteractable()
    {
        if (currentInteractableActions.Count == 0)
            return;

        if (currentInteractableActions[0] == null)
        {
            currentInteractableActions.RemoveAt(0); //  IF THE CURRENT INTERACTABLE ITEM AT POSITION 0 BECOMES NULL (REMOVED FROM GAME), WE REMOVE POSITION 0 FROM THE LIST
            return;
        }

        if (currentInteractableActions[0] != null)
        {
            InteractableItem item = currentInteractableActions[0] as InteractableItem;
            if (item)
            {
                ItemInfo itemInfo =  WorldDatabase_Item.Instance.GetItemByID(item.GetItemCode());
                GUIController.Instance.playerUIPopUpManager.OpenPlayerItemPickUpPopUp(itemInfo);
            }
            else
            {
                GUIController.Instance.playerUIPopUpManager.
                    SendPlayerMessagePopUp(currentInteractableActions[0].interactableText);
            }
        }
    }

    private void RefreshInteractionList()
    {
        for (int i = currentInteractableActions.Count - 1; i > -1; i--)
        {
            if (currentInteractableActions[i] == null)
                currentInteractableActions.RemoveAt(i);
        }
    }

    public void AddInteractionToList(Interactable interactableObject)
    {
        if (!currentInteractableActions.Contains(interactableObject))
            currentInteractableActions.Add(interactableObject);
    }

    public void RemoveInteractionFromList(Interactable interactableObject)
    {
        if (currentInteractableActions.Contains(interactableObject))
            currentInteractableActions.Remove(interactableObject);

        lastInteractable = interactableObject;
        RefreshInteractionList();
    }

    public void Interact()
    {
        if (currentInteractableActions.Count == 0)
            return;

        if (currentInteractableActions[0] != null)
        {
            currentInteractableActions[0].Interact(_player);
            RefreshInteractionList();
        }
    }

    private void ResetInteraction()
    {
        currentInteractableActions.Clear();
        GUIController.Instance.playerUIPopUpManager.CloseAllPopUpWindows();
    }
}
