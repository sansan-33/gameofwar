using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventTrigger : MonoBehaviour {

    [SerializeField] UnityEvent triggerEvents;

    public void TriggerEvent () {
        triggerEvents.Invoke();
    }

}