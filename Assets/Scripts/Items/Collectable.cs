using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField] private int pointsWorth;
    public int PointsWorth { get => this.pointsWorth; }

    public virtual void Collect() {
        Destroy(this.gameObject);
    }
}
