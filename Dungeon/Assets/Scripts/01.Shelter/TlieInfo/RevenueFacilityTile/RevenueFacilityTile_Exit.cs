using UnityEngine;

public class RevenueFacilityTile_Exit : RevenueFacilityTile
{
    public override void AddVisitor(ShelterVisitor visitor)
    {
        GenerateIncome();
        visitor.LeaveShelter();
    }
}
