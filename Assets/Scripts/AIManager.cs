using UnityEngine;
using System.Collections;

public class AIManager : MonoBehaviour
{
    public static AIManager instance;

    void Awake()
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

    private void AutoMoveToHex(Unit unit)
    {
        int n = Random.Range(0, 6);

        Hex toGoHex = HexCalculator.FindHexByDir(unit.CurHex, (HexDirection)n, GameManager.instance.AllHexes);

        if (toGoHex == null)
            return;

        unit.ShowHideSprite(true);
        unit.PrepareMoveToHex(toGoHex);
    }

    private IEnumerator EnemyUnitMoves()
    {
        foreach (Faction faction in GameManager.instance.Factions)
        {
            GameManager.instance.ResetAllUnits(faction);

            if (faction == GameManager.instance.PlayerFaction)
                continue;


            foreach (Unit unit in faction.Units)
            {
                GameManager.instance.SelectAiUnit(unit);

                if (unit.Visible)
                    CameraController.instance.MoveCamera(unit.CurPos);

                if (unit.UnitStatus == UnitStatus.OnBoard)
                    continue;
                //Debug.Log("Move");
                AutoMoveToHex(unit);

                yield return new WaitForSeconds(0.1f);
            }
        }
        GameManager.instance.GameTurn++;
        //New Turn Dialog and Focus on Player's unit
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        //Debug.Log("Release Mouse");
        //Debug.Log("New Turn");

        GameManager.instance.PlayerTurn = true;
        GameManager.instance.SelectPlayerFirstUnit();
    }

    public void StartAITurn()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        StartCoroutine(EnemyUnitMoves());
    }
}
