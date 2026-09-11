using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class TerrainSlot : MonoBehaviour, IDropHandler
{
    [SerializeField]
    private Hex hex;
    public Hex Hex { get { return hex; } }

    [SerializeField]
    private Image terrainImage;

    [SerializeField]
    private Image forestImage;

    [SerializeField]
    private Image townImage;

    [SerializeField]
    private Transform yieldParent;

    [SerializeField]
    private Transform laborParent;
    public Transform LaborParent { get { return laborParent; } }

    //0-food, 1-sugar, 2-tobacco 3-cotton 4-fur 5-lumber 6-ore 7-silver
    [SerializeField]
    private int[] normalYield = new int[8];
    public int[] NormalYield { get { return normalYield; } set { normalYield = value; } }

    [SerializeField]
    private int[] actualYield = new int[8]; //Actual Yield labor is currently working
    public int[] ActualYield { get { return actualYield; } set { actualYield = value; } }

    [SerializeField]
    private bool centerHex;

    [SerializeField]
    private TMP_Text yieldText;

    [SerializeField]
    private List<GameObject> yieldIconList = new List<GameObject>();
    public List<GameObject> YieldIconList { get { return yieldIconList; } set { yieldIconList = value; } }

    [SerializeField]
    private GameManager gameMgr;
    [SerializeField]
    private UIManager uiMgr;

    void Awake()
    {
        gameMgr = GameManager.instance;
        uiMgr = UIManager.instance;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (centerHex || hex.Labor != null)
            return;

        GameObject unitObj = eventData.pointerDrag;
        UnitDrag unitDrag = unitObj.GetComponent<UnitDrag>();

        unitDrag.QuitOldTerrainSlot(); //old slot remove this labor
        unitDrag.WorkAtNewTerrainSlot(this); //this slot is remembered in this labor

        unitDrag.IconParent = laborParent;
        unitObj.transform.position = laborParent.position;

        unitDrag.LandUnit.UnitStatus = UnitStatus.WorkInField;
        hex.Labor = unitDrag.LandUnit;
    }

    public void HexSlotInit(Hex hexData)
    {
        if (hexData != null)
        {
            hex = hexData;
            terrainImage.sprite = hex.TerrainSprite.sprite;

            if (hex.HasForest)
            {
                forestImage.sprite = hex.ForestSprite.sprite;
                forestImage.gameObject.SetActive(true);
            }

            if (hex.HasTown)
            {
                townImage.sprite = hex.Town.TownSprite.sprite;
                townImage.gameObject.SetActive(true);
            }
        }
    }
}
