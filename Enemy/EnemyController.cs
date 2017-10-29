using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent (typeof(NavMeshAgent))]
public class EnemyController : AIController
{
    //Info class, just contains name and index. INdex is used for the objectPool to agrupate in different lists
    // object of the same type
    public Info enemyInfo;
    public Enemy enemy;
    public Settings settings;

    public AICoolDownHandler enemyCooldownHandler;
    public AIStateHandler enemyStateHandler;
    public AIHealthHandler enemyhealthHandler;
    public AIAnimationHandler animationHandler;
    public AIEffectsHandler effectsHandler;
    public AIDropHandler dropHandler;

    //Metodos de unity
    private void OnEnable()
    {
        Start();
    }

    //Here in the awake method we create all the dependencies. Also with a global Settings container for all the enemy
    //settings of each component we can controll each instance of this class, giving each one unique values.
    public override void Awake()
    {
        enemy = new Enemy(settings.NavMeshAgent, settings.EnemySettings);
        enemyCooldownHandler = new AICoolDownHandler();
        effectsHandler = new AIEffectsHandler(settings.EffectsHandlerSettings);
        dropHandler = new AIDropHandler(settings.DropHandlerSettings);
        enemyStateHandler = new AIStateHandler(this, settings.OriginalState, settings.RemainState, settings.StateHandlerSettings);
        enemyhealthHandler = new AIHealthHandler(this, effectsHandler, settings.HealthHandlerSettings);
        animationHandler = new AIAnimationHandler(this, settings.Sprite, settings.Animator);

        SetBaseVariables(enemyInfo, enemyCooldownHandler, enemyStateHandler);
    }

    public override void Start()
    {
        enemyStateHandler.Start();
        enemy.StartNavMeshForFirstTime();
        if (enemy.MeleeCollider != null)
        {
            enemy.MeleeCollider.gameObject.SetActive(false);
        }
    }

    public override void Update()
    {
        enemyCooldownHandler.Update();
    }

    public override void SetBaseVariables(Info _info, AICoolDownHandler _cooldownHandler, AIStateHandler _stateHandler)
    {
        base.SetBaseVariables(_info, _cooldownHandler, _stateHandler);
    }

    public override void Release()
    {
        effectsHandler.SetEffects();
        dropHandler.DropToTheField();
        cooldownHandler.ResetTimers();
        WaveSpawner.Instance.gameNumberOfEnemies--;
        PoolsManagerEnemies.Instance.ReleaseEnemy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("PlantMeleeCollider"))
        {
            var plant = other.GetComponent<PlantController>();
            enemy.TakeDamage(plant.plant.settings.PlantDamage);
            enemyhealthHandler.TakeDamage(plant.plant.settings.PlantDamage);
        }
        if(other.CompareTag("Bullet"))
        {
            var bullet = other.GetComponent<BulletController>();
            enemy.TakeDamage(bullet.bullet.settings.BulletDamage);
            enemyhealthHandler.TakeDamage(bullet.bullet.settings.BulletDamage);
        }
    }

    [Serializable]
    public class Settings
    {
        public NavMeshAgent NavMeshAgent;
        public Animator Animator;
        public Transform Sprite;

        public AIState OriginalState;
        public AIState RemainState;

        public Enemy.Settings EnemySettings;
        public AIStateHandler.Settings StateHandlerSettings;
        public AIDropHandler.Settings DropHandlerSettings;
        public AIEffectsHandler.Settings EffectsHandlerSettings;
        public AIHealthHandler.Settings HealthHandlerSettings;
    }
}
