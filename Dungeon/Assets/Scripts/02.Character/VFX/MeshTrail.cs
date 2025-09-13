using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

/*
 * PlayerManager : playerVariableManager.isTrailActive.OnValueChanged 로 동작 
 */
public class MeshTrail : MonoBehaviour
{
    private float _activeTime = 3f;

    private Coroutine _trailEffect;
    
    private readonly float _meshRefreshRate = 0.1f;
    private readonly float _meshDestroyDelay = 0.5f;

    private SkinnedMeshRenderer[] _skinnedMeshRenderers;
    private MeshRenderer[] _meshRenderers;

    private readonly int _poolSize = 30;
    private IObjectPool<GameObject> _trailPool;
    private Transform _poolParent;

    [Header("Shader Related")] 
    [SerializeField] private Material mat;
    [SerializeField] private string shaderVarRef;
    private readonly float _shaderVarRate = 0.1f;
    private readonly float _shaderVarRefreshRate = 0.05f;

    private void Start()
    {
        // Get both SkinnedMeshRenderer and MeshRenderer components
        _skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        _meshRenderers = GetComponentsInChildren<MeshRenderer>();
        
        // Create pool parent object
        GameObject trailPoolParent = new GameObject("TrailObjectsPool");
        _poolParent = trailPoolParent.transform;
        
        // Create ObjectPool with proper Unity 2021.1+ constructor
        _trailPool = new ObjectPool<GameObject>(
            createFunc: CreateTrailObject,
            actionOnGet: OnGetFromPool,
            actionOnRelease: OnReturnToPool,
            actionOnDestroy: OnDestroyPoolObject,
            defaultCapacity: _poolSize,
            maxSize: _poolSize * 2
        );
    }

    private GameObject CreateTrailObject()
    {
        GameObject trail = new GameObject("TrailObject");
        trail.SetActive(false);
        trail.layer = LayerMask.NameToLayer("Character");
        trail.transform.SetParent(_poolParent);
        
        MeshRenderer mr = trail.AddComponent<MeshRenderer>();
        MeshFilter mf = trail.AddComponent<MeshFilter>();
        
        return trail;
    }

    private void OnGetFromPool(GameObject trail)
    {
        trail.SetActive(true);
    }

    private void OnReturnToPool(GameObject trail)
    {
        trail.SetActive(false);
        trail.transform.SetParent(_poolParent);
        
        // 셰이더 값 초기화 - 모든 머티리얼에 대해
        MeshRenderer mr = trail.GetComponent<MeshRenderer>();
        if (mr != null && mr.materials != null)
        {
            foreach (var material in mr.materials)
            {
                if (material != null)
                {
                    material.SetFloat(shaderVarRef, 1f);
                }
            }
        }
    }

    private void OnDestroyPoolObject(GameObject trail)
    {
        if (trail != null)
        {
            Destroy(trail);
        }
    }
    
    public void SetTrailTime(float time)
    {
        _activeTime = time;
    }

    public void OnTrailActiveChanged(bool isActive)
    {
        _meshRenderers = GetComponentsInChildren<MeshRenderer>();
        if (isActive)
        {
            _trailEffect = StartCoroutine(ActivateTrail(_activeTime));
        }
        else
        {
            if (_trailEffect != null)
            {
                StopCoroutine(_trailEffect);
            }
        }
    }
    
    private IEnumerator ActivateTrail(float timeActive)
    {
        while (timeActive > 0)
        {
            if (this == null || gameObject == null) yield break;

            timeActive -= _meshRefreshRate;

            // Handle SkinnedMeshRenderers
            foreach (var skinnedMeshRenderer in _skinnedMeshRenderers)
            {
                if (skinnedMeshRenderer == null) continue;

                GameObject trail = _trailPool?.Get();
                if (trail == null) continue;

                Transform trailTransform = trail.transform;
                if (trailTransform == null) continue;

                trailTransform.SetParent(null, worldPositionStays: true); // 부모 영향 제거
                trailTransform.SetPositionAndRotation(skinnedMeshRenderer.transform.position, skinnedMeshRenderer.transform.rotation);
                trailTransform.localScale = Vector3.one; // 스케일 고정

                MeshRenderer mr = trail.GetComponent<MeshRenderer>();
                MeshFilter mf = trail.GetComponent<MeshFilter>();
                if (mf == null) continue;

                Mesh mesh = new Mesh();
                skinnedMeshRenderer.BakeMesh(mesh);
                mf.mesh = mesh;

                SetupTrailMaterials(mr, skinnedMeshRenderer.materials);

                StartCoroutine(ReturnTrailAfterDelay(trail, _meshDestroyDelay));
            }

            // Handle MeshRenderers
            foreach (var meshRenderer in _meshRenderers)
            {
                if (meshRenderer == null) continue;

                GameObject trail = _trailPool?.Get();
                if (trail == null) continue;

                Transform trailTransform = trail.transform;
                if (trailTransform == null) continue;

                trailTransform.SetParent(null, worldPositionStays: true); // 부모 영향 제거
                trailTransform.SetPositionAndRotation(meshRenderer.transform.position, meshRenderer.transform.rotation);
                trailTransform.localScale = Vector3.one; // 스케일 고정

                MeshRenderer mr = trail.GetComponent<MeshRenderer>();
                MeshFilter mf = trail.GetComponent<MeshFilter>();
                if (mf == null) continue;

                MeshFilter sourceMeshFilter = meshRenderer.GetComponent<MeshFilter>();
                if (sourceMeshFilter == null) continue;

                mf.mesh = sourceMeshFilter.mesh;

                SetupTrailMaterials(mr, meshRenderer.materials);

                StartCoroutine(ReturnTrailAfterDelay(trail, _meshDestroyDelay));
            }

            yield return new WaitForSecondsRealtime(_meshRefreshRate);
        }

        _activeTime = 3;
    }


    private void SetupTrailMaterials(MeshRenderer trailRenderer, Material[] sourceMaterials)
    {
        if (trailRenderer == null || sourceMaterials == null || sourceMaterials.Length == 0)
            return;

        Material[] trailMaterials = new Material[sourceMaterials.Length];
        
        for (int i = 0; i < sourceMaterials.Length; i++)
        {
            if (sourceMaterials[i] != null)
            {
                // 개별 머티리얼 인스턴스를 생성하여 공유 방지
                trailMaterials[i] = new Material(mat != null ? mat : sourceMaterials[i]);
            }
        }

        
        trailRenderer.materials = trailMaterials;

        // 각 머티리얼에 대해 애니메이션 시작
        foreach (var material in trailMaterials)
        {
            if (material != null)
            {
                StartCoroutine(AnimateMaterialFloat(material, 0, _shaderVarRate, _shaderVarRefreshRate));
            }
        }
    }
    
    private IEnumerator ReturnTrailAfterDelay(GameObject trail, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        _trailPool.Release(trail);
    }

    private IEnumerator AnimateMaterialFloat(Material material, float goal, float rate, float refreshRate)
    {
        material.SetFloat(shaderVarRef, 1f);
        float valueToAnimate = material.GetFloat(shaderVarRef);

        while (valueToAnimate > goal)
        {
            valueToAnimate -= rate;
            material.SetFloat(shaderVarRef, valueToAnimate);
            yield return new WaitForSecondsRealtime(refreshRate);
        }
        material.SetFloat(shaderVarRef, goal);
    }
}