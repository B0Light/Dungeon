using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ThemePark/Income Event Channel")]
public class IncomeEventSO : ScriptableObject
{
    // 이벤트 정의 - 수입 발생시 호출될 이벤트
    public event Action<IncomeData> OnIncomeGenerated;

    // 수입 발생 시 이벤트 발행 메서드
    public void RaiseIncomeEvent(IncomeData incomeData)
    {
        WorldPlayerInventory.Instance.balance.Value += incomeData.incomeAmount;
        OnIncomeGenerated?.Invoke(incomeData);
    }

    // 이벤트 구독 취소 메서드 (씬 전환 등에 사용)
    public void ClearAllSubscribers()
    {
        OnIncomeGenerated = null;
    }
}

// IncomeData.cs - 수입 데이터 구조체
[Serializable]
public struct IncomeData
{
    public string attractionName; // 놀이기구 이름
    public int incomeAmount;    // 수입 금액
    public DateTime timestamp;    // 수입 발생 시간

    public IncomeData(string name, int amount)
    {
        attractionName = name;
        incomeAmount = amount;
        timestamp = DateTime.Now;
    }
}