using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class CraftingDataImporter : MonoBehaviour
{
    [MenuItem("Tools/Import Crafting Data from CSV")]
    public static void ImportCrafting()
    {
        string filePath = "Assets/Data/Load/Sheets/10.CraftingData/Crafting.csv";
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            Debug.LogError(filePath + " 해당 CSV 파일을 찾을 수 없습니다. 파일 경로를 확인해주세요.");
            return;
        }
        
        string[] lines = File.ReadAllLines(filePath, Encoding.GetEncoding("euc-kr"));
        
        if (lines.Length <= 1)
        {
            Debug.LogError("CSV 파일에 데이터가 없습니다.");
            return;
        }

        // 기존 레시피 데이터 폴더 생성 (없으면)
        string outputPath = "Assets/Resources/Crafting";
        if (!AssetDatabase.IsValidFolder(outputPath))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Crafting");
        }

        for (int i = 1; i < lines.Length; i++) // 1부터 시작해서 헤더를 건너뜁니다.
        {
            string[] values = lines[i].Split(',');
            
            // 빈 줄이나 잘못된 데이터 건너뛰기
            if (string.IsNullOrEmpty(values[1]))
                continue;
            
            string recipeID = values[0].Trim();
            string recipeName = values[1].Trim();
            int resultItemID = int.Parse(values[2].Trim());
            int resultQuantity = int.Parse(values[3].Trim());

            // 결과 아이템 찾기
            ItemData resultItem = FindItemDataByID(resultItemID);
            if (resultItem == null)
            {
                Debug.LogWarning($"결과 아이템을 찾을 수 없습니다: {resultItemID} (레시피: {recipeName})");
                continue;
            }

            // 재료 리스트 생성
            List<RecipeIngredient> ingredients = new List<RecipeIngredient>();
            
            // 재료 데이터 처리 (4번 인덱스부터 2개씩 묶어서 처리)
            for (int j = 4; j < values.Length; j += 2)
            {
                if (int.Parse(values[j]) == -1) break;
                
                int ingredientID = int.Parse(values[j].Trim());
                if (ingredientID == 0) break;
                
                string quantityStr = values[j + 1].Trim();
                string xStr = values[j + 2].Trim();
                string yStr = values[j + 3].Trim();
                
                if (string.IsNullOrEmpty(quantityStr)) break;
                
                ItemData ingredientItem = FindItemDataByID(ingredientID);
                if (ingredientItem == null)
                {
                    Debug.LogWarning($"재료 아이템을 찾을 수 없습니다: {ingredientID} (레시피: {recipeName})");
                    continue;
                }
                
                int quantity = int.Parse(quantityStr);
                int x = string.IsNullOrEmpty(xStr) ? 0 : int.Parse(xStr);
                int y = string.IsNullOrEmpty(yStr) ? 0 : int.Parse(yStr);
                
                RecipeIngredient ingredient = new RecipeIngredient
                {
                    itemData = ingredientItem,
                    quantity = quantity,
                };
                
                ingredients.Add(ingredient);
            }

            // CraftingRecipe 생성
            CraftingRecipe recipe = new CraftingRecipe
            {
                ingredients = ingredients,
                resultItem = resultItem,
                resultQuantity = resultQuantity,
            };

            // CraftingRecipeData ScriptableObject 생성
            CraftingRecipeData recipeData = ScriptableObject.CreateInstance<CraftingRecipeData>();
            recipeData.recipe = recipe;

            // 파일로 저장
            string assetPath = $"{outputPath}/{recipeID}_{recipeName}.asset";
            AssetDatabase.CreateAsset(recipeData, assetPath);
            
            Debug.Log($"레시피 데이터 생성 완료: {recipeName} ({recipeID})");
            
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("모든 크래프팅 레시피 데이터 임포트 완료!");
    }

    // ItemData를 ID로 찾는 헬퍼 함수
    private static ItemData FindItemDataByID(int itemID)
    {
        // ItemData가 저장된 경로에서 찾기
        string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/Resources/Items" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            
            if (item != null && item.itemCode == itemID)
            {
                return item;
            }
        }
        
        return null;
    }
}