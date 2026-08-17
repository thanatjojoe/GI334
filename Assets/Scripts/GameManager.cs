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

    public static GameManager instance;

    void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetUpFaction();

        DetermineOcean();
        GenerateAllHexes();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
            ToggleHexText();
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
}
