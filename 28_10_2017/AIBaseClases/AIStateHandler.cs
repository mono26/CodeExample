using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class for state management. Needs a controller to execute coroutines and actions.
public class AIStateHandler
{
    public AIController objectController;
    public Settings settings;

    public AIState objectCurrentState;
    public AIState objectRemainState;
    public AIState objectOriginalState;

    public AIStateHandler(AIController _controller, AIState _originalState, AIState _remainState, Settings _settings)
    {
        objectController = _controller;
        objectOriginalState = _originalState;
        objectRemainState = _remainState;
        settings = _settings;
    }

    public void Start()
    {
        SetStartingState(objectOriginalState);
        objectController.StartCoroutine(UpdateState());
    }

    public IEnumerator UpdateState()
    {
        objectCurrentState.UpdateState(objectController);
        yield return new WaitForSeconds(1 / settings.objectUpdateRate);
        objectController.StartCoroutine(UpdateState());
    }

    public virtual void TransitionToState(AIState _nextState)
    {
        if (_nextState != objectRemainState)
        {
            objectController.StopAllCoroutines();
            objectCurrentState = _nextState;
            objectController.StartCoroutine(UpdateState());
        }
    }

    public virtual void SetStartingState(AIState _state)
    {
        objectCurrentState = _state;
    }

    [Serializable]
    public class Settings
    {
        public float objectUpdateRate = 2f;
    }
}
