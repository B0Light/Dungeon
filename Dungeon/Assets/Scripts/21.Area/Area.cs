using UnityEngine;

public class Area : MonoBehaviour
{
    [SerializeField] protected Collider interactableCollider;   
    
    protected virtual void Awake()
    {
        if (interactableCollider == null)
            interactableCollider = GetComponent<Collider>();
    }
    
    public virtual void OnTriggerEnter(Collider other)
    {
        PlayerManager player = other.GetComponent<PlayerManager>();

        if (player)
        {
            EnterArea(player);
        }
    }
    
    public virtual void OnTriggerExit(Collider other)
    {
        PlayerManager player = other.GetComponent<PlayerManager>();

        if (player)
        {
            ExitArea(player);
        }
    }

    protected virtual void EnterArea(CharacterManager character)
    {
        
    }
    
    protected virtual void ExitArea(CharacterManager character)
    {
        
    }
}
