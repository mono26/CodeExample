using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLoot : AIDrop
{
    public int lootReward;
    public int lootIndex;

    [SerializeField]
    private float dropDeadTime = 5.0f;

    private void Awake()
    {
        SetBaseVariables(lootIndex);
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
        lootReward = _reward;
        base.SetReward(_reward);
    }
    public void IncreaseLoot(int _increase)
    {
        lootReward += _increase;
    }
    public void PlaceLoot(Transform place)
    {
        transform.position = place.position;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Spooky"))
        {
            GameManager.Instance.GiveMoney(this.lootReward);
            PoolsManagerDrop.Instance.ReleaseDrop(this.gameObject);
        }
    }
}
