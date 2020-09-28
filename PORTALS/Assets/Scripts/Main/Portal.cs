using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Portal linkedPortal;
    private Camera portalCam;
    public Vector3 portalCamPosition {get{ return portalCam.transform.position; } }
    
    private RenderTexture portalTexture;
    public MeshRenderer screen;
    public Shader portalShader;

    static private Camera PlayerCam;

    private void Awake()
    {
        portalCam = GetComponentInChildren<Camera>();
        portalCam.enabled = false;

        if(PlayerCam == null)
            PlayerCam = Camera.main;
    }

    private void Update()
    {
        if (travellers.Count > 0)
        {
            for (int i = 0; i < travellers.Count; i++)
            {
                float dotProduct = Vector3.Dot(transform.forward, travellers[i].transform.position - transform.position);
                travellers[i].SliceTraveller(dotProduct, transform);

                if (dotProduct < 0 && travellers[i].dotNumber > 0)
                    travellers[i].isTravelling = true;
                else if (dotProduct > 0 && travellers[i].dotNumber < 0)
                    travellers[i].isTravelling = true;
                //Conditions met, starting teleportation
                if (travellers[i].isTravelling)
                {
                    travellers[i].isTravelling = false;

                    Travel(travellers[i]);
                    travellers.RemoveAt(i);
                }
            }
        }
        ProtectScreenFromClipping();
    }

    private void CreateViewTexture()
    {
        if (portalTexture == null || portalTexture.width != Screen.width || portalTexture.height != Screen.height)
        {
            if (portalTexture != null)
            {
                portalTexture.Release();
            }

            portalTexture = new RenderTexture(Screen.width, Screen.height, 0);
            portalCam.targetTexture = portalTexture;
            linkedPortal.screen.material.shader = portalShader;
            linkedPortal.screen.material.SetTexture("_MainTex", portalTexture);
        }
    }

    static bool VisibleFromCamera(Renderer renderer, Camera camera)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
    }

    private int recursionLimit = 2;
    public void Render()
    {
        if (!VisibleFromCamera(linkedPortal.screen, PlayerCam))
        {
            Texture2D testTexture = new Texture2D(1, 1);
            testTexture.SetPixel(0, 0, Color.black);
            testTexture.Apply();
            linkedPortal.screen.material.SetTexture("_MainTex", testTexture);
            return;
        }
        linkedPortal.screen.material.SetTexture("_MainTex", portalTexture);
        CreateViewTexture();

        Matrix4x4 localTolWorldMatrix = PlayerCam.transform.localToWorldMatrix;
        Matrix4x4[] matrices = new Matrix4x4[recursionLimit];
        for (int i = 0; i < recursionLimit; i++)
        {
            localTolWorldMatrix = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix * localTolWorldMatrix;
            matrices[recursionLimit - i - 1] = localTolWorldMatrix;
        }

        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        //screen.enabled = false;

        for (int i = 0; i < recursionLimit; i++)
        {
            portalCam.transform.SetPositionAndRotation(matrices[i].GetColumn(3), matrices[i].rotation);
            SetNearClipPlane();
            portalCam.Render();
        }
        //MovePortalCamera();

        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        //screen.enabled = true;
    }

    public void ProtectScreenFromClipping()
    {
        float dotProduct = Vector3.Dot(transform.TransformDirection(Vector3.forward), PlayerCam.transform.position - transform.position);
  
        Transform screenT = screen.transform;
        bool facingSameDirection = dotProduct < 0;
        screenT.localPosition = new Vector3(screenT.localPosition.x, screenT.localPosition.y, (facingSameDirection) ? (screenT.localScale.z / 2) : (-screenT.localScale.z / 2));
    }

    private float clipOffSet = 0f;
    private void SetNearClipPlane()//idk?????????????????????
    {
        Transform clipPlane = transform;
        int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - portalCam.transform.position));

        Vector3 camSpacePos = portalCam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        Vector3 camSpaceNormal = portalCam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
        float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + clipOffSet;
        Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

        portalCam.projectionMatrix = PlayerCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }

    private List<Traveller> travellers = new List<Traveller>();
    private void Travel(Traveller traveller)
    {
        //Corrects viewing angle before teleporting to match linked portal's camera
        float angle = PlayerCam.transform.rotation.eulerAngles.y - linkedPortal.portalCam.transform.rotation.eulerAngles.y;
        Quaternion newLook = Quaternion.Euler(0, -angle, 0);

        //Prevents player position and rotation from being off
        traveller.transform.rotation *= newLook;
        linkedPortal.MovePortalCamera();

        traveller.transform.position = linkedPortal.portalCam.transform.position;
        //Prevents clipping right after teleporting
        linkedPortal.ProtectScreenFromClipping();
        traveller.SliceTraveller(Vector3.Dot(linkedPortal.transform.forward, traveller.transform.position - linkedPortal.transform.position), linkedPortal.transform);

        //Using the OnTrigger Methods are slower and may show clipping
        traveller.dotNumber = Vector3.Dot(linkedPortal.transform.TransformDirection(Vector3.forward), traveller.transform.position - linkedPortal.transform.position);
        linkedPortal.travellers.Add(traveller);
    }

    //The Update Method deals with adding the traveller to the Travellers List of each portal
    //However if the traveller does not teleport and merely enters or exit the teleport range
    //Then the OnTriggerEnter and OnTriggerExit Methods deal with said traveller
    private void OnTriggerEnter(Collider col)
    {
        Traveller traveller = col.GetComponent<Traveller>();
        if (traveller)
        {
            if (travellers.Contains(traveller))
                return;
            travellers.Add(traveller);
            traveller.dotNumber = Vector3.Dot(transform.TransformDirection(Vector3.forward), traveller.transform.position - transform.position);
        }
    }

    private void OnTriggerExit(Collider col)
    {
        Traveller traveller = col.GetComponent<Traveller>();
        if (traveller)
        {
            if(travellers.Contains(traveller))
                travellers.Remove(traveller);
            if (!linkedPortal.travellers.Contains(traveller))
                traveller.DisableSlicing();
        }
    }

    private void MovePortalCamera()
    {
        //The order matters
        Matrix4x4 m = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix * PlayerCam.transform.localToWorldMatrix;
        portalCam.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
        if(linkedPortal.travellers.Count > 0)
        {
            foreach(Traveller traveller in linkedPortal.travellers)
            {
                //Using this method here was found to provide the best results visually
                traveller.SliceClone(this, m);
            }
        }
    }
}
