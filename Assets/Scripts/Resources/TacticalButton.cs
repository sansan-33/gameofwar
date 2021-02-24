using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TacticalButton : MonoBehaviour
{

    public Button buttonComponent;
    public int TacticalId;
    public TacticalBehavior tb;
    // Use this for initialization
    void Start()
    {
        buttonComponent.onClick.AddListener(HandleClick);
    }
    
    public void HandleClick()
    {
        tb.TryTB(TacticalId);
    }
}
