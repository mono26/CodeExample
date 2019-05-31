using System.Collections.Generic;
using UnityEngine;

public enum GravitySourceType { Planet, Blackhole }

public class Planet : SpawnableObject
{
    [Header("Settings")]
    [SerializeField][Range(-1,1)] int gravitationalFieldDirection = 1; // +1 left, -1 right
    // TODO create rotation component
    [SerializeField] float rotationSpeed = 4.4f;
    [SerializeField] float rotationForce = 9.8f;
    [SerializeField] protected float gravityForce = 9.8f;
    [SerializeField] float gravFieldRadius = 14;
    [SerializeField] protected GravitySourceType gravitySource;
    [SerializeField] bool playerInGravitationalField = false;

    [Header("Components")]
    [SerializeField] protected SignalEmitter signal;
    [SerializeField] CircleCollider2D gravitationalField = null;

    [SerializeField] AutoDestroy autoDestroyComponent = null;

    [Header("Planet editor debuggin")]
    [SerializeField] protected List<IAffectedByGravity> objsInGravitationField = new List<IAffectedByGravity>();


    public float GetRotationForce { get { return rotationForce; } }
    public float GetGravityForce { get { return gravityForce; } }
    public float GetGravFieldRadius { get { return gravFieldRadius; } }
    public int GetGravFieldDirection { get { return gravitationalFieldDirection; } }
    public GravitySourceType GetGravitySource { get { return gravitySource; } }
    public SignalEmitter GetSignal { get { return signal; } }
    public Vector2 GetCenterPosition { get {return (Vector2)transform.position + GetCollisionBody.offset; } }

    bool alreadyAwaked = false;

    // Use this for initialization
    public override void Awake()
    {
        if (alreadyAwaked)
        {
            return;
        }

        base.Awake();

        CircleCollider2D planetCollider = GetComponent<CircleCollider2D>();
        if (planetCollider) 
        {
            GetComponent<CircleCollider2D>().radius = GetRadius;
        }

        if (gravitationalField == null)
        {
            GetComponentInChildren<CircleCollider2D>();
        }

        if(gravitationalField != null)
        {
            gravFieldRadius = gravitationalField.radius;
        }

        if (signal == null)
        {
            signal = GetComponent<SignalEmitter>();
        }

        if (autoDestroyComponent == null)
        {
            autoDestroyComponent = GetComponent<AutoDestroy>();
        }
    }

    protected virtual void Start()
    {
        gravitySource = GravitySourceType.Planet;

        playerInGravitationalField = false;
	}

    protected virtual void FixedUpdate()
    {
        if(!playerInGravitationalField)
        {
            return;
        }

        transform.Rotate(new Vector3(0, 0, rotationSpeed * gravitationalFieldDirection * Time.fixedDeltaTime));

        RotateObjectsInGravitationField();
        ApplyGravityOnObjects();
    }

    protected void RotateObjectsInGravitationField()
    {
        if(objsInGravitationField.Count == 0)
        {
            return;
        }

        for (int i =  objsInGravitationField.Count - 1; i > -1; i--)
        {
            if(objsInGravitationField[i] == null)
            {
                objsInGravitationField.RemoveAt(i);
                continue;
            }

            objsInGravitationField[i].ApplyRotation(this);
            // Becuse we are using the opposite of the transform.up for the lerp. We must use the opposite of the lerp.
            objsInGravitationField[i].RotateTowardsGravitationCenter(this);
        }
    }

    protected virtual void ApplyGravityOnObjects()
    {
        if(objsInGravitationField.Count == 0)
        {
            return;
        }

        for (int i =  objsInGravitationField.Count - 1; i > -1; i--)
        {
            if(objsInGravitationField[i] == null)
            {
                objsInGravitationField.RemoveAt(i);
                continue;
            }

            objsInGravitationField[i].ApplyGravity(this);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D _collider)
    {
        IAffectedByGravity obj = _collider.GetComponent<IAffectedByGravity>();
        if (obj != null && !objsInGravitationField.Contains(obj))
        {
            objsInGravitationField.Add(obj);
        }

        if(_collider.tag == "Player")
        {
            playerInGravitationalField = true;
        }
    }

    void OnTriggerStay2D(Collider2D _collider)
    {
        if(_collider.tag == "Player")
        {
            playerInGravitationalField = true;
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D _collider)
    {
        IAffectedByGravity obj = _collider.GetComponent<IAffectedByGravity>();
        if (obj != null && objsInGravitationField.Contains(obj) == true)
        {
            objsInGravitationField.Remove(obj);
        }

        if (_collider.tag == "Player")
        {
            playerInGravitationalField = false;
        }
    }

    public override void Release()
    {
        // DestroyWeapons();

        PoolsManager.Instance.ReleaseObjectToPool(this);
    }

    public void SetLifeTimeAccordingToDistanceFromPlayer(float _distanceFromPlayer)
    {
        if (autoDestroyComponent == null)
        {
            return;
        }
        
        int minLifeTime = autoDestroyComponent.GetMinLifeTime;
        int maxLifeTime = autoDestroyComponent.GetMaxLifeTime;
        int lifeTime = Random.Range(minLifeTime, maxLifeTime);
        autoDestroyComponent.SetLifeTime(lifeTime + (_distanceFromPlayer * 2));
    }
}
