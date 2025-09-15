using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldDatabase_Perk : Singleton<WorldDatabase_Perk>
{
    [SerializeField] private List<Perk> perks = new List<Perk>();
    public bool IsDataLoaded { get; private set; }
    
    public Dictionary<int, Perk> PerkDict = new Dictionary<int, Perk>();
    public OneToManyMap<int, int> SubPerkDict = new OneToManyMap<int, int>();
    public Dictionary<int, int> MainPerkDict = new Dictionary<int, int>();
    protected override void Awake()
    {
        base.Awake();
        IsDataLoaded = false;
        LoadData();
    }
    
    private void LoadData()
    {
        foreach (var perk in perks)
        {
            PerkDict.Add(perk.perkId, perk);

            if (perk.perkId % 10 == 0)
            {
                // main perk
                if(perk.RequiredPerkId == 0) continue;
                MainPerkDict.Add(perk.RequiredPerkId, perk.perkId);
            }
            else
            {
                // sub perk
                SubPerkDict.Add(perk.RequiredPerkId, perk.perkId);
            }
        }
        
        IsDataLoaded = true;
    }
}
