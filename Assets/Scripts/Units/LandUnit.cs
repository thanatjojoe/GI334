using UnityEngine;
using UnityEngine.InputSystem;

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

    [SerializeField]
    private NavalUnit transportShip;
    public NavalUnit TransportShip { get { return transportShip; } set { transportShip = value; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            if (this == gameMgr.CurUnit)
                BuildSettlement();
        }

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (this == gameMgr.CurUnit)
                ClearingLand();
        }
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

    private void MakeLandFall()
    {
        unitStatus = UnitStatus.None;
        gameObject.transform.parent = faction.UnitParent.transform;
        transportShip.Passengers.Remove(this);
        transportShip = null;
    }

    public void ClearingLand()
    {
        if (unitStatus == UnitStatus.Clearing)
            return;

        if (curHex.HexType == HexType.Ocean || curHex.HexType == HexType.Mountains
            || curHex.HexType == HexType.Hills || !curHex.HasForest)
        {
            //warning has to be cleared land
            Debug.Log("Must be on Forest");
        }
        else if (toolsNum < 20)
        {
            //warning not enough tools
            Debug.Log("Not enough tools");
        }
        else
        {
            unitStatus = UnitStatus.Clearing;
            toolsNum -= 20;
        }
    }

    public void BuildSettlement()
    {
        if (unitStatus == UnitStatus.Building || curHex.HasTown)
            return;

        Debug.Log("Build Settlement");

        if (curHex.HexType == HexType.Ocean || curHex.HexType == HexType.Mountains || curHex.HasForest)
        {
            //warning has to be cleared land
            Debug.Log("Must be on Cleared Land");
        }
        else if (toolsNum < 20)
        {
            //warning not enough tools
            Debug.Log("Not enough tools");
        }
        else
        {
            unitStatus = UnitStatus.Building;
            toolsNum -= 20;
        }
    }

    public override void PrepareMoveToHex(Hex targetHex) //Begin to move by RC or AI auto movement
    {
        //Debug.Log($"{unitName}:walks");

        if (targetHex.HexType != HexType.Ocean)
        {
            base.PrepareMoveToHex(targetHex);

            if (unitStatus == UnitStatus.OnBoard)
                MakeLandFall();
        }
        else if (gameMgr.CheckIfHexHasOurShipToBoard(targetHex))
        {
            base.PrepareMoveToHex(targetHex);
        }
        else
        {
            StayOnHex(curHex);
        }
    }

    public void BoardingShip(NavalUnit ship)
    {
        ship.Passengers.Add(this);
        transportShip = ship;
        gameObject.transform.parent = ship.PassengerParent.transform;
        unitStatus = UnitStatus.OnBoard;
    }

    protected override void StayOnHex(Hex hex)
    {
        base.StayOnHex(hex);

        if (hex.HexType != HexType.Ocean)
            return;

        //Check again if this ship can be boarded
        NavalUnit ship = gameMgr.CheckIfHexHasOurShipToBoard(hex);

        if (ship != null)
            BoardingShip(ship);
    }
}
