using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerShoot 
{
	public Player spooky;
	//Settings
    public Settings settings;

    //Here we store the cooldown, private because we only modify this value here
	//even though there is no reference of it is good practice to make private variables.
    [SerializeField]
    private float shootCooldownTimer;
	//Information of the actual bullet in scene. The bullet can be charged to increase damage
    //also it has a charge time limit
    public float chargeTime;
    public bool isCharging;
    private GameObject actualBullet;

    public PlayerShoot(Player _spooky, Settings _settings)
    {
        spooky = _spooky;
        settings = _settings;
    }

	public void Start()
	{
		//settings.playerBullet.Initialize();
	}

	public void Update()
    {
        if (shootCooldownTimer >= 0)
            shootCooldownTimer -= Time.deltaTime;
        if(isCharging == true && chargeTime < 3){
            chargeTime += 1 * Time.deltaTime ;
            actualBullet .GetComponent <Transform>().localScale +=  new Vector3 (0.002f,0.002f,0.002f) * chargeTime;
        } 
        
        chargeTime = Mathf .Clamp(chargeTime , 0, 3);
            
        //TESTING INPUT
        if(Input.GetKeyDown (KeyCode.Q))
        ChargeBullet ();
        if(Input .GetKeyUp(KeyCode.Q))
        FireWithMovementDirection (chargeTime);
    }

	public void ChargeBullet()
	{

         if (shootCooldownTimer > 0.0f)
        {
            return;
        }

        isCharging = true;
        actualBullet = PoolsManagerBullets.Instance.GetBullet(settings.playerBullet.info.objectIndex);
        
       actualBullet.transform.position = spooky.moveDireccionTransform.position;
       // actualBullet.transform.rotation = Quaternion .Euler(0,180,0);
        
        actualBullet .GetComponent <Transform>().localScale = new Vector3 (0.7f,0.7f,0.7f);
        actualBullet .GetComponent <Rigidbody>().isKinematic = true;
        actualBullet .transform.parent = spooky.moveDireccionTransform .transform;
    }

    public void FireWithMovementDirection(float cTime)
    {
        cTime = chargeTime ;
       // bullet = actualBullet ;
        if (shootCooldownTimer > 0.0f)
        {
            return;
        }
        Debug.Log("Shooting");

        shootCooldownTimer = settings.attackSpeed;       

        //Charge time affects damage
        var damage = 0; 
        if(cTime < 1 ){
            damage = 1;
        } else if (cTime < 2){
            damage = 2;
        } else if(cTime <= 3){
            damage = 3;
        }
        
        actualBullet.GetComponent <BulletController >().settings.BulletDamage = damage ;
        Debug .Log(damage );

        //Adds force
        actualBullet .GetComponent <Rigidbody>().isKinematic = false;
        actualBullet .transform.parent = null;
        actualBullet.transform.rotation = spooky.moveDireccionTransform.transform.rotation;        
        actualBullet.GetComponent<Rigidbody>().AddForce(spooky.MoveDireccion * settings.bulletSpeed, ForceMode.Impulse);
        isCharging = false;
        chargeTime = 0;   
    }

    [Serializable]
    public class Settings
    {
        public float attackSpeed = 15f;
        public float playerDamage = 1.0f;
        public float bulletSpeed = 6.5f;
        public BulletController playerBullet;
    }
}
