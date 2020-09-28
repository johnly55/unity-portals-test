﻿using UnityEngine;

public class MainCamera : MonoBehaviour
{

    Portal[] portals;

    void Awake()
    {
        portals = FindObjectsOfType<Portal>();
    }
    void OnPreCull()
    {
        for (int i = 0; i < portals.Length; i++)
        {
            //portals[i].PreRender();
        }
        for (int i = 0; i < portals.Length; i++)
        {
            portals[i].Render();
        }

        for (int i = 0; i < portals.Length; i++)
        {
            //portals[i].PostRender();
        }

    }

}