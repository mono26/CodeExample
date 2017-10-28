using UnityEngine;

//Base class for any decision in the game. Can be implemented for Controller specific decision
//and it also allows to cast the Controller into a specific one
public abstract class AIDecision : ScriptableObject
{
    public abstract bool Decide(AIController controller);
}
