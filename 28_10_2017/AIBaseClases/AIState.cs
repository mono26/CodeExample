using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class packs all the other AI clases, decision, transition and action.
//A scriptableobject is an asset that contains information and methods.
[CreateAssetMenu(menuName = "AIComponents/State")]
public class AIState : ScriptableObject
{
    //Here it contains all the actions and transitios the controller have to execute and look for
    public AIAction[] aiActions;
    public AITransition[] aiTransitions;

    //The controller first execute the actions and then checks the transitions.
    public void UpdateState(AIController controller)
    {
        controller.StartCoroutine(DoActions(controller));
        CheckTransitions(controller);
    }

    //Its a coroutine to keep a distance between action execution. 
    private IEnumerator DoActions(AIController controller)
    {
        if (aiActions.Length > 0)
        {
            for (int i = 0; i < aiActions.Length; i++)
            {
                aiActions[i].DoAction(controller);
                yield return new WaitForSeconds(1 / controller.stateHandler.settings.objectUpdateRate);
            }
        }
        else yield return false;
    }

    //For each decision we check the condition. If the condition is met the controller statehandler will transition
    //to the true state.
    private void CheckTransitions(AIController controller)
    {
        if (aiTransitions.Length > 0)
        {
            for (int i = 0; i < aiTransitions.Length; i++)
            {
                bool decisionState = aiTransitions[i].decision.Decide(controller);
                if (decisionState)
                {
                    controller.stateHandler.TransitionToState(aiTransitions[i].trueState);
                    return;
                }
                else
                {
                    controller.stateHandler.TransitionToState(aiTransitions[i].falseState);
                }
            }
        }
        else return;
    }
}
