using TMPro;
using UnityEngine;

public class TilteTextSetter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleShadow;
    [SerializeField] private TextMeshProUGUI titleUnderLay;
    [SerializeField] private TextMeshProUGUI titleMain;
    [SerializeField] private string title;

    [SerializeField] private int sizeF = 140;
    [SerializeField] private int sizeBase = 120;
    private void Start()
    {
        char firstWord;
        string remainString;
        string titleString;
        if (title.Length < 1) return;
        
        firstWord = title[0];
        remainString = title.Substring(1);
        titleString = $"<size={sizeF}><cspace=-8>" + firstWord + $"<size={sizeBase}><cspace=-6>" + remainString;

        titleShadow.text = titleString;
        titleUnderLay.text = titleString;
        titleMain.text = titleString;
    }

    
}
