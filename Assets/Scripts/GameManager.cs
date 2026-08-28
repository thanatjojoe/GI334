using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Grid grid;

    [SerializeField]
    private Transform hexParent;

    [SerializeField]
    private GameObject hexPrefab;

    public const int WIDTH = 50; //no. of Column in this map
    public const int HEIGHT = 60; //no. of Row in this map

    [SerializeField]
    private Hex[,] allHexes = new Hex[WIDTH, HEIGHT];
    public Hex[,] AllHexes { get { return allHexes; } }

    [SerializeField]
    private HexData[] hexData;
    public HexData[] HexData { get { return hexData; } }

    [SerializeField]
    private bool showingText;

    [SerializeField]
    private int oceanEdgeIndex;

    [SerializeField]
    private Faction playerFaction;
    public Faction PlayerFaction { get { return playerFaction; } }

    [SerializeField]
    private Faction[] factions; //England, France, Spain, Netherland, Portugal
    public Faction[] Factions { get { return factions; } }

    [SerializeField]
    private FactionData[] factionData;
    public FactionData[] FactionData { get { return factionData; } }

    [SerializeField]
    private GameObject landUnitPrefab;

    [SerializeField]
    private GameObject navalUnitPrefab;

    [SerializeField]
    private GameObject townPrefab;

    [SerializeField]
    private Unit curUnit;
    public Unit CurUnit { get { return curUnit; } set { curUnit = value; } }

    [SerializeField]
    private Unit curAiUnit;
    public Unit CurAiUnit { get { return curAiUnit; } set { curAiUnit = value; } }

    [SerializeField]
    private LandUnitData[] landUnitData;
    public LandUnitData[] LandUnitData { get { return landUnitData; } }

    [SerializeField]
    private NavalUnitData[] navalUnitData;
    public NavalUnitData[] NavalUnitData { get { return navalUnitData; } }

    [SerializeField]
    private bool playerTurn = true;
    public bool PlayerTurn { get { return playerTurn; } set { playerTurn = value; } }

    [SerializeField]
    private int gameTurn = 1;
    public int GameTurn { get { return gameTurn; } set { gameTurn = value; } }

    [SerializeField]
    private int nativeTownNum;

    public static GameManager instance;

    void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetUpFaction();
        SelectPlayerFaction();
        DetermineOcean();
        GenerateAllHexes();

        GenerateAllEuropeanShips();
        GenerateAllEuropeanExplorerUnits();
        GenerateAllNativeTowns();
        GenerateAllNativeUnits();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
            ToggleHexText();

        if (Keyboard.current.tabKey.wasPressedThisFrame)
            SelectNextPlayerUnit();
    }

    private void GenerateAllHexes()
    {
        for (int x = 0; x < WIDTH; x++)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                Vector3 hexPos = grid.GetCellCenterWorld(new Vector3Int(x, y));
                //Debug.Log(hexPos);

                GameObject hexObj = Instantiate(hexPrefab, hexPos, Quaternion.identity, hexParent);
                Hex hex = hexObj.GetComponent<Hex>();

                int n = Random.Range(oceanEdgeIndex - 3, oceanEdgeIndex + 4);

                if (x >= n)
                    hex.HexInit(x, y, hexPos, this, 0);//Ocean
                else
                {
                    int i = Random.Range(1, hexData.Length);
                    hex.HexInit(x, y, hexPos, this, i);//Land
                }

                //Debug.Log($"{x}:{y}");
                allHexes[x, y] = hex;
            }
        }
    }

    private void ToggleHexText()
    {
        foreach (Hex hex in allHexes)
            hex.ToggleAllBasicText(!showingText);

        showingText = !showingText;
    }

    private void DetermineOcean()
    {
        oceanEdgeIndex = WIDTH - Random.Range(7, 10);
        //Debug.Log($"min:{oceanEdgeIndex}");
    }

    private void SetUpFaction()
    {
        for (int i = 0; i < factions.Length; i++)
        {
            factions[i].FactionInit(factionData[i]);
        }
    }

    public void SelectPlayerFaction()
    {
        int i = 3; //Netherland
        playerFaction = factions[i];
    }

    private void GenerateEuropeanShip(Faction faction)
    {
        int x = WIDTH - 1; //near right edge of a map
        int y = Random.Range(0, HEIGHT);
        Hex hex = allHexes[x, y];

        GameObject obj = Instantiate(navalUnitPrefab, hex.Pos, Quaternion.identity, faction.UnitParent);
        NavalUnit ship = obj.GetComponent<NavalUnit>();

        ship.UnitInit(this, faction, navalUnitData[0]); //Caravel
        ship.SetupPosition(hex);
        faction.Units.Add(ship); //First Unit of European nations is a ship

        if (faction == playerFaction)
        {
            ClearDarkFogAroundUnit(ship);
            SelectPlayerUnit(ship);
            CameraController.instance.MoveCamera(ship.CurPos);
            ship.Visible = true;
        }
    }

    private void GenerateAllEuropeanShips()
    {
        for (int i = 0; i < 5; i++)
        {
            GenerateEuropeanShip(factions[i]);
        }
    }

    public void ShowToggleBorder(Unit unit)
    {
        if (unit.Faction == playerFaction)
            unit.ToggleBorder(true, Color.green);
        else
            unit.ToggleBorder(true, Color.red);
    }

    public void ClearToggleBorder(Unit unit)
    {
        unit.ToggleBorder(false, Color.green);
    }

    public void FocusPlayerUnit(Unit unit)
    {
        ShowToggleBorder(unit);
    }

    public void ClearDarkFogAroundUnit(Unit unit)
    {
        unit.CurHex.DiscoverHex();

        List<Hex> adjHexes = HexCalculator.GetHexAround(allHexes, unit.CurHex);

        //Debug.Log(adjHexes.Count);

        foreach (Hex hex in adjHexes)
        {
            hex.DiscoverHex();
        }
    }

    public void SelectPlayerUnit(Unit unit)
    {
        if (curUnit != null)
        {
            ClearToggleBorder(curUnit);
            curUnit.SetUnitToNormalLayerOrder();

            if (curUnit.UnitStatus == UnitStatus.OnBoard)
                curUnit.gameObject.SetActive(false);
        }

        unit.gameObject.SetActive(true);
        unit.SetUnitToFrontLayerOrder();

        curUnit = unit;
        //UpdateCanGoHex();

        FocusPlayerUnit(curUnit);
        //Debug.Log(curUnit);
    }

    public bool CheckIfHexIsAdjacent(Hex centerHex, Hex targetHex)
    {
        List<Hex> adjHexes = HexCalculator.GetHexAround(allHexes, centerHex);

        return (adjHexes.Contains(targetHex)) ? true : false;
    }

    public void LeaveSeenFogAroundUnit(Unit unit)
    {
        unit.CurHex.SeenHex();

        List<Hex> adjHexes = HexCalculator.GetHexAround(allHexes, unit.CurHex);

        //Debug.Log(adjHexes.Count);

        foreach (Hex hex in adjHexes)
        {
            hex.SeenHex();
        }
    }

    public void ClearDarkFogAroundEveryUnit(Faction faction)
    {
        foreach (Unit unit in faction.Units)
        {
            //Debug.Log($"{unit.UnitName} discovers:");
            ClearDarkFogAroundUnit(unit);
        }
    }

    private void GeneratePassengerUnit(Faction faction, Hex hex, int unitId, bool show, NavalUnit ship)//ship passengers
    {
        GameObject obj = Instantiate(landUnitPrefab, hex.Pos, Quaternion.identity, ship.PassengerParent.transform);
        LandUnit unit = obj.GetComponent<LandUnit>();

        unit.UnitInit(this, faction, landUnitData[unitId]);
        unit.SetupPosition(hex);

        unit.UnitStatus = UnitStatus.OnBoard;
        unit.TransportShip = ship;
        obj.SetActive(false);

        faction.Units.Add(unit);
        ship.Passengers.Add(unit);
    }

    private void GenerateAllEuropeanExplorerUnits()
    {
        for (int i = 0; i < 5; i++)
        {
            NavalUnit firstShip = factions[i].Units[0].gameObject.GetComponent<NavalUnit>();

            GeneratePassengerUnit(factions[i], firstShip.CurHex, 1, false, firstShip); //Veteran Soldiers
            GeneratePassengerUnit(factions[i], firstShip.CurHex, 2, false, firstShip); //Hardy Pioneers
        }
    }

    private int FindIndexOfCurUnit()
    {
        if (playerFaction.Units.Contains(curUnit))
        {
            for (int i = 0; i < playerFaction.Units.Count; i++)
            {
                if (curUnit == playerFaction.Units[i])
                    return i;
            }
            return -1;
        }
        else
            return -1;
    }

    private void SelectNextPlayerUnit()
    {
        int i = FindIndexOfCurUnit();
        i++;

        if (i >= playerFaction.Units.Count)
            i = 0;

        SelectPlayerUnit(playerFaction.Units[i]);
        CameraController.instance.MoveCamera(curUnit.transform.position);
    }

    public void GenerateTown(Faction faction, Hex curHex)
    {
        GameObject obj = Instantiate(townPrefab, curHex.Pos, Quaternion.identity, faction.TownParent);
        Town town = obj.GetComponent<Town>();

        town.TownInit(this, faction);
        town.CurHex = curHex;
        town.CurPos = town.CurHex.Pos;
        faction.Towns.Add(town);

        curHex.HasTown = true;
    }

    private void GenerateAllNativeTowns()
    {
        for (int i = 5; i < factions.Length; i++)
        {
            nativeTownNum = Random.Range(5, 10);

            for (int j = 0; j < nativeTownNum; j++)
            {
                int landEdge = oceanEdgeIndex - 1;

                int x = Random.Range(0, landEdge);
                int y = Random.Range(0, HEIGHT);
                Hex hex = allHexes[x, y];

                if (HexCalculator.CheckIfHexAroundHasTown(allHexes, hex))
                    continue;

                if (hex.HexType != HexType.Ocean)
                    GenerateTown(factions[i], hex);
            }
        }
    }

    private void GenerateLandUnit(Faction faction, Hex hex, int unitId, bool show)//normal land units
    {
        GameObject obj = Instantiate(landUnitPrefab, hex.Pos, Quaternion.identity, faction.UnitParent);
        LandUnit unit = obj.GetComponent<LandUnit>();

        unit.UnitInit(this, faction, landUnitData[unitId]);
        unit.SetupPosition(hex);
        unit.ShowHideSprite(show);

        faction.Units.Add(unit);
    }

    private void GenerateAllNativeUnits()
    {
        for (int i = 5; i < factions.Length; i++)
        {
            foreach (Town town in factions[i].Towns)
            {
                GenerateLandUnit(factions[i], town.CurHex, 4, false); //Tropical Indian
            }
        }
    }
}
