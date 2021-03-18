using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissapearOnLoad : MonoBehaviour
{
    void Start() => GetComponent<MeshRenderer>().enabled = false;

}
