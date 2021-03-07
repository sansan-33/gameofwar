using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Mirror;

public class TacticalLeaderButton : MonoBehaviour
{

    public Button buttonComponent;
    public int TacticalId;
    public Unit unit;

    private static TacticalBehavior tb;
    // Use this for initialization
    void Start()
    {
        tb = GameObject.FindGameObjectWithTag("TacticalSystem").GetComponent<TacticalBehavior>();
        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(HandleClick);
    }
    
    public void HandleClick()
    {
        this.transform.parent.gameObject.SetActive(false);
        tb.TryTB(TacticalId, unit.unitType);
    }
}
