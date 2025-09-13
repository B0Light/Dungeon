using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EnemyInfoHUD : MonoBehaviour
{
    [SerializeField] private Image iconRenderer;
    [SerializeField] private TextMeshProUGUI enemyNameText;
    [SerializeField] private TextMeshProUGUI killCountText;

    public void Init(int id, int count)
    {
        if (iconRenderer != null)
            iconRenderer.sprite = WorldDatabase_Enemy.Instance.GetIconById(id);

        if (enemyNameText != null)
            enemyNameText.text = WorldDatabase_Enemy.Instance.GetNameById(id);

        if (killCountText != null)
            killCountText.text = $"Kill : {count}";
    }
}
