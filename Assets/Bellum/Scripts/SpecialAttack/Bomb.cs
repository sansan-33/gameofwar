using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] private float damage = 100;
    [SerializeField] private float explodedTime = 10;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Explode(explodedTime));
    }
    private IEnumerator Explode(float time)
    {
        yield return new WaitForSeconds(time);
        GameObject[] units = GameObject.FindGameObjectsWithTag("Player" + 1);
        GameObject[] provokeUnit = GameObject.FindGameObjectsWithTag("Provoke" + 1);
        GameObject[] sneakUnit = GameObject.FindGameObjectsWithTag("Sneaky" + 1);
        GameObject king = GameObject.FindGameObjectWithTag("King" + 1);
        //GameObject[] bomb = GameObject.FindGameObjectsWithTag("Bomb");
        List<GameObject> armies = new List<GameObject>();
        armies = units.ToList();

        provokeUnit.CopyTo(armies);
        if (king != null) { armies.Add(king); }

        //Debug.Log($"1 {armies.Count}");
        sneakUnit.CopyTo(armies);
        foreach(GameObject unit in armies)
        {
            unit.GetComponent<Health>().DealDamage(damage);
        }
        Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
