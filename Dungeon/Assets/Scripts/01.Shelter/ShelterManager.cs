using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShelterManager : MonoBehaviour
{
    [SerializeField] private GameObject parkGoerPrefab;
    [SerializeField] private Transform parkEntrance;
    [SerializeField] private Button openShelterButton;

    public int visitorCapacity = 30;
    public float visitCycle = 3f;
    private readonly object _lockObject = new object();
    private readonly List<PathFindingUnit> _shelterVisitorList = new List<PathFindingUnit>();
    
    public void RemoveVisitor(PathFindingUnit visitor)
    {
        lock (_lockObject)
        {
            _shelterVisitorList.Remove(visitor);
        }
    }

    private void OnEnable()
    {
        WorldTimeManager.Instance.day.OnValueChanged += ResetDailyOperation;
    }

    private void ResetDailyOperation(int newValue)
    {
        SetVisit(false);
    }

    public void OpenShelter()
    {
        if(WorldSaveGameManager.Instance.currentGameData.isVisitedToday) return;
        
        SetVisit(true);
        StartCoroutine(VisitEnumerator());
    }

    private IEnumerator VisitEnumerator()
    {
        float duration = visitorCapacity * visitCycle;
        float elapsedTime = 0f;
    
        while (elapsedTime < duration)
        {
            yield return new WaitForSecondsRealtime(visitCycle);
            HandleVisitorEntry();
            elapsedTime += visitCycle;
        }
    }
    
    [ContextMenu("VisitorEntry")]
    public void HandleVisitorEntry()
    {
        GameObject visitor = Instantiate(parkGoerPrefab, parkEntrance);
        PathFindingUnit pathFindingUnit = visitor.GetComponent<PathFindingUnit>();
        if(pathFindingUnit) pathFindingUnit.SpawnVisitor(this);
        lock (_lockObject)
        {
            _shelterVisitorList.Add(pathFindingUnit);
        }
    }

    private void SetVisit(bool value)
    {
        WorldSaveGameManager.Instance.currentGameData.isVisitedToday = value;
        openShelterButton.interactable = !value;
    }

    public bool IsVisitorInShelter()
    {
        lock (_lockObject)
        {
            return _shelterVisitorList.Count > 0;
        }
    }

    
    
} 
