using UnityEngine;

public class FieldOfView_Sight : FieldOfView_Base
{
    [SerializeField] private float fov = 110f;
    [SerializeField] private float multiple = 2f;
    private Mesh _mesh;

    protected override void Start()
    {
        _mesh = new Mesh();
        
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = _mesh;
        
        SetAimDirection(transform.forward);
        base.Start();
    }

    private void LateUpdate() 
    {
        UpdateMesh();
    }
    
    private void UpdateMesh() {
        int rayCount = (int)(fov*multiple);
        float angle = _startingAngle;
        float angleIncrease = fov / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 2];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = _origin;

        int vertexIndex = 1;
        int triangleIndex = 0;
        
        for (int i = 0; i <= rayCount; i++) {
            Vector3 rayDirection = GetVectorFromAngle(angle); 
            
            if (Physics.Raycast(_origin, rayDirection, out RaycastHit hit, viewDistance, obstacleLayerMask)) {
                vertices[vertexIndex] = hit.point;
            } else {
                vertices[vertexIndex] = _origin + rayDirection * viewDistance;
            }
            
            if (i > 0) {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }

        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;
        _mesh.RecalculateNormals(); 
        _mesh.bounds = new Bounds(_origin, Vector3.one * viewDistance * 2f);
    }

    protected override void FindTarget()
    {
        foreach (Transform target in _previouslyVisibleTargets)
        {
            if (target != null) 
            {
                ControlTargetViewMeshRenderer(target, false);
            }
        }
        _previouslyVisibleTargets.Clear();
        _visibleTarget.Clear();
        
        int hitCount = Physics.OverlapSphereNonAlloc(_origin, viewDistance, _targetColliders, searchLayerMask);
        for (int i = 0; i < hitCount; i++)
        {
            Transform target = _targetColliders[i].transform;
            Vector3 targetPosition = target.position;
            Vector3 dirToTarget = (targetPosition - _origin).normalized;
            
            float angleToTarget = Vector3.Angle(_aimDirection , dirToTarget);
            if (angleToTarget < fov / 2f)
            {
                ControlTargetViewMeshRenderer(target, true);
                float distanceToTarget = Vector3.Distance(_origin, targetPosition);
                if (!Physics.Raycast(_origin, dirToTarget, distanceToTarget, obstacleLayerMask))
                {
                    _visibleTarget.Add(target); 
                    Debug.Log($"FindTarget : {target.name}");
                }
            }
        }
        
    }
    
    public void SetAimDirection(Vector3 aimDirection)
    {
        var viewAngle = GetAngleFromVectorFloat(aimDirection);
        _startingAngle = viewAngle; // + _fov / 2f;
        _aimDirection = GetVectorFromAngle(viewAngle).normalized;
    }
    
    // --- Helper Method ---
    private float GetAngleFromVectorFloat(Vector3 dir) {
        dir.y = 0; 
        dir = dir.normalized;
        
        float n = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg; 
        if (n < 0) n += 360;

        return n;
    }
    
    private Vector3 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad)); 
    }
    
    // --- public Method ---
    
    public void SetFoV(float fov) {
        this.fov = fov;
    }

    public void SetViewDistance(float distanceValue) {
        this.viewDistance = distanceValue;
    }
    
    
}