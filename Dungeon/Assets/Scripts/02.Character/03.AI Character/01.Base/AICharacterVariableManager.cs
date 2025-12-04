public class AICharacterVariableManager : CharacterVariableManager
{
    private AICharacterManager _aiCharacterManager;

    protected override void Awake()
    {
        base.Awake();
        _aiCharacterManager = character as AICharacterManager;
    }

    protected override void Start()
    {
        base.Start();
        CLVM.isWalking = true;
    }

    public override void DeathProcess(float newValue)
    {
        base.DeathProcess(newValue);
        _aiCharacterManager.aiCharacterDeathInteractable.PerformDeath();
        _aiCharacterManager.lockOnObject.PerformDeath();

        // MapDataManager.Instance.AddKillLog(_aiCharacterManager.characterID);
    }
}
