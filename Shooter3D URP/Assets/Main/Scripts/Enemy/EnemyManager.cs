using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemies Pools")]
    public List<GameObject> EnemyBlobPool;
    public List<GameObject> EnemyOrcPool;
    public List<GameObject> EnemyLeechPool;
    public List<GameObject> EnemiesList;
    public List<GameObject> EnemiesInCombatArea;
    [SerializeField] private GameObject _enemyBlob;
    [SerializeField] private GameObject _enemyOrc;
    [SerializeField] private GameObject _enemyLeech;
    private List<List<GameObject>> _enemiesPools;
    private int enemiesPoolQty = 5;

    [Header("Enemies Spawn")]
    [SerializeField] private Transform[] _enemySpawners;
    private int _enemiesToSpawn = 20;
    private int _enemiesSpawned;

    [Header("Combat Area")]
    [SerializeField] private GameObject _combatArea;
    public bool PlayerInCombatArea { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        _enemiesPools = new List<List<GameObject>>();
        EnemyBlobPool = CreatePool(_enemyBlob, EnemyBlobPool, enemiesPoolQty);
        _enemiesPools.Add(EnemyBlobPool);
        EnemyOrcPool = CreatePool(_enemyOrc, EnemyOrcPool, enemiesPoolQty);
        _enemiesPools.Add(EnemyOrcPool);
        EnemyLeechPool = CreatePool(_enemyLeech, EnemyLeechPool, enemiesPoolQty);
        _enemiesPools.Add(EnemyLeechPool);

        PlayerInCombatArea = false;
    }

    private IEnumerator SpawnWavesLogic(int enemiesLeft)
    {
        Vector3 spawnPosition = _enemySpawners[Random.Range(0, _enemySpawners.Length)].position;
        if (enemiesLeft < 3 && _enemiesSpawned < _enemiesToSpawn)
        {
            yield return new WaitForSeconds(1);
            InstantiateEnemy(_enemiesPools[Random.Range(0, 3)], spawnPosition);
            _enemiesSpawned++;
        }
    }

    private List<GameObject> CreatePool(GameObject enemy, List<GameObject> enemiesPool, int maxQtyinPool)
    {
        GameObject tmp;

        for (int i=0; i < maxQtyinPool; i++)
        {
            tmp = Instantiate(enemy);
            tmp.SetActive(false);
            tmp.transform.SetParent(gameObject.transform);
            tmp.transform.localPosition = Vector3.zero;
            enemiesPool.Add(tmp);
        }

        return enemiesPool;
    }

    private GameObject TakeEnemyFromPool(List<GameObject> enemiesPool)
    {
        for (int i = 0; i < enemiesPool.Count; i++)
        {
            if (!enemiesPool[i].activeInHierarchy)
            {
                return enemiesPool[i];
            }
        }
        return null;
    }

    private void InstantiateEnemy(List<GameObject> enemiesPool, Vector3 spawnPosition)
    {
        GameObject enemy = TakeEnemyFromPool(enemiesPool);
        if(enemy != null)
        {
            enemy.transform.position = spawnPosition;
            //enemy.GetComponentInChildren<EnemyBaseController>().ResetPosition();
            enemy.SetActive(true);
        }
    }

    public void AddEnemyToList(GameObject enemy)
    {
        if (Vector3.Distance(enemy.transform.position, _combatArea.transform.position) < 30)
        {
            EnemiesInCombatArea.Add(enemy);
        }
        else
        {
            EnemiesList.Add(enemy);
        }
    }

    public void RemoveEnemyFromList(GameObject enemy)
    {
        for(int i = 0; i < EnemiesList.Count; i++)
        {
            if (EnemiesList[i].GetHashCode() == enemy.GetHashCode())
            {
                EnemiesList.RemoveAt(i);
                return;
            }

        }
        for (int j = 0; j < EnemiesInCombatArea.Count; j++)
        {
            if (EnemiesInCombatArea[j].GetHashCode() == enemy.GetHashCode())
            {
                Debug.Log("removing enemy from list");
                EnemiesInCombatArea.RemoveAt(j);
                StartCoroutine(SpawnWavesLogic(EnemiesInCombatArea.Count - 1));
                return;
            }
        }
    }

    public void NotifyEnemiesOfPlayerDetection(GameObject enemy, float distance)
    {
        for (int i = 0; i < EnemiesList.Count; i++)
        {
            if(Vector3.Distance(enemy.transform.position, EnemiesList[i].transform.position) < distance && EnemiesList[i] != null && 
                EnemiesList[i].GetHashCode() != enemy.GetHashCode())
            {
                EnemyBaseController enemyController = EnemiesList[i].GetComponentInParent<EnemyBaseController>();
                enemyController.GetNotificationOfChasing();
                Debug.Log($"notify {EnemiesList[i].name} {EnemiesList[i].gameObject.GetComponent<EnemyHealth>().GetInstanceID()}");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInCombatArea = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInCombatArea = false;
        }
    }
}
