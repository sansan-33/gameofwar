using UnityEngine;


    /// <summary>
    /// Interface for an agent that is able to attack.
    /// </summary>
    public interface ISpecialAttack
{

    /// <summary>
    /// Returns the furthest distance that the agent is able to attack from.
    /// </summary>
    /// <returns>The distance that the agent can attack from.</returns>
    void OnPointerDown();
    int GetSpCost();
    
}
