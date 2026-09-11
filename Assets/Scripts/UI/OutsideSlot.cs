using UnityEngine;
using UnityEngine.EventSystems;

public class OutsideSlot : MonoBehaviour, IDropHandler
{
    [SerializeField]
    private Town town;

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
        GameObject unitObj = eventData.pointerDrag;
        UnitDrag unitDrag = unitObj.GetComponent<UnitDrag>();
        unitDrag.IconParent = transform;

        unitDrag.QuitOldTerrainSlot(); //old slot remove this labor
        unitDrag.LandUnit.UnitStatus = UnitStatus.None;
    }
}
