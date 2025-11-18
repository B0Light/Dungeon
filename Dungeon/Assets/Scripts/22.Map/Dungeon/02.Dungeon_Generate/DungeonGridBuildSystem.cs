using UnityEngine;

public class DungeonGridBuildSystem : BaseGridBuildSystem
{
    [SerializeField] private DungeonManager dungeonManager;

    protected override void Awake()
    {
        base.Awake();
        dungeonManager.OnGeneratedDungeon += OnGeneratedDungeon;
    }

    private void OnGeneratedDungeon()
    {
        Debug.Log("[Dungeon Grid Build System] : Set Fixed Grid");
        _fixedGrid = dungeonManager.FixedGrid;
    }
}
