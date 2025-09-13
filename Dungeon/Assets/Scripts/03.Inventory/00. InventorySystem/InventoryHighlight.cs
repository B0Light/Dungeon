using UnityEngine;

public class InventoryHighlight : MonoBehaviour
{
    [SerializeField] RectTransform highlighter; // 위치할 좌표를 표시하는 하얀 박스 
    [SerializeField] RectTransform selector; // 아이템 선택을 표시하는 노란박스 
    
    public void ShowHighlighter(bool isShow)
    {
        highlighter.gameObject.SetActive(isShow);
    }
    
    public void ShowSelector(bool isShow)
    {
        selector.gameObject.SetActive(isShow);
    }

    public void SetSize(InventoryItem targetItem, bool selectorChange = true)
    {
        Vector2 size = new Vector2();
        size.x = targetItem.Width * ItemGrid.TileSizeWidth;
        size.y = targetItem.Height * ItemGrid.TileSizeHeight;
        highlighter.sizeDelta = size;
        if(selectorChange)
            selector.sizeDelta = size;
    }

    //아이템의 크기와 위치를 받아서 highlighter의 위치를 정한다.
    public void SetPosition(ItemGrid targetGrid, InventoryItem targetItem)
    {
        if(targetGrid == null || targetItem == null) return;
        Vector2 pos = targetGrid.CalculatePositionOnGrid(
            targetItem,
            targetItem.onGridPositionX,
            targetItem.onGridPositionY
        );

        highlighter.localPosition = pos;
        selector.localPosition = pos;
        
        highlighter.SetAsLastSibling();
        selector.SetAsLastSibling();
    }

    public void SetParent(ItemGrid targetGrid)
    {
        if(targetGrid == null){
            return;
        }
        
        highlighter.SetParent(targetGrid.GetComponent<RectTransform>());
        highlighter.SetAsLastSibling();
    }

    public void SetSelectorParent(ItemGrid targetGrid)
    {
        if(targetGrid == null){
            return;
        }
        
        selector.SetParent(targetGrid.GetComponent<RectTransform>());
        selector.SetAsLastSibling();
    }

    // 마우스 위치에 따라 하이라이터 위치 변경 
    public void SetPosition(ItemGrid targetGrid, InventoryItem targetItem, int posX, int posY){
        Vector2 pos = targetGrid.CalculatePositionOnGrid(
            targetItem,
            posX,
            posY
        );

        highlighter.localPosition = pos;
        highlighter.SetAsLastSibling();
    }
}