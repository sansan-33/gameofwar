using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effects : MonoBehaviour
{

    public GameObject[] effect;
    public Transform[] effectTransform;

    public Animator animator;

    public void AttackEffect(int number)
    {
        animator.SetInteger("SkillNumber", number);
        animator.SetTrigger("attack");
    }
    public void useEffect(int number)
    {
        Instantiate(effect[number], effectTransform[number].position, effectTransform[number].rotation);
    }
}
