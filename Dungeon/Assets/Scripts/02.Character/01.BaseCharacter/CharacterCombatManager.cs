using System.Collections.Generic;
using UnityEngine;

public class CharacterCombatManager : MonoBehaviour
{
    protected CharacterManager character;
    
    [HideInInspector] public EquipmentItemInfoWeapon equipmentItemInfoWeapon;
    [HideInInspector] public Dir currentAttackDir;

    private Dictionary<Dir, int> _dirByAnimationDic;

    #region Attack Hash

    private readonly int _attackUp = Animator.StringToHash("Attack_Up");
    private readonly int _attackUpRight = Animator.StringToHash("Attack_UpRight");
    private readonly int _attackRight = Animator.StringToHash("Attack_Right");
    private readonly int _attackDownRight = Animator.StringToHash("Attack_DownRight");
    private readonly int _attackDown = Animator.StringToHash("Attack_Down");
    private readonly int _attackDownLeft = Animator.StringToHash("Attack_DownLeft");
    private readonly int _attackLeft = Animator.StringToHash("Attack_Left");
    private readonly int _attackUpLeft = Animator.StringToHash("Attack_UpLeft");

    #endregion

    protected virtual void Awake()
    {
        character = GetComponent<CharacterManager>();
        SetDirDic();
    }

    private void SetDirDic()
    {
        _dirByAnimationDic = new Dictionary<Dir, int>
        {
            { Dir.Up, _attackUp },
            { Dir.UpRight, _attackUpRight },
            { Dir.Right, _attackRight },
            { Dir.DownRight, _attackDownRight },
            { Dir.Down, _attackDown },
            { Dir.DownLeft, _attackDownLeft },
            { Dir.Left, _attackLeft },
            { Dir.UpLeft, _attackUpLeft }
        };
    }

    public void PerformWeaponDirAction(Dir dir)
    {
        if (_dirByAnimationDic.TryGetValue(dir, out int animationHash))
        {
            currentAttackDir = dir;
            character.characterAnimatorManager.PlayTargetAttackActionAnimation(equipmentItemInfoWeapon, animationHash);
        }
        else
        {
            Debug.LogWarning("[CCM] No Animation");
        }
    }
}