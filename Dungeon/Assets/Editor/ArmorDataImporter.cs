using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ArmorDataImporter : MonoBehaviour
{
    [MenuItem("Tools/Import Armor Data from CSV")]
    public static void ImportArmorData()
    {
        string filePath = "Assets/Data/Load/Sheets/01.EquipmentItemData/Armor.csv";
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
            if(category.Equals("")) continue;
            EquipmentItemInfoArmor item = ScriptableObject.CreateInstance<EquipmentItemInfoArmor>();

            item.itemAbilities = new List<ItemAbility>();
            
            /* Base Item Info */
            item.itemCode = int.Parse(values[1]);  
            item.itemName = values[2];             
            string itemInfoPath = category + "/" + $"ID_{item.itemCode:D4}_{item.itemName}";
            string iconPath = "Assets/Data/Load/ItemSprites/ID_02_Armor/" + itemInfoPath + ".png";

            item.itemIcon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath); 
            item.itemTier = (ItemTier)int.Parse(values[3]);                 
            item.itemDescription = values[4];                               
            item.purchaseCost = int.Parse(values[5]);                       
            item.saleCost = Mathf.FloorToInt(item.purchaseCost * 0.6f);    
            item.height = int.Parse(values[6]);                             
            item.width = int.Parse(values[7]);                             
            item.weight = int.Parse(values[8]);                            

            /* Armor Item Info */
            string modelPath = "Assets/Data/Load/ItemModels/ID_02_Armor/" + itemInfoPath + ".prefab";
            item.itemModel = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            item.itemType = ItemType.Armor;
            ItemAbility ability = new ItemAbility(ItemEffect.HealthPoint, int.Parse(values[9]));
            
            item.itemAbilities.Add(ability); 

            // backpackSize는 Vector2Int로 설정 (x, y 값을 CSV에서 읽어온다고 가정)
            int backpackSizeX = int.Parse(values[10]);
            int backpackSizeY = int.Parse(values[11]);
            item.backpackSize = new Vector2Int(backpackSizeX, backpackSizeY);

            if (item.backpackSize != Vector2Int.zero)
            {
                ItemAbility abilityCapacity = new ItemAbility(ItemEffect.StorageSpace, backpackSizeX * backpackSizeY);
                item.itemAbilities.Add(abilityCapacity); 
            }
            
            item.itemName = values[12]; // 한국어 이름 으로 저장 
            // ScriptableObject를 애셋으로 저장
            string assetPath = "Assets/Resources/Items/A_Items_Equipment/Items_02xx_Armor/" + itemInfoPath + ".asset";
            AssetDatabase.CreateAsset(item, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Armor data imported successfully.");
    }
}