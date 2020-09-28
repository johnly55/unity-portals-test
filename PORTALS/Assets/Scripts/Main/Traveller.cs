using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Traveller : MonoBehaviour
{
    public float dotNumber;
    public bool isTravelling = false;

    private Material travellersMaterial;

    public GameObject cloneGFX;
    private Material cloneMaterial;

    private void Awake()
    {
        travellersMaterial = GetComponentInChildren<Renderer>().material;

        if (!cloneGFX)
        {
            foreach(GameObject child in transform)
            {
                if(child.name == "GFX")
                { 
                    cloneGFX = child;
                    break;
                }
            }
        }

        cloneGFX = GameObject.Instantiate(cloneGFX);
        cloneGFX.SetActive(false);

        cloneMaterial = cloneGFX.GetComponent<Renderer>().material;

    }
    float enteredDotProduct;
    public void SliceTraveller(float dot, Transform t)
    {
        Vector3 side = t.forward;
        enteredDotProduct = dot;
        if (dot >= 0)
            side = -side;

        travellersMaterial.SetVector("sliceNormal", side);//Anything past this direction is invisible
        travellersMaterial.SetVector("sliceCentre", t.position);//Where the slicing begins
    }

    public void SliceClone(Portal linkedPortal, Matrix4x4 m)
    {
        if (!cloneGFX.activeSelf)
            cloneGFX.SetActive(true);

        cloneMaterial.SetVector("sliceNormal", (enteredDotProduct >= 0) ? linkedPortal.transform.forward : -linkedPortal.transform.forward);
        cloneMaterial.SetVector("sliceCentre", linkedPortal.transform.position);
        cloneGFX.transform.position = m.GetColumn(3);
    }

    public void DisableSlicing()
    {
        travellersMaterial.SetVector("sliceNormal", Vector3.zero);

        cloneMaterial.SetVector("sliceNormal", Vector3.zero);
        cloneGFX.SetActive(false);
    }
}
