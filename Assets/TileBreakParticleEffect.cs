using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBreakParticleEffect : MonoBehaviour
{
    private float duration;

    private void Awake()
    {
        this.duration = GetComponent<ParticleSystem>().main.duration;
        Destroy(gameObject, this.duration);
    }

}
