using UnityEngine;

public class DungeonGridBuildSystem : BaseGridBuildSystem
{
    [SerializeField] private DungeonManager dungeonManager;

    protected override void Awake()
    {
        base.Awake();
        dungeonManager.GeneratedDungeon += OnGeneratedDungeon;
    }

    private void OnGeneratedDungeon()
    {
        _fixedGrid = dungeonManager.FixedGrid;
    }
}
