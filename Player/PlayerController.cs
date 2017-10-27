using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[Serializable]
public class PlayerController : MonoBehaviour 
{
	public Player spooky;
	public PlayerPlantPointDetect spookyPlantPointDetection;
	public PlayerEnemyAutoDetect spookyEnemyDetection;
	public PlayerMove spookyMove;
	public PlayerShoot spookyShoot;

	//Global settings for each one of the settings
	public GlobalSettings globalSettings;

	public Settings settings;

	private void Awake()
	{
		spooky = new Player(settings.RigidBody, settings.MoveDirectionTransform);

        spookyPlantPointDetection = new PlayerPlantPointDetect(
            spooky,
			settings.PlantPointSphereTrigger, 
			globalSettings.playerSettings.SpookyPlantPointDetectSettings);

		spookyEnemyDetection = new PlayerEnemyAutoDetect(
			spooky, 
			globalSettings.playerSettings.SpookyEnemyAutoDetectSettings, 
			settings.EnemyDetectSphereTrigger);

		spookyMove = new PlayerMove(
			spooky, 
			globalSettings.playerSettings.SpookyMoveSettings, 
			spookyEnemyDetection);

		spookyShoot = new PlayerShoot(
			spooky,
			globalSettings.playerSettings.SpookyShootSettings);
	}

	private void Start()
	{
		spookyEnemyDetection.Start();
		spookyMove.Start();
		spookyPlantPointDetection.Start();
		spookyShoot.Start();
	}
	private void Update()
	{
		spookyEnemyDetection.Update();
		spookyShoot.Update();
        spookyPlantPointDetection.Update();
	}

	private void FixedUpdate()
	{
		spookyMove.FixedUpdate();
	}

	public void FireButtonPress()
	{
		spookyShoot.ChargeBullet();
	}
	public void FireButtonRelease()
	{
		spookyShoot.FireWithMovementDirection(spookyShoot.chargeTime);
	}

	private void OnTriggerEnter(Collider collider)
	{
        Debug.Log("Detectando collisiones");
		spookyEnemyDetection.OnTriggerEnter(collider);
		spookyPlantPointDetection.OnTriggerEnter(collider);
        spookyMove.OnTriggerEnter(collider);
	}

    private void OnTriggerExit(Collider collider)
    {
        Debug.Log("Las collisiones salen de los boundaries");
        spookyEnemyDetection.OnTriggerExit(collider);
        spookyPlantPointDetection.OnTriggerExit(collider);
        spookyMove.OnTriggerExit(collider);
    }

	[Serializable]
	public class Settings
	{
		public Rigidbody RigidBody;
		public SphereCollider PlantPointSphereTrigger;
		public SphereCollider EnemyDetectSphereTrigger;
		public Transform MoveDirectionTransform;
	}
}
