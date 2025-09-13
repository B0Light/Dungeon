using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class RevenueFacilityTile : PlacedObject
{
    private GameObject _vCam;
    
    [Header("Attraction Properties")]
    [SerializeField] protected int attractionCapacity = 12;  // 수용 가능한 인원 수
    [SerializeField] protected float attractionCycleTime = 10f; // 놀이기구 한 사이클 시간 (초 단위)
    protected Transform exitPoint; // 출구 위치

    private Queue<ShelterVisitor> _onAttractionQueue = new Queue<ShelterVisitor>(); // 타일에는 들어 왔으나 아직 줄 서지 않은 인원 
    protected Queue<ShelterVisitor> waitingQueue = new Queue<ShelterVisitor>(); // 대기열
    protected bool isOperating = false; // 놀이기구가 동작 중인지 여부
    
    [SerializeField] protected Transform queueStartPoint;
    protected readonly float queueSpacing = 1.5f;
    [SerializeField] private int maxColumns = 8; 
    
    protected ParticleSystem moneyVfx;

    [SerializeField] protected IncomeEventSO incomeEventChannel;

    protected override void Awake()
    {
        base.Awake();
        moneyVfx = GetComponentInChildren<ParticleSystem>();
        exitPoint = FindChildByName(gameObject, "Exit").transform;
        
        _vCam = FindChildByName(gameObject, "CinemachineCamera");
    }

    public void SelectObject(bool value)
    {
        _vCam.SetActive(value);
    }
    
    public virtual void AddVisitor(ShelterVisitor visitor)
    {
        _onAttractionQueue.Enqueue(visitor);
        StartCoroutine(EnqueueVisitor(visitor));
    }
    
    private IEnumerator EnqueueVisitor(ShelterVisitor visitor)
    {
        int queueCount = _onAttractionQueue.Count + waitingQueue.Count -1;

        int row = queueCount / maxColumns; // 몇 번째 줄인지
        int column = queueCount % maxColumns; // 해당 줄에서 몇 번째 칸인지

        Vector3 targetPosition = queueStartPoint.position 
                                 - queueStartPoint.forward * queueSpacing * row  // 앞뒤 간격
                                 + queueStartPoint.right * queueSpacing * column; // 좌우 간격
        
        yield return StartCoroutine(visitor.MoveToPointCoroutine(targetPosition));
        
        waitingQueue.Enqueue(_onAttractionQueue.Dequeue());
        if (!isOperating)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    protected virtual IEnumerator ProcessQueue()
    {
        yield return null;
    }

    protected virtual void BoardRide(ShelterVisitor rider, Transform seat)
    {
        GenerateIncome();
    }

    protected void GenerateIncome()
    {
        // 약간의 랜덤 변동을 주는 수입 계산
        int actualIncome = Mathf.RoundToInt(GetFee() * UnityEngine.Random.Range(0.8f, 1.2f));
        
        // 수입 데이터 생성
        IncomeData incomeData = new IncomeData(buildObjData.itemName, actualIncome);
        
        // 이벤트 발행
        if (incomeEventChannel != null)
        {
            incomeEventChannel.RaiseIncomeEvent(incomeData);
        }
    }
}
