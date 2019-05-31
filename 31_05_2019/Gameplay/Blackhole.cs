// Copyright (c) What a Box Creative Studio. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlackholeEventType { CenterReachead, LifeTimeEnd }

public class BlackholeEvent : GameEvent
{
    private GameObject affectedGameObject;
    private BlackholeEventType eventType;

    public GameObject GetAffectedObject { get { return affectedGameObject; } }
    public BlackholeEventType GetEventType { get { return eventType; } }

    public BlackholeEvent(GameObject _affectedGameObject, BlackholeEventType _eventType)
    {
        affectedGameObject = _affectedGameObject;
        eventType = _eventType;
        return;
    }
}

public class Blackhole : Planet
{
    [Header("Settings")]
    [SerializeField] float maxForce = 10;
    [SerializeField] float minForce = 7;
    protected float minimumDistanceToCenter;

    float currentForce;
    float currentSize = 0;

    protected override void OnTriggerEnter2D(Collider2D _collider)
    {
        base.OnTriggerEnter2D(_collider);
    }

    protected override void Start() 
    {
        base.Start();

        gravitySource = GravitySourceType.Blackhole;
    }

    void OnEnable() 
    {
        currentSize = 0;
        transform.localScale = new Vector3 (0, 0, 1);
    }

    void Update()
    {
        if (!currentSize.Equals(1.0f))
        {
            currentSize += Time.deltaTime/2;
            currentSize = Mathf.Clamp(currentSize, 0, 1);
            transform.localScale = new Vector3 (currentSize, currentSize, 1);
        }

        if (!gravityForce.Equals(minForce))
        {
            float forceDifference = maxForce - minForce;
            gravityForce += forceDifference * Time.deltaTime/2;
        }
    }

    protected void CheckObjectsDistanceToCenter()
    {
        for (int i =  objsInGravitationField.Count - 1; i > -1; i--)
        {
            if(objsInGravitationField[i] == null)
            {
                objsInGravitationField.RemoveAt(i);
                continue;
            }

            if(Vector3.Distance(objsInGravitationField[i].GetBodyComponent.position, transform.position) < minimumDistanceToCenter) {
                EventManager.TriggerEvent<BlackholeEvent>(new BlackholeEvent(objsInGravitationField[i].GetBodyComponent.gameObject, BlackholeEventType.CenterReachead));
               
            }
        }
    }
}

