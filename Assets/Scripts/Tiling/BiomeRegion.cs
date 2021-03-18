using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeRegion : MonoBehaviour
{
    [SerializeField] private List<GameObject> tiles;

    public GameObject GetRandomBiomeThemedTile() => tiles[Random.Range(0, tiles.Count)];

}
