using UnityEngine;

public enum UnitType
{
    Land,
    Naval
}

public enum UnitStatus
{
    None,
    OnBoard,
    Fortified,
    Clearing,
    Building
}

public class Unit : MonoBehaviour
{
    [SerializeField]
    protected string unitName;
    public string UnitName { get { return unitName; } }

    [SerializeField]
    protected UnitType unitType;
    public UnitType UnitType { get { return unitType; } }

    [SerializeField]
    protected UnitStatus unitStatus;
    public UnitStatus UnitStatus { get { return unitStatus; } set { unitStatus = value; } }

    [SerializeField]
    protected int strength;
    public int Strength { get { return strength; } set { strength = value; } }

    [SerializeField]
    protected int movePoint;
    public int MovePoint { get { return movePoint; } set { movePoint = value; } }

    [SerializeField]
    protected int movePointMax;
    public int MovePointMax { get { return movePointMax; } }

    [SerializeField]
    protected Faction faction;
    public Faction Faction { get { return faction; } }

    [SerializeField]
    protected int visualRange;
    public int VisualRange { get { return visualRange; } }

    [SerializeField]
    private Vector2 curPos;
    public Vector2 CurPos { get { return curPos; } set { curPos = value; } }

    [SerializeField]
    protected Hex curHex;
    public Hex CurHex { get { return curHex; } set { curHex = value; } }

    [SerializeField]
    protected Hex targetHex;
    public Hex TargetHex { get { return targetHex; } set { targetHex = value; } }

    [SerializeField]
    protected bool isMoving = false;
    public bool IsMoving { get { return isMoving; } set { isMoving = value; } }

    [SerializeField]
    protected bool visible = false;
    public bool Visible { get { return visible; } set { visible = value; } }

    [Header("Unit")]
    [SerializeField]
    protected SpriteRenderer unitSprite;

    [Header("Flag")]
    [SerializeField]
    protected SpriteRenderer flagSprite;

    [Header("Border")]
    [SerializeField]
    protected SpriteRenderer borderSprite;

    [SerializeField]
    protected GameManager gameMgr;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupPosition(Hex hex)
    {
        curHex = hex;
        curPos = hex.Pos;
    }

    public void ToggleBorder(bool flag, Color32 color)
    {
        borderSprite.gameObject.SetActive(flag);
        borderSprite.color = color;
    }

    public void ShowHideSprite(bool flag)
    {
        unitSprite.gameObject.SetActive(flag);
        flagSprite.gameObject.SetActive(flag);
    }
}
