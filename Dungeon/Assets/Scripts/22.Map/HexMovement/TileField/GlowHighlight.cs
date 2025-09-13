using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowHighlight : MonoBehaviour
{
    Dictionary<Renderer, Material[]> glowMaterialDictionary = new Dictionary<Renderer, Material[]>();
    Dictionary<Renderer, Material[]> originalMaterialDictionary = new Dictionary<Renderer, Material[]>();

    Dictionary<Color, Material> cachedGlowMaterials = new Dictionary<Color, Material>();

    public Material glowMaterial;

    private bool isGlowing = false;
    private bool onMouse = false;

    private Color validSpaceColor = Color.green;
    private Color originalGlowColor;
    private Color originalSpriteColor;

    private void Awake()
    {
        //PrepareMaterialDictionaries();
        setDictionaries();
        originalGlowColor = glowMaterial.GetColor("_GlowColor");
        originalSpriteColor = new Color(.2f,.2f,.2f,.4f);
    }

    // new selection 
    private void setDictionaries()
    {
        Transform selectObject = transform.Find("Selection");

        if (selectObject != null)
        {
            Renderer[] renderers = selectObject.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                Material[] originalMaterials = renderer.materials;
                originalMaterialDictionary.Add(renderer, originalMaterials);

                Material[] newMaterials = new Material[renderer.materials.Length];

                for (int i = 0; i < originalMaterials.Length; i++)
                {
                    Material mat = null;
                    if (cachedGlowMaterials.TryGetValue(originalMaterials[i].color, out mat) == false)
                    {
                        mat = new Material(glowMaterial);
                        //By default, Unity considers a color with the property name name "_Color" to be the main color
                        mat.color = originalMaterials[i].color;
                        cachedGlowMaterials[mat.color] = mat;
                    }
                    newMaterials[i] = mat;
                }
                glowMaterialDictionary.Add(renderer, newMaterials);
            }
        }
    }

    internal void HighlightValidPath()
    {
        if (isGlowing == false)
            return;
        foreach (Renderer renderer in glowMaterialDictionary.Keys)
        {
            foreach (Material item in glowMaterialDictionary[renderer])
            {
                item.SetFloat("_DynamicGlow", 0f);
                item.SetFloat("_GlowPower", 0f);
                item.SetColor("_GlowColor", validSpaceColor);
            }
        }
    }

    internal void ResetGlowHighlight()
    {
        foreach (Renderer renderer in glowMaterialDictionary.Keys)
        {
            foreach (Material item in glowMaterialDictionary[renderer])
            {
                item.SetFloat("_DynamicGlow", 1f);
                item.SetFloat("_GlowPower", 1f);
                item.SetColor("_GlowColor", originalGlowColor);
            }
        }
    }

    public void ToggleGlow()
    {
        if (isGlowing == false)
        {
            ResetGlowHighlight();
            foreach (Renderer renderer in originalMaterialDictionary.Keys)
            {
                renderer.materials = glowMaterialDictionary[renderer];
            }

        }
        else
        {
            foreach (Renderer renderer in originalMaterialDictionary.Keys)
            {
                renderer.materials = originalMaterialDictionary[renderer];
            }
        }
        
        Transform selectObject = transform.Find("Selection");
        selectObject.GetComponentInChildren<Renderer>().enabled = !selectObject.GetComponentInChildren<Renderer>().enabled;
        isGlowing = !isGlowing;
    }

    public void ToggleGlow(bool state)
    {
        if (isGlowing == state)
            return;
        isGlowing = !state;
        ToggleGlow();
    }

    public void OnMouseToggleGlow() // just Cursor on the tile
    {
        Transform selectObject = transform.Find("OnMouse");
        MeshRenderer meshRenderer = selectObject.GetComponentInChildren<MeshRenderer>();
        SpriteRenderer sr = selectObject.GetComponentInChildren<SpriteRenderer>();
        if(sr == null || meshRenderer == null) return;
        meshRenderer.enabled = !meshRenderer.enabled; 
        if (onMouse == false)
        {
            sr.color = Color.red + Color.yellow;
        }
        else
        {
            sr.color = originalSpriteColor;
        }
        onMouse = !onMouse;
    }
}
