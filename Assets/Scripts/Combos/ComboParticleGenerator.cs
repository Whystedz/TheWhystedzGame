using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboParticleGenerator : MonoBehaviour
{
    private ComboPlayer comboPlayer;
    private List<ComboPlayer> teammates;

    [SerializeField] private GameObject particleConnectionPrefab;

    private void Start()
    {
        this.comboPlayer = transform.parent.GetComponent<ComboPlayer>();
        this.teammates = this.comboPlayer.Teammates(false);

        for(int i = 0; i < this.teammates.Count; i++)
        {
            var particleConnectionGameObject = Instantiate(this.particleConnectionPrefab, transform);
            var comboParticleIndicator = particleConnectionGameObject.GetComponent<ComboParticleIndicator>();
            comboParticleIndicator.SetPlayers(this.comboPlayer, this.teammates[i].GetComponent<ComboPlayer>());
            
            /* TODO: Nice to have - complete the particle effect logic for triangle combos - lines originating from the other two participants
            for (int j = 0; j < this.teammates.Count; j++)
            {
                if(this.teammates[i] != this.teammates[j])
                    Instantiate(this.particleConnectionPrefab, transform).GetComponent<ParticleConnection>().SetPlayers(this.teammates[i].GetComponent<Transform>(), this.teammates[j].GetComponent<Transform>());
            }*/
        }
    }


}
