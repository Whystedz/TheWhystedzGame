using UnityEngine;

public class NetworkRopeTrigger : MonoBehaviour
{
    [SerializeField] private NetworkRope rope;
    private void OnTriggerEnter(Collider other) => rope.EnterLadderZone(other);
    private void OnTriggerExit(Collider other) => rope.ExitLadderZone(other);
}
