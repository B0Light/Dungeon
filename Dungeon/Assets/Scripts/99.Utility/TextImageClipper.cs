using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextImageClipper : MonoBehaviour
{
    [Header("클리핑 설정")]
    [SerializeField] private TextMeshProUGUI maskText;
    [SerializeField] private Image targetImage;
    [SerializeField] private bool useStencilMask = true;
    
    [Header("스텐실 설정")]
    [SerializeField] private int stencilID = 1;
    
    private Material maskMaterial;
    private Material imageMaterial;

    void Start()
    {
        SetupClipping();
    }

    void SetupClipping()
    {
        if (useStencilMask)
        {
            SetupStencilMask();
        }
        else
        {
            SetupBasicMask();
        }
    }

    /// <summary>
    /// 기본 Mask 컴포넌트를 사용한 클리핑
    /// </summary>
    void SetupBasicMask()
    {
        // 부모에 Mask 컴포넌트 추가
        Mask mask = GetComponent<Mask>();
        if (mask == null)
        {
            mask = gameObject.AddComponent<Mask>();
        }
        
        // 마스크용 이미지 컴포넌트 추가 (투명하게 설정)
        Image maskImage = GetComponent<Image>();
        if (maskImage == null)
        {
            maskImage = gameObject.AddComponent<Image>();
        }
        maskImage.color = new Color(1, 1, 1, 0); // 투명
        
        mask.showMaskGraphic = false;
    }

    /// <summary>
    /// Stencil Buffer를 사용한 고급 클리핑
    /// </summary>
    void SetupStencilMask()
    {
        // TextMeshPro용 스텐실 마스크 Material 생성
        if (maskText != null)
        {
            CreateStencilMaskMaterial();
            maskText.fontMaterial = maskMaterial;
        }

        // 이미지용 스텐실 Material 생성
        if (targetImage != null)
        {
            CreateStencilImageMaterial();
            targetImage.material = imageMaterial;
        }
    }

    void CreateStencilMaskMaterial()
    {
        // TextMeshPro 스텐실 마스크 셰이더 Material
        Shader stencilMaskShader = Shader.Find("TextMeshPro/Distance Field");
        maskMaterial = new Material(stencilMaskShader);
        
        // 스텐실 설정
        maskMaterial.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.Always);
        maskMaterial.SetInt("_Stencil", stencilID);
        maskMaterial.SetInt("_StencilOp", (int)UnityEngine.Rendering.StencilOp.Replace);
        maskMaterial.SetInt("_StencilWriteMask", 255);
        maskMaterial.SetInt("_StencilReadMask", 255);
        
        // 텍스트를 투명하게 (마스크 역할만)
        maskMaterial.SetInt("_ColorMask", 0);
    }

    void CreateStencilImageMaterial()
    {
        // UI 이미지용 스텐실 Material
        Shader stencilImageShader = Shader.Find("UI/Default");
        imageMaterial = new Material(stencilImageShader);
        
        // 스텐실 설정 - 마스크 영역에서만 렌더링
        imageMaterial.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.Equal);
        imageMaterial.SetInt("_Stencil", stencilID);
        imageMaterial.SetInt("_StencilOp", (int)UnityEngine.Rendering.StencilOp.Keep);
        imageMaterial.SetInt("_StencilWriteMask", 255);
        imageMaterial.SetInt("_StencilReadMask", 255);
    }

    /// <summary>
    /// 마스크 텍스트 변경
    /// </summary>
    public void SetMaskText(string text)
    {
        if (maskText != null)
        {
            maskText.text = text;
        }
    }

    /// <summary>
    /// 클리핑 활성화/비활성화
    /// </summary>
    public void SetClippingEnabled(bool enabled)
    {
        if (useStencilMask)
        {
            if (targetImage != null)
            {
                targetImage.material = enabled ? imageMaterial : null;
            }
        }
        else
        {
            Mask mask = GetComponent<Mask>();
            if (mask != null)
            {
                mask.enabled = enabled;
            }
        }
    }

    void OnDestroy()
    {
        // Material 메모리 정리
        if (maskMaterial != null)
        {
            DestroyImmediate(maskMaterial);
        }
        if (imageMaterial != null)
        {
            DestroyImmediate(imageMaterial);
        }
    }
} 