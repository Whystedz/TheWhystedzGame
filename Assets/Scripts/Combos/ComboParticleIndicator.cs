using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboParticleIndicator : MonoBehaviour
{
    private ComboPlayer targetPlayer;
    private ComboPlayer initializingPlayer;

    [SerializeField] private float particleHintSizeFrom;
    [SerializeField] private float particleHintSizeTo;
    [SerializeField] private Gradient colorGradientHint1;
    [SerializeField] private Gradient colorGradientHint2;

    [SerializeField] private float particleComboSizeFrom;
    [SerializeField] private float particleComboSizeTo;
    [SerializeField] private Gradient colorGradientLineCombo1;
    [SerializeField] private Gradient colorGradientLineCombo2;

    [SerializeField] private Gradient colorGradientTriangleCombo1;
    [SerializeField] private Gradient colorGradientTriangleCombo2;

    private ParticleSystem particleSystem;
    private bool showParticles;

    private void Awake() => this.particleSystem = GetComponent<ParticleSystem>();

    public void SetPlayers(ComboPlayer initializingPlayer, ComboPlayer targetPlayer)
    {
        this.initializingPlayer = initializingPlayer;
        this.targetPlayer = targetPlayer;
    }

    public void UpdateParticles(List<Combo> Combos, List<ComboHint> ComboHints)
    {
        this.showParticles = false;

        var particleSystemMain = this.particleSystem.main;
        particleSystemMain.startSpeed = Vector3.Distance(this.initializingPlayer.transform.position, this.targetPlayer.transform.position);

        for (int i = 0; i < ComboHints.Count; i++)
        {
            if (ComboHints[i].OriginPlayer == this.initializingPlayer
                && ComboHints[i].TargetPlayer == this.targetPlayer)
            {
                this.showParticles = true;
                particleSystemMain.startColor = new ParticleSystem.MinMaxGradient(this.colorGradientHint1, this.colorGradientHint2);
                particleSystemMain.startSize = new ParticleSystem.MinMaxCurve(this.particleHintSizeFrom, this.particleHintSizeTo);
                break;
            }
        }

        for (int i = 0; i < Combos.Count; i++)
        {
            if (Combos[i].InitiatingPlayer == this.initializingPlayer
                && Combos[i].Players.Contains(this.targetPlayer))
            {
                this.showParticles = true;
                if (Combos[i].ComboType == ComboType.Line)
                    particleSystemMain.startColor = new ParticleSystem.MinMaxGradient(this.colorGradientLineCombo1, this.colorGradientLineCombo2);
                else
                    particleSystemMain.startColor = new ParticleSystem.MinMaxGradient(this.colorGradientTriangleCombo1, this.colorGradientTriangleCombo2);
                
                particleSystemMain.startSize = new ParticleSystem.MinMaxCurve(this.particleComboSizeFrom, this.particleComboSizeTo);
                
                break;
            }
        }

        if (this.showParticles && !this.particleSystem.isPlaying)
            this.particleSystem.Play();
        
        if(!this.showParticles && this.particleSystem.isPlaying)
            this.particleSystem.Stop();
        
        transform.position = this.initializingPlayer.transform.position;
        transform.rotation = Quaternion.Euler(new Vector3(0f, Mathf.Atan2(this.targetPlayer.transform.position.x - this.initializingPlayer.transform.position.x, this.targetPlayer.transform.position.z - this.initializingPlayer.transform.position.z) * Mathf.Rad2Deg, 0f));
    }
}