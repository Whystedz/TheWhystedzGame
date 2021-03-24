using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class will keep spawning a specified prefab at a specified rate.
/// 
/// Last updated 20-03-2021 by Shifat
/// </summary>
public class PrefabSpawner : MonoBehaviour
{
    [Header("Spawn variables")]
    [SerializeField] private bool keepSpawning = true;
    [SerializeField] private float minSpawnTime;
    [SerializeField] private float maxSpawnTime;

    [Header("Spawn game objects")]
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private Transform[] spawnPoints;

    void Start() => StartCoroutine(SpawnAtIntervals());

    IEnumerator SpawnAtIntervals()
    {
        // Repeat until keepSpawning is false or this GameObject is disabled/destroyed.
        while (this.keepSpawning)
        {
            // Put this coroutine to sleep until the next spawn time.
            yield return new WaitForSeconds(Random.Range(this.minSpawnTime, this.maxSpawnTime));

            // Now it's time to spawn again.
            Spawn();
        }
    }

    void Spawn()
    {
        int spawnIndex = Random.Range(0, this.spawnPoints.Length);
        Instantiate(this.prefabToSpawn, this.spawnPoints[spawnIndex].position, this.prefabToSpawn.transform.rotation);
    }
}
