using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject townPanel;
    public GameObject TownPanel { get { return townPanel; } set { townPanel = value; } }

    [SerializeField]
    private TerrainSlot centerSlot;

    [SerializeField]
    private TerrainSlot[] areaSlots;

    [SerializeField]
    private GameObject unitDragPrefab;

    [SerializeField]
    private GameObject outsideTownParent;

    [SerializeField]
    private GameObject yieldIconPrefab;
    public GameObject YieldIconPrefab { get { return yieldIconPrefab; } }

    [SerializeField]
    private List<GameObject> allUnitDrags;
    public List<GameObject> AllUnitDrags { get { return allUnitDrags; } set { allUnitDrags = value; } }

    public static UIManager instance;

    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupHexSlots(Hex centerHex, Hex[] aroundHexes)
    {
        centerSlot.HexSlotInit(centerHex);

        for (int i = 0; i < areaSlots.Length; i++)
        {
            areaSlots[i].HexSlotInit(aroundHexes[i]);
        }
    }

    private void DestroyOldUnitDrag()
    {
        foreach (GameObject obj in allUnitDrags)
        {
            Destroy(obj);
        }
        allUnitDrags.Clear();
    }

    private void HideUnitWorkInTown(Hex hex)
    {
        foreach (Unit unit in hex.UnitsInHex)
        {
            if (unit.UnitStatus == UnitStatus.WorkInField)
                unit.gameObject.SetActive(false);
        }
    }

    public void SetupUnitDragOutsideTown(Hex hex)
    {
        foreach (Unit unit in hex.UnitsInHex)
        {
            if (unit.UnitType == UnitType.Land && unit.UnitStatus == UnitStatus.None)
            {
                GameObject unitObj = Instantiate(unitDragPrefab, outsideTownParent.transform);
                allUnitDrags.Add(unitObj);

                UnitDrag unitDrag = unitObj.GetComponent<UnitDrag>();
                unitDrag.UnitInit((LandUnit)unit);
            }
        }
    }

    public void SetupUnitDragWorkingInTerrain()
    {
        foreach (TerrainSlot terrainSlot in areaSlots)
        {
            if (terrainSlot.Hex == null)
                continue;

            if (terrainSlot.Hex.Labor != null)
            {
                GameObject unitObj = Instantiate(unitDragPrefab, terrainSlot.LaborParent);
                allUnitDrags.Add(unitObj);

                UnitDrag unitDrag = unitObj.GetComponent<UnitDrag>();
                unitDrag.UnitInit((LandUnit)terrainSlot.Hex.Labor);

                unitDrag.IconParent = terrainSlot.LaborParent;
                unitDrag.WorkAtNewTerrainSlot(terrainSlot);
            }
        }
    }

    public void ToggleTownPanel(bool show)
    {
        if (show == false)
        {
            DestroyOldUnitDrag();
            HideUnitWorkInTown(GameManager.instance.CurTown.CurHex);
        }
        townPanel.SetActive(show);
    }

    public void SetupCurrentTown(Hex curHex, Hex[] aroundHexes)
    {
        SetupHexSlots(curHex, aroundHexes);
        SetupUnitDragOutsideTown(curHex);
        SetupUnitDragWorkingInTerrain();
    }
}
