using System.Collections;
using UnityEngine;

public class WireframeShader : MonoBehaviour
{
    [HideInInspector] public GameObject wireframeObject;
    [SerializeField] private Material wireframeMaterial;
    
    [Header("WireFrame Color")]
    [SerializeField] private Color acquireColor;
    [SerializeField] private Color refundColor;
    private Mesh _bakedMesh;

    private bool _hasMesh = false;
    private bool _isSkinned = false;

    private Coroutine _fadeOutCoroutine;

    public void ShowWireFrameMat(bool isAcquire)
    {
        RemoveAllChildren();
        if (wireframeMaterial == null)
        {
            Debug.LogError("The Wireframe Material field is empty. You must assign the wireframe material!");
            return;
        }

        if (GetComponent<MeshFilter>() != null || GetComponent<SkinnedMeshRenderer>() != null)
            _hasMesh = true;

        if (_hasMesh)
        {
            _bakedMesh = new Mesh();
            wireframeObject = new GameObject("Wireframe");
            wireframeObject.layer = LayerMask.NameToLayer("Wireframe");
            wireframeObject.transform.SetParent(transform);
            wireframeObject.transform.localPosition = Vector3.zero;
            wireframeObject.transform.localScale = Vector3.one;
            wireframeObject.transform.localRotation = Quaternion.identity;

            var meshFilter = GetComponent<MeshFilter>();

            if (meshFilter == null)
                _isSkinned = true;

            Material newWireframeMat = new Material(wireframeMaterial); // 원본 유지
            newWireframeMat.SetColor("_WireColor", isAcquire ? acquireColor : refundColor);
            if (_isSkinned)
            {
                var skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
                _bakedMesh = BakeMesh(skinnedMeshRenderer.sharedMesh);
                var wireframeRenderer = wireframeObject.AddComponent<SkinnedMeshRenderer>();
                wireframeRenderer.bones = skinnedMeshRenderer.bones;
                wireframeRenderer.sharedMesh = _bakedMesh;
                wireframeRenderer.material = newWireframeMat;
            }
            else
            {
                _bakedMesh = BakeMesh(meshFilter.sharedMesh);
                var meshRenderer = wireframeObject.AddComponent<MeshRenderer>();
                wireframeObject.AddComponent<MeshFilter>();
                wireframeObject.GetComponent<MeshFilter>().sharedMesh = _bakedMesh;
                meshRenderer.material = newWireframeMat;
            }

            if (_fadeOutCoroutine != null)
            {
                StopCoroutine(_fadeOutCoroutine);
            }
            
            _fadeOutCoroutine = StartCoroutine(FadeAndDestroy(newWireframeMat, 3f)); // 3초 동안 투명화 후 삭제
        }
        else
        {
            Debug.LogError(name + " does not have a mesh!");
        }
    }
    
    private void RemoveAllChildren()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }


    private IEnumerator FadeAndDestroy(Material material, float duration)
    {
        float elapsedTime = 0f;
        Color startColor = material.GetColor("_WireColor");
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // 알파를 0으로 설정
        material.SetColor("_WireColor", new Color(startColor.r, startColor.g, startColor.b, 1f));
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            Color newColor = Color.Lerp(startColor, endColor, elapsedTime / duration);
            material.SetColor("_WireColor", newColor); // 셰이더의 _WireColor 알파 값 변경
            yield return null;
        }

        Destroy(wireframeObject);
        _fadeOutCoroutine = null;
    }


    private Mesh BakeMesh(Mesh originalMesh)
    {
        var maxVerts = 2147483647;
        var meshNor = originalMesh.normals;
        var meshTris = originalMesh.triangles;
        var meshVerts = originalMesh.vertices;
        var boneW = originalMesh.boneWeights;
        var vertsNeeded = meshTris.Length;

        if (vertsNeeded > maxVerts)
        {
            Debug.LogError("The mesh has so many vertices that Unity could not create it!");
            return null;
        }

        var resultMesh = new Mesh();
        resultMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        var resultVerts = new Vector3[vertsNeeded];
        var resultUVs = new Vector2[vertsNeeded];
        var resultTris = new int[meshTris.Length];
        var resultNor = new Vector3[vertsNeeded];
        var boneWLen = (boneW.Length > 0) ? vertsNeeded : 0;
        var resultBW = new BoneWeight[boneWLen];

        for (var i = 0; i < meshTris.Length; i += 3)
        {
            resultVerts[i] = meshVerts[meshTris[i]];
            resultVerts[i + 1] = meshVerts[meshTris[i + 1]];
            resultVerts[i + 2] = meshVerts[meshTris[i + 2]];
            resultUVs[i] = new Vector2(0f, 0f);
            resultUVs[i + 1] = new Vector2(1f, 0f);
            resultUVs[i + 2] = new Vector2(0f, 1f);
            resultTris[i] = i;
            resultTris[i + 1] = i + 1;
            resultTris[i + 2] = i + 2;
            resultNor[i] = meshNor[meshTris[i]];
            resultNor[i + 1] = meshNor[meshTris[i + 1]];
            resultNor[i + 2] = meshNor[meshTris[i + 2]];

            if (resultBW.Length > 0)
            {
                resultBW[i] = boneW[meshTris[i]];
                resultBW[i + 1] = boneW[meshTris[i + 1]];
                resultBW[i + 2] = boneW[meshTris[i + 2]];
            }
        }

        resultMesh.vertices = resultVerts;
        resultMesh.uv = resultUVs;
        resultMesh.triangles = resultTris;
        resultMesh.normals = resultNor;
        resultMesh.bindposes = originalMesh.bindposes;
        resultMesh.boneWeights = resultBW;

        return resultMesh;
    }
}
