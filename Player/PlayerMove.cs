using CnControls;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerMove
{
    public Player spooky;
    public Settings settings;
    public PlayerEnemyAutoDetect spookyAutoDetect;

    //Para el animator
    public bool isMoving;
    public Vector3 lastPosition;

    //Componenetes de unity
    public Animator playerAnimator;
    public Transform playerSprite;

    //Para almacenar los valores del input
    private float horizontalAxis;
    private float verticalAxis;

    public PlayerMove(Player _spooky, Settings _settings, PlayerEnemyAutoDetect _spookyAutoDetect)
    {
        spooky = _spooky;
        settings = _settings;
        spookyAutoDetect = _spookyAutoDetect;
    }

    public void Start()
    {
        settings.slowMoSpeed = settings.normalMoSpeed /2;
        settings.movementSpeed = settings.normalMoSpeed ;
    }

    public void FixedUpdate()
    {
        horizontalAxis = CnInputManager.GetAxis("Movement Horizontal");
        verticalAxis = CnInputManager.GetAxis("Movement Vertical");

        if(spookyAutoDetect.targetLine.Count > 0 && !spookyAutoDetect.targetLine[0])
            spooky.RotateTransform(horizontalAxis, verticalAxis);

        if (horizontalAxis != 0 || verticalAxis != 0)
        {
            MovePosition(new Vector3(horizontalAxis, 0, verticalAxis));
        }
        else isMoving = false;
        ClampPosition();
        //Animate();
        lastPosition = spooky.Position;
    }
    private void Animate()
    {
        
        if (!isMoving)
        {
            playerAnimator.SetBool("Walking", false);
            return;
        } else
            
        if (spooky.Position.x <= lastPosition.x)
        {
            playerSprite.localScale = new Vector3(-1, 1, 1);
        }
        else if (spooky.Position.x > lastPosition.x)
        {
            playerSprite.localScale = new Vector3(1, 1, 1);
        }
    }
    private void MovePosition(Vector3 direction)
    {
        isMoving = true;
        //playerAnimator.SetBool("Walking", true);
        spooky.Position += direction * settings.movementSpeed * Time.fixedDeltaTime;
    }
    //Method for clamping character inside moving plane
    private void ClampPosition()
    {
        spooky.Position = new Vector3
            (
                Mathf.Clamp(spooky.Position.x, -settings.maxMovementHorizontal, settings.maxMovementHorizontal),
                spooky.Position.y,
                Mathf.Clamp(spooky.Position.z, -settings.maxMovementVertical, settings.maxMovementVertical)
            );
    }

    //Para controlar la velocidad de spooky cuando entra en los campos de maiz.
    private void ChangeSpeed(float speed){
        settings.movementSpeed = speed;
    }
    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag ("CornField")){
            ChangeSpeed(settings.slowMoSpeed);
        }
        if(other.CompareTag("EnemyMeleeCollider"))
        {
            //FightCloud.Instance.SetFight(this.gameObject, other.GetComponentInParent<AIEnemyController>().gameObject);
        }
    }
    public void OnTriggerExit(Collider other){
        if(other.CompareTag ("CornField")){
            Debug.Log("Saliendo del cornfield");
            ChangeSpeed(settings.normalMoSpeed);
        }
    }

    [Serializable]
    public class Settings
    {
            //Variables del jugador
        public float movementSpeed;
        public float normalMoSpeed;
        public float slowMoSpeed;

            //Para clampear el valor maximo que se puede mover
        public float maxMovementHorizontal;
        public float maxMovementVertical;
    }
}
