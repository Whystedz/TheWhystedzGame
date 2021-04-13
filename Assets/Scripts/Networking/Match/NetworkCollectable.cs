using UnityEngine;
using Mirror;

public class NetworkCollectable : NetworkBehaviour
{
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
}
