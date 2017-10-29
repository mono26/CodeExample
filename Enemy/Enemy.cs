using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy
{
    //Like player class all the needed components or the Enemy to work. They are not going to be changed during run time
    //Each one should have a getter and a method to Set or Change to a new value.
    public Settings settings;

    readonly NavMeshAgent navMeshAgent;

    public Transform enemyTarget;

    public float stolenValue;
    public bool finishedStealing;

    public SoundPlayer soundPlayer;

    public int health;

    public Enemy(NavMeshAgent _navMeshAgent, Settings _settings)
    {
        navMeshAgent = _navMeshAgent;
        settings = _settings;
    }

    //Properties of the class. Each one with its own getter, or setter if needed.
    public Vector3 Position
    {
        get { return navMeshAgent.transform.position; }
        set { navMeshAgent.transform.position = value; }
    }

    public NavMeshAgent NavMeshAgent
    {
        get { return navMeshAgent; }
    }

    public Transform EnemyTarget
    {
        get { return enemyTarget; }
    }

    public Collider MeleeCollider
    {
        get { return settings.MeleeCollider; }
    }

    public bool FinishStealing
    {
        get { return finishedStealing; }
    }

    public int Health
    {
        get { return health; }
    }

    //Here are the methods to change current values of this class. Each Enemy should have its own values
    //So no over writing will be espected
    public void TakeDamage(int _damage)
    {
        health -= _damage;
    }

    public void ChangeTarget(Transform _newTarget)
    {
        enemyTarget = _newTarget;
    }

    public void StartNavMeshForFirstTime()
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.updateRotation = false;
    }

    public void StopNavMesh()
    {
        navMeshAgent.isStopped = true;
    }

    public void FinishSteal(bool _stole)
    {
        finishedStealing = _stole;
    }

    [Serializable]
    public class Settings
    {
        public Collider MeleeCollider;

        public float EnemyMovementSpeed;
        public int EnemyDamage;
        public float EnemyAtackSpeed;

        public float EnemyRange;
        public float EnemyMeleeRange;
        public float objectForce;

        public AIAbility EnemyMeleeAbility;
        public AIAbility EnemyBasicAbility;
        public AIAbility EnemySpecialAbility;
    }
}
