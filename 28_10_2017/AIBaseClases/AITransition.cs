using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A simple data class
[System.Serializable]
public class AITransition
{
    //Contains one decision and two states, one true other false. Depending on the condition it will transition to a false or true state.
    public AIDecision decision;
    public AIState trueState;
    public AIState falseState;
}
