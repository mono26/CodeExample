using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDrop : AIDrop
{
    public int dropReward;
    public int dropIndex;
    [SerializeField]
    private float dropDeadTime = 5.0f;

    private void Awake()
    {
        SetBaseVariables(dropIndex);
    }
    private void OnEnable()
    {
        dropDeadTime = 5.0f;
    }
    private void Update()
    {
        dropDeadTime -= Time.deltaTime;
        if(dropDeadTime <= 0)
        {
            PoolsManagerDrop.Instance.ReleaseDrop(this.gameObject);
        }
    }
    public override void SetBaseVariables(int _index)
    {
        base.SetBaseVariables(_index);
    }
    public override void SetReward(int _reward)
    {
        dropReward = _reward;
        base.SetReward(_reward);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Spooky"))
        {
            GameManager.Instance.GiveMoney(this.dropReward);
            PoolsManagerDrop.Instance.ReleaseDrop(this.gameObject);
        }
    }
}
