using UnityEngine;


    /// <summary>
    /// Interface for an agent that is able to attack.
    /// </summary>
    public interface IAttack 
    {
        /// <summary>
        /// Returns the furthest distance that the agent is able to attack from.
        /// </summary>
        /// <returns>The distance that the agent can attack from.</returns>
        void ScaleAttackDelay(float factor);

        /// <summary>
        /// Returns the maximum angle that the agent can attack from.
        /// </summary>
        /// <returns>The maximum angle that the agent can attack from.</returns>
        void ScaleDamageDeal(int attack, float repeatAttackDelay, float factor);

        void ScaleAttackRange(float factor);

        float RepeatAttackDelay();

        float AttackDistance();
    void ChangeAttackDelay(double changeValue);
}
