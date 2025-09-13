using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class WeaponDataImporter : MonoBehaviour
{
    [MenuItem("Tools/Import Weapon Data from CSV")]
    public static void ImportWeaponData()
    {
        string filePath = "Assets/Data/Load/Sheets/01.EquipmentItemData/Weapon.csv";
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
            EquipmentItemInfoWeapon item = ScriptableObject.CreateInstance<EquipmentItemInfoWeapon>();

            item.itemAbilities = new List<ItemAbility>();
            /* Base Item Info */
            item.itemCode = int.Parse(values[1]);  
            item.itemName = values[2];             
            string itemInfoPath = category + "/" + $"ID_{item.itemCode:D4}_{item.itemName}";
            string iconPath = "Assets/Data/Load/ItemSprites/ID_00_Weapon/" + itemInfoPath + "_Base.png";

            item.itemIcon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath); 
            item.itemTier = (ItemTier)int.Parse(values[3]);                 
            item.itemDescription = values[4];                               
            item.purchaseCost = int.Parse(values[5]);                       
            item.saleCost = Mathf.FloorToInt(item.purchaseCost * 0.9f);    
            item.height = int.Parse(values[6]);                             
            item.width = int.Parse(values[7]);                             
            item.weight = int.Parse(values[8]);                            

            /* Weapon Item Info */
            string animPath = $"Assets/Data/Load/WeaponAnimControllers/{category}.overrideController"; 
            item.weaponAnimator = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(animPath);

            string modelPath = "Assets/Data/Load/ItemModels/ID_00_Weapon/" + itemInfoPath + ".prefab";
            item.itemModel = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

            string equipSpritePath = "Assets/Data/Load/ItemSprites/ID_00_Weapon/" + itemInfoPath + "_Equip.png";
            item.weaponEquipSprite = AssetDatabase.LoadAssetAtPath<Sprite>(equipSpritePath);

            item.itemType = ItemType.Weapon;
            
            ItemAbility ability1 = new ItemAbility(ItemEffect.PhysicalAttack, int.Parse(values[9]));
            item.itemAbilities.Add(ability1); 
            ItemAbility ability2 = new ItemAbility(ItemEffect.MagicalAttack, int.Parse(values[10]));
            item.itemAbilities.Add(ability2);
            
            item.poiseDamage = int.Parse(values[11]);                      
            item.attackSpeed = float.TryParse(values[12], out var atkSpd) ? atkSpd : 1.0f;                     
            item.hAtkMod01 = 1.0f;
            item.hAtkMod02 = 1.2f;
            item.hAtkMod03 = 1.4f;
            item.vAtkMod01 = 1.3f;
            item.vAtkMod02 = 1.7f;
            item.vAtkMod03 = 2.0f;
            item.runningAtkMod = 1.5f;
            item.rollingAtkMod = 1.0f;
            item.backStepAtkMod = 1.0f;
            item.jumpingAtkMod = 1.0f;
            item.baseActionCost = 1;

            item.physicalDamageAbsorption = 30;
            item.magicalDamageAbsorption = 0;
            item.stability = 50;

            
            item.lightAttackAction = ScriptableObject.CreateInstance<BaseAttackAction_Light>();
            item.heavyAttackAction = ScriptableObject.CreateInstance<BaseAttackAction_Heavy>();
            item.blockAction = ScriptableObject.CreateInstance<BlockAction>();
            
            item.itemName = values[13]; // 한국어 이름 으로 저장 

            // ScriptableObject를 애셋으로 저장
            string assetPath = "Assets/Resources/Items/A_Items_Equipment/Items_00xx_Weapon/" + itemInfoPath + ".asset";
            AssetDatabase.CreateAsset(item, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Weapon data imported successfully.");
    }
}
