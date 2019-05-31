using System.Collections.Generic;
using UnityEngine;

public class PoolsManagerEnemies : MonoBehaviour {
    //Singleton part
    private static PoolsManagerEnemies instance;
    public static PoolsManagerEnemies Instance
    {
        get { return instance; }
    }

    //List for all the enemyPrfabs to save in the pool
    public List<GameObject> enemyPrefabs;
    //Here is a list of lists containing all the enemies, each list has only one enemy and a certain amount at start.
    //Use index to acces the correst list of enemies.
    public List<List<GameObject>> pools = new List<List<GameObject>>();

    //First time size initialization for all the lists of enemies.
    public List<int> listOfSizes;

    [SerializeField]
    Transform poolsPosition;

    private void Awake()
    {
        //This is for making this the only instance in the game. Singleton part.
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);

        //Here we read the amount of enemy prefabs are assigned on the ínspector. Then por each one of them
        //we tell the listOfSizes the size of it and set the value of the amount of enemies we want to sapwn at start.
        for (int enemyPrefab = 0; enemyPrefab < enemyPrefabs.Count; enemyPrefab++)
        {
            pools.Add(new List<GameObject>());
            listOfSizes.Add(5);
        }
    }
    // Use this for initialization
    void Start ()
    {
        //Because the listOfSizes has the same lenght as the EnemyPrefabsList for each index of the listOfSizes we add to the current list
        //the number of enemies equal to the value store in the index.
		for (int index = 0; index < listOfSizes.Count; index++)
        {
            var size = listOfSizes[index];
            //Here we save the size of the list; the value inside the the index in the listOfSizes
            //we the same value of enemies inside the list.
            for (int i = 0; i < size; i++)
            {
                AddEnemy(index);
            }
        }
	}

    //Here we add to the list the enemy depending on its index
    private void AddEnemy(int index)
    {
        GameObject obj = Instantiate(enemyPrefabs[index], poolsPosition.position, poolsPosition.rotation);
        //Here we force and make sure the index is the same as the list in the list of lists.
        obj.GetComponent<EnemyController>().enemyInfo.objectIndex = index;
        obj.gameObject.SetActive(false);
        pools[index].Add(obj);
    }
    //To take a enemy out of the pool we need the correct index to access the correct list.
    public GameObject GetEnemy(int index, Transform target)
    {
        //If there is no Enemy we add one
        if (pools[index].Count == 0)
            AddEnemy(index);
        //List according to index
        var list = pools[index];
        GameObject enemy = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);
        SetEnemyPosition(enemy, target);
        enemy.gameObject.SetActive(true);
        return enemy;
    }
    //To release and put back inside the list, we use the same index for that
    public void ReleaseEnemy(GameObject obj)
    {
        obj.gameObject.SetActive(false);
        pools[obj.GetComponent<EnemyController>().enemyInfo.objectIndex].Add(obj);
    }
    public void SetEnemyPosition(GameObject obj, Transform target)
    {
        obj.transform.position = target.position;
        obj.transform.rotation = target.rotation;
    }
}
