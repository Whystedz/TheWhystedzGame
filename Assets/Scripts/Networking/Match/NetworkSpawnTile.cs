
public class NetworkSpawnTile : NetworkTile
{
    protected override void Start()
    {
        base.Start();

        SetUnbreakable();
    }
}
