using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] private float damage = 100;
    [SerializeField] private float explodedTime = 10;
    [SerializeField] private float range = 0.25f;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Explode(explodedTime));
    }
    private IEnumerator Explode(float time)
    {
        yield return new WaitForSeconds(time);
        //Debug.Log($"1 {armies.Count}");
        Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(range, range, range));
        foreach(Collider unit in colliders)
        {
            if(unit.TryGetComponent<Unit>(out Unit _unit))
            {
                unit.GetComponent<Health>().DealDamage(damage);
            }
        }
        Destroy(gameObject);
    }
}
