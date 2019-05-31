using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
	public Rigidbody rigidBody;
	public Transform moveDireccionTransform;
	public Settings settings;

	public Player(Rigidbody _rigidbody, Transform _moveDireccionTransform)
	{
		rigidBody = _rigidbody;
		moveDireccionTransform = _moveDireccionTransform;
	}

    public Vector3 Position
    {
        get { return rigidBody.position; }
        set { rigidBody.position = value; }
    }
	public Vector3 MoveDireccion
	{
		get {return moveDireccionTransform.forward;}
	}
	public float SoulGainRatio
	{
		get {return settings.soulGainRatio;}
		set {settings.soulGainRatio = value;}
	}

	public float CostRatio
	{
		get {return settings.costRatio;}
		set {settings.costRatio = value;}
	}

	public void RotateTransform(float posX, float posZ)
    {
        float rotY = Mathf.Atan2(posX, posZ) * Mathf.Rad2Deg;
        moveDireccionTransform.rotation = Quaternion.Euler(0f, rotY, 0f);
    }

	[Serializable]
	public class Settings
	{
		public float soulGainRatio;
    	public float costRatio;
	}
}
