using UnityEngine;

[CreateAssetMenu(fileName = "LandUnitData", menuName = "Scriptable Objects/LandUnitData")]
public class LandUnitData : ScriptableObject
{
    public string unitName;
    public LandUnitType landUnitType;
    public int strength;
    public int movePointMax;
    public int visualRange;
    public Sprite unitIcon;
    public bool armed;
    public bool hasMusket;
    public bool hasHorse;
}
