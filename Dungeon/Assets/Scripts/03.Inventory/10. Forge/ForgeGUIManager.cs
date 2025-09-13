using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class ForgeGUIManager : GUIComponent
{
    private int _girdWidth = 5;
    private int _gridHeight = 5;
    
    private List<CraftingRecipeData> _recipes;
    [SerializeField] private ItemGrid craftingGrid;
    [SerializeField] private ItemGrid previewGrid;
    [SerializeField] private Transform recipeBtnSlot;
    [SerializeField] private GameObject recipeBtnPrefab;
    private readonly List<RecipeButton> _recipeButtons = new List<RecipeButton>();
    
    private System.Action<int> _onValueChangedHandler;
    
    public void InitForge()
    {
        craftingGrid.SetGrid(_girdWidth, _gridHeight, null);
        previewGrid.SetGrid(5, 3, null);
        LoadRecipe();
        _onValueChangedHandler = i => UpdateCraftingGridUI();
        craftingGrid.totalItemValue.OnValueChanged += _onValueChangedHandler;
    }
    
    private void LoadRecipe()
    {
        _recipes = new List<CraftingRecipeData>();
        CraftingRecipeData[] recipeData = Resources.LoadAll<CraftingRecipeData>("Crafting");

        foreach (var recipe in recipeData)
        {
            _recipes.Add(recipe);
        }
        
        recipeBtnSlot.Cast<Transform>().ToList().ForEach(child => Destroy(child.gameObject));
        _recipeButtons.Clear();
        
        foreach (var craftingRecipeData in _recipes)
        {
            RecipeButton recipeButton = Instantiate(recipeBtnPrefab, recipeBtnSlot).GetComponent<RecipeButton>();
            recipeButton.Init(this, craftingRecipeData);
            _recipeButtons.Add(recipeButton);
        }
        
    }
    
    public override void CloseGUI()
    {
        if (_onValueChangedHandler != null)
        {
            craftingGrid.totalItemValue.OnValueChanged -= _onValueChangedHandler;
            _onValueChangedHandler = null;
        }
        base.CloseGUI();
    }

    public void TryCraft()
    {
        var currentItemDict = craftingGrid.GetCurItemDictById();
        var matchedRecipe = FindMatchingRecipe(currentItemDict);
        
        if (matchedRecipe != null)
        {
            ExecuteCrafting(matchedRecipe);
        }
    }
    
    private CraftingRecipe FindMatchingRecipe(Dictionary<int,int> items)
    {
        var sortedRecipes = _recipes.OrderByDescending(r => r.recipe.ingredients.Count);

        foreach (var recipeData in sortedRecipes)
        {
            if (CheckIngredientCount(items, recipeData.recipe))
            {
                return recipeData.recipe;
            }
        }
        return null;
    }
    
    // 레시피 매칭 
    private bool CheckIngredientCount(Dictionary<int,int> gridItems, CraftingRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            if (!gridItems.ContainsKey(ingredient.itemData.itemCode) || 
                gridItems[ingredient.itemData.itemCode] < ingredient.quantity)
            {
                return false;
            }
        }
        return true;
    }
    
    // 조합 로직 
    private void ExecuteCrafting(CraftingRecipe recipe)
    {
        UpdateCraftingGridUI();
        // 1. 재료 아이템들 제거
        RemoveIngredients(recipe);
    
        // 2. 결과 아이템 생성
        CreateResultItem(recipe);
    
        // 3. UI 업데이트
        UpdateCraftingGridUI();
    }

    private void RemoveIngredients(CraftingRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            craftingGrid.RemoveItemById(ingredient.itemData.itemCode, ingredient.quantity);
        }
    }

    private void CreateResultItem(CraftingRecipe recipe)
    {
        craftingGrid.AddItemById(recipe.resultItem.itemCode,recipe.resultQuantity, false);
    }

    private void UpdateCraftingGridUI()
    {
        var currentItemDict = craftingGrid.GetCurItemDictById();
        var matchedRecipe = FindMatchingRecipe(currentItemDict);
        
        previewGrid.ResetItemGrid();
        if (matchedRecipe != null)
        {
            previewGrid.AddItemById(matchedRecipe.resultItem.itemCode, matchedRecipe.resultQuantity);
        }

        foreach (var recipeButton in _recipeButtons)
        {
            recipeButton.CheckCanCreateItem();
        }
    }
    
    public ItemGrid GetItemGrid => craftingGrid;
}
