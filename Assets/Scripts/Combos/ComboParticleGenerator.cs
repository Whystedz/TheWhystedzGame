using UnityEngine;

public class ComboParticleGenerator : MonoBehaviour
{
    [SerializeField] private GameObject particleConnectionPrefab;

    private void Start()
    {
        var comboPlayer = transform.parent.GetComponent<ComboPlayer>();

        var teammates = comboPlayer.Teammates(false);

        foreach (var teammate in teammates)
        {
            var particleConnectionGO = Instantiate(this.particleConnectionPrefab, transform);
            var comboParticleIndicator = particleConnectionGO.GetComponent<ComboParticleIndicator>();
            
            comboParticleIndicator.SetPlayers(comboPlayer, teammate.GetComponent<ComboPlayer>());
            comboParticleIndicator.name = $"{comboPlayer.name}-to-{teammate.name} indicator";

            /* TODO: Nice to have - complete the particle effect logic for triangle combos - lines originating from the other two participants
            for (int j = 0; j < this.teammates.Count; j++)
            {
                if(this.teammates[i] != this.teammates[j])
                    Instantiate(this.particleConnectionPrefab, transform).GetComponent<ParticleConnection>().SetPlayers(this.teammates[i].GetComponent<Transform>(), this.teammates[j].GetComponent<Transform>());
            }*/
        }

        comboPlayer.InitializeComboParticleIndicators();
    }


}
