using System.Collections.Generic;
using UnityEngine;

public class AlienPlanet : Planet
{
    [Header("Alien Planet settings")]
    [SerializeField] int numberOfAliens = 5;

    [SerializeField] List<Alien> aliens;
    [SerializeField] Transform aliensContainer;

    void OnEnable() 
    {
        signal.TurnSignal(SignalEmitter.SignalState.ON);
    }

    void Update()
    {
        if (aliens.Count == 0)
        {
            signal.TurnSignal(SignalEmitter.SignalState.OFF);
            return;
        }
    }

    public void SpawnAliens()
    {
        SpawnAliensInPlanet();
        LocateAliens();
        AlignAliensTowarsCenter();
    }

    void SpawnAliensInPlanet()
    {
        aliens = new List<Alien>();

        for (int i = 0; i < numberOfAliens; i++)
        {
            Alien alien = PoolsManager.Instance.GetObjectFromPool<Alien>();
            alien.transform.SetParent(aliensContainer);
            aliens.Add(alien);
        }
    }

    protected void LocateAliens()
    {
        foreach (Alien alien in aliens)
        {
            int angle = Random.Range(0, 360);
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * (GetRadius + 0.3f);
            float y = Mathf.Sin(Mathf.Deg2Rad * angle) * (GetRadius + 0.3f);
            alien.transform.position = GetCenterPosition + new Vector2(x, y);
        }
    }

    private void AlignAliensTowarsCenter()
    {
        foreach (Alien alien in aliens)
        {
            alien.transform.up = ((Vector2)alien.transform.position - GetCenterPosition).normalized;
        }
    }

    void StopAliensMovement()
    {
        for(int i = aliens.Count - 1; i > -1; i--)
        {
            // TODO remover lista y usar array
            if(!aliens[i].gameObject.activeInHierarchy)
            {
                aliens.RemoveAt(i);
                continue;
            }

            aliens[i].GetBodyComponent.velocity = Vector3.zero;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D _collider)
    {
        base.OnTriggerEnter2D(_collider);

        if(_collider.CompareTag("Alien"))
        {
            IAffectedByGravity obj = _collider.GetComponent<IAffectedByGravity>();
            if (obj != null && !objsInGravitationField.Contains(obj))
            {
                objsInGravitationField.Add(obj);        
            }
        }
    }

    protected override void OnTriggerExit2D(Collider2D _collider)
    {
        base.OnTriggerExit2D(_collider);

        if(_collider.CompareTag("Alien"))
        {
            IAffectedByGravity obj = _collider.GetComponent<IAffectedByGravity>();
            if (obj != null && objsInGravitationField.Contains(obj) == true)
            {
                objsInGravitationField.Remove(obj);
            }
        }

        if(_collider.CompareTag("Player"))
        {
            StopAliensMovement();
        }
    }

    public override void Release()
    {
        foreach (Alien alien in aliens)
        {
            if (alien == null)
            {
                continue;
            }

            alien.transform.SetParent(null);
        }

        aliens.Clear();
        PoolsManager.Instance.ReleaseObjectToPool(this);
    }
}
