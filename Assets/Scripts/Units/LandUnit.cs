using UnityEngine;

public enum LandUnitType
{
    FreeColonists,
    VeteranSoldiers,
    HardyPioneers,
    PlainIndians,
    TropicalIndians,
    SeasonedScouts,
    Farmers,
    Fishermen,
    Lumberjacks,
    Carpenters,
    OreMiners,
    Blacksmiths
}

public class LandUnit : Unit
{
    [SerializeField]
    private LandUnitType landUnitType;
    public LandUnitType LandUnitType { get { return landUnitType; } set { landUnitType = value; } }

    [SerializeField]
    private int toolsNum; //max 100
    public int ToolsNum { get { return toolsNum; } set { toolsNum = value; } }

    [SerializeField]
    private int musketsNum; //max 50
    public int MusketsNum { get { return musketsNum; } set { musketsNum = value; } }

    [SerializeField]
    private int horseNum; //max 50
    public int HorseNum { get { return horseNum; } set { horseNum = value; } }

    [SerializeField]
    private bool armed = false;
    public bool Armed { get { return armed; } set { armed = value; } }

    [SerializeField]
    private bool hasMusket = false;
    public bool HasMusket { get { return hasMusket; } set { hasMusket = value; } }

    [SerializeField]
    private bool hasHorse = false;
    public bool HasHorse { get { return hasHorse; } set { hasHorse = value; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UnitInit(GameManager gameMgr, Faction fact, LandUnitData data)
    {
        base.gameMgr = gameMgr;
        faction = fact;
        flagSprite.sprite = fact.ShieldIcon;

        unitName = data.unitName;
        movePointMax = data.movePointMax;
        movePoint = data.movePointMax;
        strength = data.strength;
        visualRange = data.visualRange;
        unitSprite.sprite = data.unitIcon;
        unitStatus = UnitStatus.None;

        landUnitType = data.landUnitType;
        if (landUnitType == LandUnitType.HardyPioneers)
            toolsNum = 100;

        armed = data.armed;

        hasMusket = data.hasMusket;
        if (hasMusket)
            musketsNum = 50;

        hasHorse = data.hasHorse;
        if (hasHorse)
            horseNum = 50;
    }
}
