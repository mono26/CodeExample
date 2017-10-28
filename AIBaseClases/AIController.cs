using System;
using UnityEngine;
using UnityEngine.AI;

//This is the base Controller class. The enemy, plant and bullet implement this class.
//this class contains the basic stuff for a controller to work. Also everything inside this class has to be implemented.
public abstract class AIController : MonoBehaviour
{
    [HideInInspector]
    public Info objectInfo;
    [HideInInspector]
    public AICoolDownHandler cooldownHandler;
    [HideInInspector]
    public AIStateHandler stateHandler;

    //Metodos vacios y unos obligatorio para cad uno de los controllers
    public abstract void Awake();
    public abstract void Start();
    public abstract void Update();

    public virtual void SetBaseVariables(Info _info, AICoolDownHandler _cooldownHandler, AIStateHandler _stateHandler)
    {
        objectInfo = _info;
        cooldownHandler = _cooldownHandler;
        stateHandler = _stateHandler;
    }

    public abstract void Release();
}
