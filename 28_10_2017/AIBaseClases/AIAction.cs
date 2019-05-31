using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base abstract class for an action. Reciebes an AIController
//in the class implementation the AIController can be cast into another controller for specific actions.
public abstract class AIAction : ScriptableObject
{
    public abstract void DoAction(AIController controller);
}
