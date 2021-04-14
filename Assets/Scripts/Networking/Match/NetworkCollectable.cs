using UnityEngine;
using Mirror;

public class NetworkCollectable : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnSetTrigger))]
    public bool isTriggerable;

    [SerializeField] private int pointsWorth;
    public int PointsWorth { get => this.pointsWorth; }

    [Command(ignoreAuthority = true)]
    public void CmdCollect()
    {
        this.Collect();
    }
    
    public virtual void Collect() 
    {
        NetworkServer.UnSpawn(this.gameObject);
        Destroy(this.gameObject);
    }

    public void SetTriggerable(bool isTrigger)
    {
        isTriggerable = isTrigger;
    }

    public void OnSetTrigger(bool oldValue, bool newValue)
    {
        this.GetComponent<Collider>().isTrigger = newValue;
    }
}
