using UnityEngine;

public class CardSlot : MonoBehaviour 
{
    [SerializeField] public GameObject mergeEffectPrefab;
    private GameObject mergeEffect;

    public void Start()
    {

        //if (mergeEffectPrefab != null)
  //      mergeEffect = Instantiate(mergeEffectPrefab, transform);
    //    Debug.Log($"{name} mergeEffectPrefab is null ? {mergeEffect == null} {mergeEffect.name}");
        //else
        //Debug.Log($"{name} mergeEffectPrefab is null");

        //if (UnitMeta.UnitEleixer.TryGetValue((UnitMeta.UnitType)type, out int value)) { uniteleixer = value; }
    }

    private void Update()
    {
             
    }
    public void playMergeEffect()
    {
//        if (mergeEffect != null)
            mergeEffect = Instantiate(mergeEffectPrefab, transform);

        //    mergeEffect.GetComponent<ParticleSystem>().Play();

      
        Debug.Log($"{name} play merge card effect");
    }

}                                                                          