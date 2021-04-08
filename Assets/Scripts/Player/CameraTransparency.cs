using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CameraTransparency : MonoBehaviour
{
    private List<Renderer> allRenderers = new List<Renderer>();
    private List<Material[]> materialSolidStates = new List<Material[]>();
    private List<Renderer> currentRenderers = new List<Renderer>();
    private List<Renderer> removeRenderers = new List<Renderer>();

    private Vector3 raycastDirection;
    private Camera mainCamera;

    private void Start() => this.mainCamera = Camera.main;

    private void Update()
    {
        this.currentRenderers.Clear();
        this.removeRenderers.Clear();

        RaycastHit[] hits;
        this.raycastDirection = mainCamera.transform.position - transform.position;
        hits = Physics.RaycastAll(transform.position, this.raycastDirection, Vector3.Distance(transform.position, this.mainCamera.transform.position));
        
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.transform.tag == "Obstacle" && hit.transform.GetComponent<Renderer>())
            {
                Renderer rend = hit.transform.GetComponent<Renderer>();
                Material[] mats = rend.materials;

                if (!this.allRenderers.Contains(rend))
                {
                    this.allRenderers.Add(rend);
                    Material[] clone = DeepCopy(rend.materials);
                    this.materialSolidStates.Add(clone);

                    for (int j = 0; j < mats.Length; j++)
                        SetFloatsTransparent(mats[j]);
                }

                if (!this.currentRenderers.Contains(rend))
                    this.currentRenderers.Add(rend);
            }
        }

        this.removeRenderers = this.allRenderers.Except(this.currentRenderers).ToList();
        for (int i = 0; i < this.removeRenderers.Count; i++)
        {
            Renderer rend = this.removeRenderers[i];
            Material[] mats = rend.materials;

            for (int j = 0; j < mats.Length; j++)
                SetFloatsSolid(mats[j], this.materialSolidStates[allRenderers.IndexOf(rend)][j]);

            this.materialSolidStates.RemoveAt(this.allRenderers.IndexOf(rend));
            this.allRenderers.Remove(rend);
        }
    }

    private void SetFloatsTransparent(Material mat)
    {
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.renderQueue = 3000;

        mat.SetFloat("_Blend", 2.0f);
        mat.SetFloat("_DstBlend", 1.0f);
        mat.SetFloat("_SrcBlend", 5.0f);
        mat.SetFloat("_Surface", 1.0f);
        mat.SetFloat("_ZWrite", 0.0f);
        mat.SetFloat("_Cull", 0.0f);
    }

    private void SetFloatsSolid(Material mat, Material materialSolidState)
    {
        mat.SetOverrideTag("RenderType", "Opaque");
        mat.renderQueue = 2000;

        mat.SetFloat("_Blend", materialSolidState.GetFloat("_Blend"));
        mat.SetFloat("_DstBlend", materialSolidState.GetFloat("_DstBlend"));
        mat.SetFloat("_SrcBlend", materialSolidState.GetFloat("_SrcBlend"));
        mat.SetFloat("_Surface", materialSolidState.GetFloat("_Surface"));
        mat.SetFloat("_ZWrite", materialSolidState.GetFloat("_ZWrite"));
        mat.SetFloat("_Cull", materialSolidState.GetFloat("_Cull"));
    }

    private Material[] DeepCopy(Material[] originalMaterials)
    {
        Material[] copiedMaterials = new Material[originalMaterials.Length];

        for(int i = 0; i < originalMaterials.Length; i++)
            copiedMaterials[i] = new Material(originalMaterials[i]);

        return copiedMaterials;
    }
}








