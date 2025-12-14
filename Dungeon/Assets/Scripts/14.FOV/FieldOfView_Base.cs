using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public abstract class FieldOfView_Base : MonoBehaviour
{
    [SerializeField] protected float viewDistance = 10f;
    [SerializeField] protected LayerMask obstacleLayerMask;
    [SerializeField] protected LayerMask searchLayerMask; 
    
    protected Vector3 _origin;
    protected float _startingAngle;
    protected Vector3 _aimDirection;
    
    private const int MaxTargets = 30;
    protected readonly Collider[] _targetColliders = new Collider[MaxTargets];
    protected readonly List<Transform> _visibleTarget = new List<Transform>();
    protected readonly List<Transform> _previouslyVisibleTargets = new List<Transform>();
    
    protected virtual void Start() 
    {
        StartCoroutine(nameof(FindTargetWithDelay), 0.5f);
    }
    
    private IEnumerator FindTargetWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindTarget();
        }
    }
    
    protected abstract void FindTarget();
    
    protected void ControlTargetViewMeshRenderer(Transform parentObject, bool isEnabled)
    {
        Transform viewTransform = parentObject.Find("View");

        if (viewTransform != null)
        {
            MeshRenderer meshRenderer = viewTransform.GetComponent<MeshRenderer>();

            if (meshRenderer != null)
            {
                meshRenderer.enabled = isEnabled;
                if(isEnabled)
                    _previouslyVisibleTargets.Add(parentObject);
            }
        }
    }

    // --- Public Methods ---
    
    public void SetOrigin(Vector3 origin) {
        this._origin = origin;
    }
}
