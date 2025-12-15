using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionManager : MonoBehaviour
{
    private PlayerManager _player;
    
    private float _sphereRadius = 1f;
    private int _maxColliders = 10;
    private Collider[] _overlapResults;
    [SerializeField] private LayerMask interactableLayerMask;
    private Interactable _interactable;
    private List<Interactable> currentInteractableActions;
    
    private void Awake()
    {
        _player = GetComponent<PlayerManager>();
        _overlapResults = new Collider[_maxColliders];
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
            CastOverlapSphere();
            if (GUIController.Instance.currentOpenGUI == null && !GUIController.Instance.popUpWindowIsOpen)
            {
                CheckForInteractable();
            }
        }
    }
    
    private void CastOverlapSphere()
    {
        int numHits = Physics.OverlapSphereNonAlloc(
            _player.transform.position,
            _sphereRadius,
            _overlapResults,
            interactableLayerMask
        );

        List<Interactable> detectedInteractables = new List<Interactable>();

        for (int i = 0; i < numHits; i++)
        {
            Collider hitCollider = _overlapResults[i];
            Interactable interactableObject = hitCollider.GetComponentInParent<Interactable>();

            if (interactableObject != null)
            {
                detectedInteractables.Add(interactableObject); 
            }
        
            _overlapResults[i] = null; 
        }
    
        currentInteractableActions.Clear();

        foreach (var interactable in detectedInteractables)
        {
            AddInteractionToList(interactable);
        }
    
        if (currentInteractableActions.Count == 0)
        {
            GUIController.Instance.playerUIPopUpManager.CloseAllPopUpWindows();
        }
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

    private void AddInteractionToList(Interactable interactableObject)
    {
        if (!currentInteractableActions.Contains(interactableObject))
            currentInteractableActions.Add(interactableObject);
    }

    public void RemoveInteractionFromList(Interactable interactableObject)
    {
        if (currentInteractableActions.Contains(interactableObject))
            currentInteractableActions.Remove(interactableObject);

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
