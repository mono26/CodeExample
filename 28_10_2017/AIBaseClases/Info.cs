using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "AIComponents/Info")]
public class Info : ScriptableObject
{
    //Variable for each object. Name and index for pooling purposes
    public string objectName;
    public int objectIndex;

    public AudioClip attackClip;
}
