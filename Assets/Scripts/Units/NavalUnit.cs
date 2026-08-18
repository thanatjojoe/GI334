using System.Collections.Generic;
using UnityEngine;

public enum NavalUnitType
{
    Caravel,
    Merchantman,
    Galleon,
    Privateer,
    Frigate,
    ManOWar
}

public class NavalUnit : Unit
{
    [SerializeField]
    private NavalUnitType navalUnitType;
    public NavalUnitType NavalUnitType { get { return navalUnitType; } set { navalUnitType = value; } }

    [SerializeField]
    private bool armed = false;

    [SerializeField]
    private int cargoHoldNum;

    [SerializeField]
    private List<LandUnit> passengers = new List<LandUnit>();
    public List<LandUnit> Passengers { get { return passengers; } set { passengers = value; } }

    [SerializeField]
    private GameObject passengerParent;
    public GameObject PassengerParent { get { return passengerParent; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UnitInit(GameManager gameMgr, Faction fact, NavalUnitData data)
    {
        base.gameMgr = gameMgr;
        faction = fact;
        flagSprite.sprite = fact.ShieldIcon;

        unitName = data.unitName;
        movePointMax = data.movePointMax;
        movePoint = data.movePointMax;
        strength = data.strength;
        visualRange = data.visualRange;
        unitSprite.sprite = data.shipIcon;

        navalUnitType = data.navalUnitType;
        armed = data.armed;
        cargoHoldNum = data.cargoHoldNum;
    }
}
