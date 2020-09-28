﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliceTest : MonoBehaviour
{
    public GameObject graphic;
    Material[] materials;
    public float x = 0;

    void Start()
    {
        materials = GetMaterials(graphic);    
    }

    void Update()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetVector("sliceCentre", transform.position);
            materials[i].SetVector("sliceNormal", transform.forward);
            materials[i].SetFloat("sliceOffsetDst", x);
        }
    }

    Material[] GetMaterials(GameObject g)
    {
        var renderers = g.GetComponentsInChildren<MeshRenderer>();
        var matList = new List<Material>();
        foreach(var renderer in renderers)
        {
            foreach(var mat in renderer.materials)
            {
                matList.Add(mat);
            }
        }
        return matList.ToArray();
    }
}
