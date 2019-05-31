using UnityEngine;

[CreateAssetMenu (menuName = "AIComponents/Actions/Enemy/FollowHouse")]
public class ActionEnemyFollowHouse : AIAction
{ 
    public override void DoAction(AIController controller)
    {
        FollowHouse(controller as EnemyController);
    }

    private void FollowHouse(EnemyController controller)
    {
        //Ejecutando movimiento a la casa!
        if (!controller.enemy.EnemyTarget)
        {
            controller.enemy.ChangeTarget(GameManager.Instance.houseStealPoints[Random.Range(0, 5)].transform);
            return;
        }
        if (!controller.enemy.EnemyTarget.CompareTag("StealPoint") && !controller.enemy.EnemyTarget.CompareTag("Plant"))
        {
            controller.enemy.ChangeTarget(GameManager.Instance.houseStealPoints[Random.Range(0, 5)].transform);
            return;
        }
        else
        {
            controller.enemy.NavMeshAgent.isStopped = false;
            controller.enemy.NavMeshAgent.SetDestination(controller.enemy.EnemyTarget.position);
        }
    }
}
