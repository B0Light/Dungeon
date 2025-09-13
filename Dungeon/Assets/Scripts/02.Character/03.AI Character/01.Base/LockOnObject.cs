using UnityEngine;

public class LockOnObject : MonoBehaviour
{
    [SerializeField] private GameObject lockOnObj;
    private bool _isLockOnMarkEnable;
    private PlayerLocomotionManager _lastLocomotionManager;
    private PlayerLocomotionManager _locomotionManager;
    private bool _isDead = false;
    private void Start()
    {
        lockOnObj.gameObject.SetActive(false);
        _isLockOnMarkEnable = false;
        _isDead = false;
    }

    private void OnTriggerEnter(Collider otherCollider)
    {
        _locomotionManager = otherCollider.GetComponentInParent<PlayerLocomotionManager>();
        
        if (_isDead || _locomotionManager == null) return;
        
        _lastLocomotionManager = _locomotionManager;
        _locomotionManager.AddTargetCandidate(gameObject);
    }

    private void OnTriggerExit(Collider otherCollider)
    {
        _locomotionManager = otherCollider.GetComponent<PlayerLocomotionManager>();

        if (_locomotionManager != null)
        {
            Highlight(false);
            _locomotionManager.RemoveTarget(gameObject);
        }
    }

    public void Highlight(bool enable)
    {
        if(_isLockOnMarkEnable == enable) return;
        if (lockOnObj != null)
        {
            _isLockOnMarkEnable = enable;
            lockOnObj.gameObject.SetActive(_isLockOnMarkEnable);
        }
    }
    
    public void PerformDeath()
    {
        _isDead = true;
        Highlight(false);
        if (_lastLocomotionManager != null)
        {
            _lastLocomotionManager.RemoveTarget(gameObject);
        }
        gameObject.SetActive(false);
    }
}
