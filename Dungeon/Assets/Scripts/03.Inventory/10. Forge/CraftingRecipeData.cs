using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftingRecipe", menuName = "Crafting/Recipe")]
public class CraftingRecipeData : ScriptableObject
{
    public CraftingRecipe recipe;
}

[System.Serializable]
public class CraftingRecipe
{
    public List<RecipeIngredient> ingredients;
    public ItemData resultItem;
    public int resultQuantity;
}

[System.Serializable]
public class RecipeIngredient
{
    public ItemData itemData;
    public int quantity;
}