using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    // Start is called before the first frame update
    public float shieldHealth = 0;
    [SerializeField] private ParticleSystem ShieldEffect;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(shieldHealth > 0)
        {
            Instantiate(ShieldEffect, this.transform).transform.localScale = new Vector3(5, 5, 5);
        }
    }
}
