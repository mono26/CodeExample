using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "AIComponents/Decision/Enemy/LookHouse")]
public class DecisionEnemyLookHouse : AIDecision
{
    public override bool Decide(AIController controller)
    {
        bool targetVisible = Look(controller as EnemyController);
        return targetVisible;
    }
    private bool Look(EnemyController controller)
    {
        //Mirando la transicion.
        if (controller.enemy.EnemyTarget != null)
        {
            var distancia = Vector3.Distance(controller.enemy.EnemyTarget.position, controller.transform.position);
            if (distancia <= controller.enemy.settings.EnemyRange)
            {
                return true;
            }
            else return false;
        }
        else return false;
    }
}
