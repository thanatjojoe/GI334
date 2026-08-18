using UnityEngine;

[CreateAssetMenu(fileName = "NavalUnitData", menuName = "Scriptable Objects/NavalUnitData")]
public class NavalUnitData : ScriptableObject
{
    public string unitName;
    public NavalUnitType navalUnitType;
    public int strength;
    public int movePointMax;
    public int visualRange;
    public Sprite shipIcon;
    public bool armed;
    public int cargoHoldNum;
}
