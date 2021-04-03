using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneHierarchyFolder : MonoBehaviour
{
    void Awake() => ResetTransform();

    private void ResetTransform()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
}
