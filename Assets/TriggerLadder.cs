using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerLadder : MonoBehaviour
{
    [SerializeField] private Rope rope;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other) => rope.EnterLadderZone(other);

    private void OnTriggerExit(Collider other) => rope.ExitLadderZone(other);
}
