using UnityEngine;

public class RevenueFacilityTile_Exit : RevenueFacilityTile
{
    public override void AddVisitor(PathFindingUnit visitor)
    {
        GenerateIncome();
        visitor.LeaveShelter();
    }
}
