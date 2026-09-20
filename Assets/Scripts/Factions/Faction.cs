using UnityEngine;
using System.Collections.Generic;

public enum Nation
{
    England,
    France,
    Spain,
    Netherland,
    Portugal,
    Sioux,
    Tupi,
    Iroquois,
    Apache,
    Aztec,
    Huron,
    Maya,
    Inca
}

public class Faction : MonoBehaviour
{
    [SerializeField]
    private Nation nation;
    public Nation Nation { get { return nation; } }

    [SerializeField]
    private bool isEuropean;
    public bool IsEuropean { get { return isEuropean; } }

    [SerializeField]
    private Transform townParent;
    public Transform TownParent { get { return townParent; } }

    [SerializeField]
    private Transform unitParent;
    public Transform UnitParent { get { return unitParent; } }

    [SerializeField]
    private Sprite flagIcon;
    public Sprite FlagIcon { get { return flagIcon; } }

    [SerializeField]
    private Sprite shieldIcon;
    public Sprite ShieldIcon { get { return shieldIcon; } }

    [SerializeField]
    private Sprite unitIcon;
    public Sprite UnitIcon { get { return unitIcon; } }

    [SerializeField]
    private Sprite townIcon;
    public Sprite TownIcon { get { return townIcon; } }

    [SerializeField]
    private List<Town> towns = new List<Town>();
    public List<Town> Towns { get { return towns; } set { towns = value; } }

    [SerializeField]
    private List<Unit> units = new List<Unit>();
    public List<Unit> Units { get { return units; } set { units = value; } }

    [SerializeField]
    private bool isTropicalNatives;
    public bool IsTropicalNatives { get { return isTropicalNatives; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FactionInit(FactionData data)
    {
        nation = data.nation;
        isEuropean = data.isEuropean;
        flagIcon = data.flagIcon;
        shieldIcon = data.shieldIcon;
        unitIcon = data.unitIcon;
        townIcon = data.townIcon;
        isTropicalNatives = data.isTropicalNatives;
    }
}
