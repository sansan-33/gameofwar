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
    [SerializeField] private GameObject bombFX = null;
    int playerid = 0;
    int enemyid = 0;
    public  int ID;
    // Start is called before the first frame update
    void Start()
    {
        RTSPlayer player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        playerid = player.GetPlayerID();
        enemyid = player.GetEnemyID();
        Invoke("Explode", explodedTime);
    }
    public void Explode()
    {
       // yield return new WaitForSeconds(time);
        //Debug.Log($"1 {armies.Count}");
        Debug.Log($"range = {range}");
        //gameObject.layer = LayerMask.NameToLayer("Projectile");
        Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(range, range, range));
        foreach(Collider unit in colliders)
        {
            Debug.Log($"Bomb{unit.name}");
            if(unit.TryGetComponent<Unit>(out Unit _unit))
            {
                unit.GetComponent<Health>().DealDamage(damage);
            }else if (unit.TryGetComponent<Fire>(out Fire fire))
            {
                if (((RTSNetworkManager)NetworkManager.singleton).Players.Count == 1)
                {
                    int id = ID == 0 ? 1 : 0;
                    //Debug.Log($"fire tag = {fire.tag} ID = {Id}");
                    if (fire.CompareTag("Fire" + id))
                    {

                        fire.DestroyFire();
                    }  //check to see if it belongs to the player, if it does, do nothing
                }
                else // Multi player seneriao
                {
                            if (fire.CompareTag("Fire" + enemyid))
                            {
                                fire.DestroyFire();
                            }
                }
            }
        }
        Instantiate(bombFX).transform.position = transform.position ;
        Destroy(gameObject);
    }
    public void ChangeDamage(int damage)
    {
        this.damage *= damage;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(range, range, range));
    }
}
