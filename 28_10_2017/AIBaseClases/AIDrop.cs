using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base class for any drop or loot object. Has the basic information for pooling and reward
//Also basic methods that have to be implemented.
public abstract class AIDrop : MonoBehaviour 
{
	[HideInInspector]
	public int index;
	[HideInInspector]
	public int reward;

    public virtual void SetBaseVariables(int _index)
    {
        index = _index;
    }

    public virtual void SetReward(int _reward)
    {
        reward = _reward;
    }
}
