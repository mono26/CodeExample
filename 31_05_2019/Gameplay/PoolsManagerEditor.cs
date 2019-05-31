using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PoolsManager))]
public class PoolsManagerEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		EditorGUILayout.Separator();

		PoolsManager poolsManager = target as PoolsManager;

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Pools", GUILayout.Width(EditorGUIUtility.currentViewWidth));
		EditorGUILayout.EndHorizontal();

		if (poolsManager.pools == null)
		{
			return;
		}

		var keys = poolsManager.pools.Keys;
		foreach (Type key in keys)
		{
			GameObjectPool objectPool;
			if (poolsManager.pools.TryGetValue(key, out objectPool))
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(key.Name, GUILayout.Width(EditorGUIUtility.currentViewWidth));
				EditorGUILayout.EndHorizontal();

				foreach (Spawnable spawnable in objectPool.pool)
				{
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.ObjectField(spawnable, typeof(Spawnable), false, GUILayout.Width(EditorGUIUtility.currentViewWidth));
					EditorGUILayout.EndHorizontal();
				}
			}
		}
	}
}
