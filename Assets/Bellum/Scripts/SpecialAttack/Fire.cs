using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    [SerializeField] GameObject firePrefab;
    [SerializeField] GameObject firePrefabChild1;
    [SerializeField] GameObject firePrefabChild2;
    [SerializeField] private float burnTime = 10;
    [SerializeField] private int maxSpawn = 3;
    [SerializeField] private int damage = 500;
    [SerializeField] private int diedTime = 20;
    bool bigFire = true;
    private Transform parent;
    // Start is called before the first frame update
    void Start()
    {
        parent = GameObject.FindGameObjectWithTag("SpecialAttackManager").transform;
        StartCoroutine(BurnAround());
        StartCoroutine(DestroySelf());
    }
    private IEnumerator DestroySelf()
    {
        //Debug.Log("start destroy");
        yield return new WaitForSeconds(diedTime);
        //Debug.Log("destroy");
        Destroy(gameObject);
    }
    public void HandleScale(bool fromBigFire = false)
    {
        if (fromBigFire == true)
        {

                firePrefabChild1.transform.localScale /= 3;
            firePrefabChild2.transform.localScale /= 3;
        }
        this.bigFire = false;

    }

    private IEnumerator BurnAround()
    {
        while (maxSpawn >= 0)
        {
            maxSpawn--;
               yield return new WaitForSeconds(5);
            float vector = bigFire ? 7 : 7 / 4;
            Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(vector, vector, vector));
            Debug.Log($"collideed {colliders.Length}");
            foreach (Collider unit in colliders)
            {
                if (unit.TryGetComponent<Health>(out Health health))
                {
                    Debug.Log($"deal damage to {unit.name}");
                    health.DealDamage(damage);
                }
            }
            if (bigFire == false)
            {
                Vector3 pos = transform.position;
                float randomX = Random.Range(pos.x - 2, pos.x + 2);
                float randomZ = Random.Range(pos.z - 2, pos.z + 2);
                GameObject fire = Instantiate(firePrefab, parent);
                fire.GetComponent<Fire>().HandleScale(bigFire);
                fire.transform.position = new Vector3(randomX, 0, randomZ);
            }
            else
            {
                Vector3 pos = transform.position;
                float _pos = Random.Range(0, 3);
                float randomX = 0;
                float randomZ = 0;
                switch (_pos)
                {
                    case 0:
                        randomX = Random.Range(pos.x - 3, pos.x - 5);
                        randomZ = Random.Range(pos.z - 3, pos.z - 5);
                        break;
                    case 1:
                        randomX = Random.Range(pos.x - 3, pos.x - 5);
                        randomZ = Random.Range(pos.z + 3, pos.z + 5);
                        break;
                    case 2:
                        randomX = Random.Range(pos.x + 3, pos.x + 5);
                        randomZ = Random.Range(pos.z - 3, pos.z - 5);
                        break;
                    case 3:
                        randomX = Random.Range(pos.x + 3, pos.x + 5);
                        randomZ = Random.Range(pos.z + 3, pos.z + 5);
                        break;
                }

                GameObject fire = Instantiate(firePrefab, parent);
                fire.GetComponent<Fire>().HandleScale(bigFire);
                fire.transform.position = new Vector3(randomX, 0, randomZ);

            }

        }
        
        
        // Update is called once per frame

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(7, 7, 7));
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(7 / 4, 7 / 4, 7 / 4));
    }
}
