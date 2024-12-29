using Fusion;
using Projectiles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static UnityEditor.Progress;

public class IEnemySpawner_Network : NetworkBehaviour
{
    public IEnemyFSM_Network monster;//관리할 몬스터 프리팹타깃(IEnemySpawner하나당 한개씩의 몬스터관리)
    public int Count = 1;
    public float SpawnRadius = 50f;
    public float SpawnHeightMin = 0f;
    public float SpawnHeightMax = 20f;

    public List<IEnemyFSM_Network> _enemies = new(128);

    public override void Spawned()
    {
        if (HasStateAuthority == false)
            return;

        //On start just show all chickens 네트워크 스폰시킨다>>
        for (int i = 0; i < Count; i++)
        {
            Debug.Log(i + "| NetworkSpawner Enemy");
            var enemyObj = Runner.Spawn(monster, transform.position, Quaternion.identity);
            enemyObj.GetComponent<IEnemyFSM_Network>().SetSpawnPoint(transform);
            _enemies.Add(enemyObj);

            Respawn(enemyObj);
        }
    }
    public override void FixedUpdateNetwork()
    {
        for (int i = 0; i < _enemies.Count; i++)
        {
            var enemyObj = _enemies[i];

            if (enemyObj)
            {
                if (/*enemyObj.IsDied*/enemyObj.Health_.IsFinished)
                {
                    enemyObj.gameObject.SetActive(false);
                    //enemyObj.OnDieReset();
                    //Respawn(enemyObj);
                    StartCoroutine(RespawnCoroutine(enemyObj));          
                }
            }
        }
    }
    private IEnumerator RespawnCoroutine(IEnemyFSM_Network enemy_)
    {
        yield return new WaitForSeconds(0.01f);
        Debug.Log("0.01초뒤 적군 재 생성>>");
        enemy_.gameObject.SetActive(true);
        Respawn(enemy_);
    }
    private void Respawn(IEnemyFSM_Network enemy)
    {
        var position = transform.position + new Vector3(0, 0.2f, 0);

        //if (!enemy.IsRevivePosExecute)
        //{
            enemy.Respawn(position, Quaternion.identity);
            enemy.StartPosSetup(position);
        //}
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, SpawnRadius);
    }
    //public bool respawn;
    //public float spawnDelay;
    //private float currentTime;
    //private bool spawning;
    //public MemoryPool enemyMemoryPool;

    //[SerializeField] private EnemyProjectileMemoryPool enemyProjectileMemoryPool;

    /*public bool isDestroyed = false;
    private void Awake()
    {
        enemyMemoryPool = new MemoryPool(monster, 5);//5개 이상씩은 넘치지 않게 관리
    }
    public void DeactivateEnemy(GameObject enemy)
    {
        enemyMemoryPool.DeactivatePoolItem(enemy);
    }
    public void DestroySpawner()
    {
        isDestroyed = true;
    }
    private void Start()
    {
        StartCoroutine(firstStartSpawn());
        currentTime = spawnDelay;
    }
    private IEnumerator firstStartSpawn()
    {
        yield return new WaitForSeconds(2f);
        Spawn();
        yield return null;
    }

    private void Update()
    {
        if (spawning)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                Spawn();
            }
        }
    }

    public void Respawn()
    {
        Debug.Log("몬스터Respawn!: memoryPool pooling 생성");
        spawning = true;
        currentTime = spawnDelay;
    }

    public void Spawn()
    {
        Debug.Log("몬스터Spawn: memoryPool pooling 생성");
        //IEnemy instance = Instantiate(monster, transform.position, Quaternion.identity).GetComponent<IEnemy>();
        GameObject monsterPrefab = enemyMemoryPool.ActivatePoolItem();
        if (monsterPrefab != null)
        {
            monsterPrefab.transform.position = transform.position;
            IEnemy instance = monsterPrefab.transform.GetComponent<IEnemy>();
            instance.Spawner = this;
            monsterPrefab.GetComponent<IEnemyFSM>().Setup(this);
            Debug.Log("해당 몬스터 IEnemyFSM 해당 위치로 Warp>>" + transform.position);
            monsterPrefab.GetComponent<IEnemyFSM>().StartPosSetup(transform.position);
            spawning = false;
        }
    }*/
}