using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevenueFacilityTile_BaseRide_Cafe : RevenueFacilityTile
{
    [SerializeField] private List<Transform> seats = new List<Transform>();
    private Dictionary<Transform, bool> seatOccupied = new Dictionary<Transform, bool>();
    
    private void Start()
    {
        // 모든 좌석을 비어 있다고 초기화
        foreach (var seat in seats)
        {
            seatOccupied[seat] = false;
        }
    }
    
    protected override IEnumerator ProcessQueue()
    {
        Transform assignedSeat = null;

        yield return StartCoroutine(WaitForLowestAvailableSeat());
        assignedSeat = GetAvailableSeat();
        if(assignedSeat == null) yield break;
        
        while (waitingQueue.Count > 0)
        {
            PathFindingUnit client = waitingQueue.Dequeue();
            
            BoardRide(client, assignedSeat);

            var visitors = waitingQueue.ToArray();
            for (int i = 0; i < visitors.Length; i++)
            {
                Vector3 targetPosition = queueStartPoint.position - queueStartPoint.forward * queueSpacing * i;
                yield return StartCoroutine(visitors[i].MoveToPointCoroutine(targetPosition));
            }
        }
    }
    
    protected override void BoardRide(PathFindingUnit rider, Transform seat)
    {
        base.BoardRide(rider, seat);
        seatOccupied[seat] = true;

        rider.transform.parent = seat; 
        rider.transform.localPosition = Vector3.zero; 
        rider.transform.localRotation = Quaternion.identity;
         
        rider.RideAttraction();
        StartCoroutine(StayOnRide(rider, seat));
    }
    
    private IEnumerator StayOnRide(PathFindingUnit rider, Transform seat)
    {
        float waitTime = Random.Range(10f, 60f);
        yield return new WaitForSeconds(waitTime); // 한 바퀴 회전

        ExitRide(rider, seat);
    }

    private void ExitRide(PathFindingUnit rider, Transform seat)
    {
        rider.transform.parent = null; 
        rider.transform.position = exitPoint.position;
        rider.transform.rotation = exitPoint.rotation;
        rider.ExitAttraction();
        seatOccupied[seat] = false; 

        rider.GetNextDestination(GetExitRoad());

        // 다음 대기자 처리
        if (waitingQueue.Count > 0)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator WaitForLowestAvailableSeat()
    {
        Transform seat = null;

        // 2️⃣ 가장 낮은 빈 좌석이 생길 때까지 반복
        while ((seat = GetAvailableSeat()) == null || seatOccupied[seat])
        {
            yield return null; // 다음 프레임까지 대기
        }
    }
    
    private Transform GetAvailableSeat()
    {
        // 비어 있는 좌석만 필터링
        List<Transform> availableSeats = new List<Transform>();

        foreach (var seat in seats)
        {
            if (seatOccupied.ContainsKey(seat) && !seatOccupied[seat])
            {
                availableSeats.Add(seat);
            }
        }

        // 비어 있는 좌석이 없으면 null 반환
        if (availableSeats.Count == 0)
            return null;

        // 무작위 좌석 선택
        int randomIndex = Random.Range(0, availableSeats.Count);
        return availableSeats[randomIndex];
    }

}
