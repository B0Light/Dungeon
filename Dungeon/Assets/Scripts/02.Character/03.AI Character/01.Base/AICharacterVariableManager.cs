public class AICharacterVariableManager : CharacterVariableManager
{
    private AICharacterManager _aiCharacterManager;

    protected override void Awake()
    {
        base.Awake();
        _aiCharacterManager = character as AICharacterManager;
    }

    public override void DeathProcess(int newValue)
    {
        base.DeathProcess(newValue);
        _aiCharacterManager.aiCharacterDeathInteractable.PerformDeath();
        _aiCharacterManager.lockOnObject.PerformDeath();

        MapDataManager.Instance.AddKillLog(_aiCharacterManager.characterID);
    }
}
