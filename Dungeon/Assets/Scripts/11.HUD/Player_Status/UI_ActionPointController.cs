using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ActionPointController : MonoBehaviour
{
    [SerializeField] private GameObject actionPointPrefab;
    [SerializeField] private List<UI_ActionPointItem> actionPointList = new List<UI_ActionPointItem>();
    [SerializeField] private RectTransform actionPointSlot_A;
    [SerializeField] private RectTransform actionPointSlot_B;
    private int _curActiveActionPoint;
    
    public void SetStat(int newValue)
    {
        for (int i = 0; i < _curActiveActionPoint; i++)
        {
            if(i < newValue)
                actionPointList[i].RegainActionPoint();
            else
                actionPointList[i].UseActionPoint();
        }
        
    }
    
    public void SetMaxStat(int maxValue)
    {
        _curActiveActionPoint = maxValue;
        
        foreach (Transform child in actionPointSlot_A.transform)
        {
            Destroy(child.gameObject);
            actionPointList.Clear();
        }
        foreach (Transform child in actionPointSlot_B.transform)
        {
            Destroy(child.gameObject);
            actionPointList.Clear();
        }
        
        // 그냥 배치 
        int halfValue = maxValue / 2;

        if (maxValue <= 7)
        {
            for (int i = 0; i < maxValue; i++)
            {
                UI_ActionPointItem point = Instantiate(actionPointPrefab, actionPointSlot_B).GetComponent<UI_ActionPointItem>();
                actionPointList.Add(point);
            }
        }
        else
        {
            for (int i = 0; i < halfValue + (maxValue % 2); i++) // 홀수면 +1
            {
                UI_ActionPointItem point = Instantiate(actionPointPrefab, actionPointSlot_A).GetComponent<UI_ActionPointItem>();
                actionPointList.Add(point);
            }
            for (int i = 0; i < halfValue; i++)
            {
                UI_ActionPointItem point = Instantiate(actionPointPrefab, actionPointSlot_B).GetComponent<UI_ActionPointItem>();
                actionPointList.Add(point);
            }
        }

        // 원형으로 배치 
        //InstantiateUIElementsInQuarterCircle(actionPointSlot, actionPointPrefab, maxValue, 150, 50, 5);
    }
    
    /*
    void InstantiateUIElementsInQuarterCircle(RectTransform parent, GameObject prefab, int totalObjects, float initialRadius, float radiusIncrement, int objectsPerLayer)
    {
        float currentRadius = initialRadius;
        int objectsInCurrentLayer = 0;

        for (int i = 0; i < totalObjects; i++)
        {
            // 레이어가 꽉 차면 반지름 증가
            if (objectsInCurrentLayer >= objectsPerLayer)
            {
                currentRadius += radiusIncrement;
                objectsInCurrentLayer = 0;
            }

            // 0도 ~ 90도 범위에서 위치 설정
            float angle = objectsInCurrentLayer * (90f / (objectsPerLayer - 1));
            float radian = angle * Mathf.Deg2Rad;

            // UI 요소의 위치 계산 (Pivot 기준)
            float x = currentRadius * Mathf.Cos(radian);
            float y = currentRadius * Mathf.Sin(radian);
            Vector3 spawnPosition = new Vector3(x, y, 0);

            // UI 요소 생성
            GameObject uiElement = Instantiate(prefab, parent);
            UI_ActionPointItem actionPointItem = uiElement.GetComponent<UI_ActionPointItem>();
            RectTransform rectTransform = uiElement.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = spawnPosition; // 부모 기준 위치 설정

            actionPointList.Add(actionPointItem);
            // 현재 레이어에서 배치된 개수 증가
            objectsInCurrentLayer++;
            
        }
    }
    */
}
