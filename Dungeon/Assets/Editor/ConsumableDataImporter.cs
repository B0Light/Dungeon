using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ConsumableDataImporter : MonoBehaviour
{
    [MenuItem("Tools/Import Consumable Data from CSV")]
    public static void ImportMisc()
    {
        string filePath = "Assets/Data/Load/Sheets/02.ConsumableData/Consumable.csv";
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            Debug.LogError(filePath + " 해당 CSV 파일을 찾을 수 없습니다. 파일 경로를 확인해주세요.");
            return;
        }
        
        string[] lines = File.ReadAllLines(filePath, Encoding.GetEncoding("euc-kr"));

        for (int i = 1; i < lines.Length; i++) // 1부터 시작해서 헤더를 건너뜁니다.
        {
            string[] values = lines[i].Split(',');
            
            string category = values[0];
            if (category.Equals("")) continue;

            ItemInfoConsumable item = ScriptableObject.CreateInstance<ItemInfoConsumable>();
            item.itemAbilities = new List<ItemAbility>();
            
            item.itemCode = int.Parse(values[1]);  
            item.itemName = values[2];            
            string itemInfoPath = category + "/" + $"ID_{item.itemCode:D4}_{item.itemName}";
            string iconPath = "Assets/Data/Load/ItemSprites/ID_03_Consumable/" + itemInfoPath + ".png";

            item.itemIcon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath); 
            item.itemTier = (ItemTier)int.Parse(values[3]);                 
            item.itemDescription = values[4];                               
            item.purchaseCost = int.Parse(values[5]);                       
            item.saleCost = Mathf.FloorToInt(item.purchaseCost * 0.9f);    
            item.width = int.Parse(values[6]);                             
            item.height = int.Parse(values[7]);                             
            item.weight = int.Parse(values[8]);
            item.itemType = ItemType.Consumables;

            if (int.TryParse(values[9], out int buffCode1) && buffCode1 >= 5 && buffCode1 < 12)
            {
                item.itemAbilities.Add(new ItemAbility((ItemEffect)buffCode1, int.Parse(values[10])));
            }
            
            if (int.TryParse(values[11], out int buffCode2) && buffCode2 >= 5 && buffCode2 < 12)
            {
                item.itemAbilities.Add(new ItemAbility((ItemEffect)buffCode2, int.Parse(values[12])));
            }
            /*
             * 5  : RestoreHealth
             * 7  : Buff Attack
             * 8  : BuffDefense,
             * 9  : BuffActionPoint,
             * 10 : UtilitySpeed,
             * 11 : UtilityWeight,
             */
            item.itemName = values[13]; // 한국어 이름으로 저장 
            
            
            // ScriptableObject를 애셋으로 저장
            string assetPath = "Assets/Resources/Items/B_Items_Consumable/Items_03xx_Consumable/" + itemInfoPath + ".asset";
            AssetDatabase.CreateAsset(item, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Consumable data imported successfully.");
    }
}
