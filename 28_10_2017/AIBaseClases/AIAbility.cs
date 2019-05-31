using UnityEngine;

[System.Serializable]
public abstract class AIAbility : ScriptableObject
{
    //This is the basic info of the ability, the name if needed, the spriteEffect, and the specific cooldown
    //the spriteEffect is used for the VFX and placeholder for prefabs.
    [HideInInspector]
    public string abilityName;
    [HideInInspector]
    public GameObject abilitySpriteEffect;

    //Not all the controllers use this cooldown. Plants use their atack speed and special speed atributes.
    //this variablos is mostly for enemies.
    [HideInInspector]
    public float abilityCooldown;

    public abstract void Start();

    //Initialize the cooldown and the virtual method for the each ability that implements this class. Can be casted to any other controller
    //for specific implementations of the class
    public abstract void Ability(AIController controller);
    public virtual void InitializeAbility(float _cooldown, GameObject _spriteEffect, string _name)
    {
        abilityCooldown = _cooldown;
        abilitySpriteEffect = _spriteEffect;
        abilityName = _name;
    }
}
