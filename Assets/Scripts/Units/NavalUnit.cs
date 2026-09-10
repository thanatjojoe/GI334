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
    public int CargoHoldNum { get { return cargoHoldNum; } }

    [SerializeField]
    private List<LandUnit> passengers = new List<LandUnit>();
    public List<LandUnit> Passengers { get { return passengers; } set { passengers = value; } }

    [SerializeField]
    private GameObject passengerParent;
    public GameObject PassengerParent { get { return passengerParent; } }

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

    public override void PrepareMoveToHex(Hex targetHex) //Begin to move by RC or AI auto movement
    {
        base.PrepareMoveToHex(targetHex);

        if (targetHex.HexType != HexType.Ocean)
        {
            StayOnHex(curHex);
        }
    }

    protected override void StayOnHex(Hex hex)
    {
        base.StayOnHex(hex);

        foreach (LandUnit passenger in passengers)
        {
            //Remove passenger from old ocean hex
            passenger.CurHex.UnitsInHex.Remove(passenger);
            //Add passenger to new ocean hex
            hex.UnitsInHex.Add(passenger);

            passenger.CurHex = hex;
            passenger.CurPos = hex.Pos;
        }
    }
}
