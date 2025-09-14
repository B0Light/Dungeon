using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class MiscItemImporter : MonoBehaviour
{
    [MenuItem("Tools/Import Misc Data from CSV")]
    public static void ImportMisc()
    {
        string filePath = "Assets/Data/Load/Sheets/03.MiscItemData/Misc.csv";
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            Debug.LogError(filePath + " 해당 CSV 파일을 찾을 수 없습니다. 파일 경로를 확인해주세요.");
            return;
        }
        
        string[] lines = File.ReadAllLines(filePath, Encoding.GetEncoding("euc-kr"));

        for (int i = 1; i < lines.Length; i++) // 1부터 시작해서 헤더를 건너뜁니다.
        {
            string[] values = lines[i].Split(',');
            /*
             * ID_10_NaturalResource
             * ID_11_HuntingMaterials
             * ID_12_Loot
             * ID_13_Point
             */
            string category = values[0];
            if (category.Equals("")) continue;

            ItemInfoMisc item = ScriptableObject.CreateInstance<ItemInfoMisc>();
            item.itemAbilities = new List<ItemAbility>();
            
            item.itemCode = int.Parse(values[1]);  
            item.itemName = values[2];            
            // InfoPath : ID_1x_Category/ID_1xxx_ItemName
            string itemInfoPath = category + "/" + $"ID_{item.itemCode:D4}_{item.itemName}";
            string iconPath = "Assets/Data/Load/ItemSprites/" + itemInfoPath + ".png";

            item.itemIcon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath); 
            item.itemTier = (ItemTier)int.Parse(values[3]);                 
            item.itemDescription = values[4];                               
            item.purchaseCost = int.Parse(values[5]);                       
            item.saleCost = Mathf.FloorToInt(item.purchaseCost * 0.9f);    
            item.width = int.Parse(values[6]);                             
            item.height = int.Parse(values[7]);                             
            item.weight = int.Parse(values[8]);

            item.itemType = ItemType.Misc;
            item.itemAbilities.Add(new ItemAbility(ItemEffect.Resource, 0));

            //item.itemName = values[9]; // 한국어 이름 으로 저장 
            
            // ScriptableObject를 애셋으로 저장
            string assetPath = "Assets/Resources/Items/C_Items_Misc/" + itemInfoPath + ".asset";
            AssetDatabase.CreateAsset(item, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Misc data imported successfully.");
    }
}
