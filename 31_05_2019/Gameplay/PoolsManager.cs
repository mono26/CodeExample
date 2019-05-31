using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolsManager : Singleton<PoolsManager>
{
	[SerializeField] PoolSettings[] poolsSettings;
	public Dictionary<Type, GameObjectPool> pools;

	protected override void Awake()
	{
		base.Awake();

		InitializePools();
	}

	void InitializePools()
	{
		pools = new Dictionary<Type, GameObjectPool>();

		foreach (PoolSettings settings in poolsSettings)
		{
			GameObjectPool poolToAdd = new GameObjectPool(settings);
			if (!pools.ContainsKey(settings.prefabs[0].GetType()))
			{
				pools.Add(settings.prefabs[0].GetType(), poolToAdd);
			}
		}
	}

	public T GetObjectFromPool<T>() where T : Spawnable
	{
		GameObjectPool pool = null;
		Spawnable goToReturn = null;
		if (pools.TryGetValue(typeof(T), out pool))
		{
			goToReturn = pool.GetGameObject();
		}

		return goToReturn as T;
	}

	public void ReleaseObjectToPool<T>(T _goToRelease) where T : Spawnable
	{
		GameObjectPool pool = null;
		if (pools.TryGetValue(typeof(T), out pool))
		{
			pool.ReleaseGameObject(_goToRelease);
		}
	}
}
