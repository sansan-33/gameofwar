using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class CrownDisplay : NetworkBehaviour
{
    [SerializeField] private TMP_Text crownRedText = null;
    [SerializeField] private TMP_Text crownBlueText = null;
    [SyncVar]
    int crownRed = 0;
    [SyncVar]
    int crownBlue = 0;

    private void Start()
    {
        Health.HeroOrKingOnDie += CrownReward;
    }
    private void OnDestroy()
    {
        Health.HeroOrKingOnDie -= CrownReward;
    }
    private void CrownReward(string tag)
    {
        if(tag.Contains("1"))
            crownBlueText.text = ++crownBlue + "";
        else
            crownRedText.text = ++crownRed + "";
    }
    
}
