using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class is in charge of the collision system and detecting enemies inside
//the detection range
[Serializable]
public class PlayerEnemyAutoDetect
{
    //This is the reference to the player so the script can access properties like position, rigidbody, etc
	public Player spooky;
	public Settings settings;
	public SphereCollider sphereTrigger;

	//For the enemyAutoDetect we create a list where we introduce enemies as they enter in collision with the sphereTrigger
    //and each time a enemy exits the trigger it takes it out of the list.
    [SerializeField]
    public List<GameObject> targetLine;

	public PlayerEnemyAutoDetect(Player _spooky, Settings _settings, SphereCollider _sphereTrigger)
	{
		spooky = _spooky;
		settings = _settings;
		sphereTrigger = _sphereTrigger;
	}

    public void Start()
    {
        targetLine = new List<GameObject>();
        sphereTrigger.radius = settings.EnemyAutoDetectionRange;
    }

	public void Update()
	{
		if(targetLine.Count > 0)
        {
            var direccion = (targetLine[0].transform.position - spooky.Position).normalized;
            RotateTransform(direccion.x, direccion.z);
            if (Vector3.SqrMagnitude(spooky.Position - targetLine[0].transform.position) >
                settings.EnemyAutoDetectionRange * settings.EnemyAutoDetectionRange)
            {
                ClearTarget();
            }
        }
	}
    private void ClearTarget()
    {
        if (targetLine.Count > 0)
        {
            targetLine.RemoveAt(0);
        }
        else return;
    }

	private void RotateTransform(float posX, float posZ)
    {
        float rotY = Mathf.Atan2(posX, posZ) * Mathf.Rad2Deg;
        spooky.moveDireccionTransform.rotation = Quaternion.Euler(0f, rotY, 0f);
    }

    //This are the built-in methos for detecting trigger collision.
	//Here is called each time an object enters in collision with the sphereCollider
	public void OnTriggerEnter(Collider collider)
	{
		//If the object that entered collision is tagged as Enemy. Here for telling the
		//script to not check any collision that is not a plantPoint
		if(collider.CompareTag("Enemy"))
		{
            Debug.Log("Detecto un enemigo");
			targetLine.Add(collider.gameObject);
		}
		else return;
	}

	//Is called each time a colliders exits the bounds of the sphereCollider
	//Each time a plantPoint exits the bouds it checks if its equal to the current one
	public void OnTriggerExit(Collider collider)
	{
        if(targetLine.Count > 0)
        {
            //If the tag matches Enemy it continues to the next step, else it cancels execution
            if (collider.CompareTag("Enemy"))
            {
                targetLine.Remove(collider.gameObject);
            }
        }
		else return;
	}

	[Serializable]
	public class Settings
	{
    	public float EnemyAutoDetectionRange;
	}
}
