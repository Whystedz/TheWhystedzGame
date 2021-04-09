using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkComboParticleIndicator : MonoBehaviour
{
    private NetworkComboPlayer targetPlayer;
    private NetworkComboPlayer originPlayer;

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

    public ParticleSystem hintParticleSystem;
    private bool showParticles;

    public void Awake() => this.hintParticleSystem = GetComponent<ParticleSystem>();

    private void Start() => this.hintParticleSystem.Stop();

    public void SetPlayers(NetworkComboPlayer initializingPlayer, NetworkComboPlayer targetPlayer)
    {
        this.originPlayer = initializingPlayer;
        this.targetPlayer = targetPlayer;
    }

    public void UpdateParticles(IEnumerable<ComboInfo> combos, IEnumerable<ComboHintInfo> comboHints)
    {
        this.showParticles = false; // Assume false until we find one

        ParticleSystem particleSystem = GetComponent<ParticleSystem>();

        var particleSystemMain = particleSystem.main;
        particleSystemMain.startSpeed = Vector3.Distance(this.originPlayer.transform.position, this.targetPlayer.transform.position);

        foreach (var comboHint in comboHints)
        {
            if (comboHint.OriginPlayer == this.originPlayer
                && comboHint.TargetPlayer == this.targetPlayer)
            {
                particleSystemMain = IndicateHint(particleSystemMain);
                break;
            }
        }

        foreach (var combo in combos)
        {
            if (combo.Players.Contains(this.targetPlayer))
            {
                IndicateTriggerableCombo(particleSystemMain, combo);
                break;
            }
        }

        UpdatePlayback();

        UpdateTransform();
    }

    private void IndicateTriggerableCombo(ParticleSystem.MainModule particleSystemMain, ComboInfo combo)
    {
        this.showParticles = true;
        if (combo.ComboType == ComboType.Line)
            particleSystemMain.startColor = new ParticleSystem.MinMaxGradient(this.colorGradientLineCombo1, this.colorGradientLineCombo2);
        else
            particleSystemMain.startColor = new ParticleSystem.MinMaxGradient(this.colorGradientTriangleCombo1, this.colorGradientTriangleCombo2);

        particleSystemMain.startSize = new ParticleSystem.MinMaxCurve(this.particleComboSizeFrom, this.particleComboSizeTo);
    }

    private ParticleSystem.MainModule IndicateHint(ParticleSystem.MainModule particleSystemMain)
    {
        this.showParticles = true;
        particleSystemMain.startColor = new ParticleSystem.MinMaxGradient(this.colorGradientHint1, this.colorGradientHint2);
        particleSystemMain.startSize = new ParticleSystem.MinMaxCurve(this.particleHintSizeFrom, this.particleHintSizeTo);
        return particleSystemMain;
    }

    private void UpdatePlayback()
    {
        if (this.showParticles && !this.hintParticleSystem.isPlaying)
            this.hintParticleSystem.Play();

        if (!this.showParticles && this.hintParticleSystem.isPlaying)
            this.hintParticleSystem.Stop();
    }

    private void UpdateTransform()
    {
        transform.position = this.originPlayer.transform.position;
        transform.rotation = GetRotation();
    }

    private Quaternion GetRotation()
    {
        var eulerVector = new Vector3(
            0f, 
            Mathf.Atan2(this.targetPlayer.transform.position.x - this.originPlayer.transform.position.x, this.targetPlayer.transform.position.z - this.originPlayer.transform.position.z) * Mathf.Rad2Deg, 
            0f);
        
        return Quaternion.Euler(eulerVector);
    }
}