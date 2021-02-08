using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TacticalStatus : MonoBehaviour
{

    [SerializeField] private TMP_Text statusText = null;
    private TacticalBehavior tb;

    // Start is called before the first frame update
    public void Awake()
    {
        tb = GameObject.FindGameObjectWithTag("TacticalSystem").GetComponent<TacticalBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        statusText.text = tb.GetTacticalStatus();
    }
}
