using UnityEngine;

[CreateAssetMenu(menuName = "AIComponents/Ability/Enemy/Steal")]
public class AbilityEnemySteal : AIAbility
{
    public float stealCooldown;
    public string stealName;
    public GameObject stealEffect;

    public EnemyLoot stealLoot;

    public override void Start()
    {
        InitializeAbility(stealCooldown, stealEffect, stealName);
    }

    public override void Ability(AIController controller)
    {
        Steal(controller as EnemyController);
    }
    private void Steal(EnemyController controller)
    {
        //GameManager.Instance.indicatorManager.addObject(controller.gameObject);
        Indicator3D.Instance.addObject(controller.gameObject);
        //Aqui se debe de sacar el loot del pool y ponerlo en el lootPosition
        //var loot = PoolsManagerDrop.Instance.GetObject(stealLoot.lootIndex, controller.enemy.lootPosition).GetComponent<EnemyLoot>();
        //controller.GetComponent<EnemyController>().lootObject = loot.gameObject;
        //Para que quede en la posicion pero cuando muera no desaparezca el loot
        //loot.transform.SetParent(controller.enemy.lootPosition);
        //loot.SetLoot(Random.Range(0, 50));
        GameManager.Instance.LoseHealth(Random.Range(0, 50));
        controller.enemy.NavMeshAgent.isStopped = false;
        controller.enemy.finishedStealing = true;
    }

    public override void InitializeAbility(float _cooldown, GameObject _spriteEffect, string _name)
    {
        base.InitializeAbility(_cooldown, _spriteEffect, _name);
    }
}
