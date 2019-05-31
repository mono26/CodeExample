using System.Collections;
using UnityEngine;

public class SpookyEnemyDetect : EnemyDetect
{
    [SerializeField]
    private Character currentEnemyTarget;
    public Character CurrentEnemyTarget { get { return currentEnemyTarget; } }

    private Coroutine targetClosestEnemy = null;

    protected override void Detect()
    {
        base.Detect();

        if (nearEnemies.Count == 0)
            currentEnemyTarget = null;
        return;
    }

    private IEnumerator TargetNearestEnemy()
    {
        if (nearEnemies.Count > 0)
        {
            float distanceToTheFirstEnemy = Vector3.SqrMagnitude(character.transform.position - nearEnemies[0].transform.position);
            Character temporalCurrentEnemy = nearEnemies[0];
            for (int plantPoint = 0; plantPoint < nearEnemies.Count; plantPoint++)
            {
                float distanceToTheNextEnemy = Vector3.SqrMagnitude(character.transform.position - nearEnemies[plantPoint].transform.position);
                if (distanceToTheNextEnemy < distanceToTheFirstEnemy)
                {
                    distanceToTheFirstEnemy = distanceToTheNextEnemy;
                    temporalCurrentEnemy = nearEnemies[plantPoint];
                }
            }
            if (!currentEnemyTarget.Equals(temporalCurrentEnemy))
            {
                ChangeCurrentEnemyTarget(temporalCurrentEnemy);
            }
        }
        yield return new WaitForSeconds(Time.deltaTime);
        targetClosestEnemy = StartCoroutine(TargetNearestEnemy());
    }

    private void ChangeCurrentEnemyTarget(Character _enemy)
    {
        currentEnemyTarget = _enemy;
        return;
    }

    public Vector3 GetCurrentEnemyTargetDirection()
    {
        Vector3 _direction = Vector3.zero;
        if (currentEnemyTarget)
        {
            _direction = (currentEnemyTarget.transform.position - character.transform.position).normalized;
            _direction.y = 0;
            return _direction;
        }
        else return _direction;
    }

    public override void OnTriggerEnter(Collider _enemyCollider)
    {
        // Check if the collider is tagged as enemy
        if (_enemyCollider.CompareTag("Enemy"))
        {
            if (nearEnemies.Count == 0)
            {
                ChangeCurrentEnemyTarget(_enemyCollider.GetComponent<Character>());
                targetClosestEnemy = StartCoroutine(TargetNearestEnemy()); 
            }
            base.OnTriggerEnter(_enemyCollider);
            return;
        }
        else return;
    }

    public override void OnTriggerExit(Collider _enemyCollider)
    {
        base.OnTriggerExit(_enemyCollider);
        // Check if the collider is tagged as enemy
        if (_enemyCollider.CompareTag("Enemy"))
        {
            if(nearEnemies.Count.Equals(0))
            {
                currentEnemyTarget = null;
                StopCoroutine(targetClosestEnemy);
            }
            return;
        }
        else return;
    }

}
