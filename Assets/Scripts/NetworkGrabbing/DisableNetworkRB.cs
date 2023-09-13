using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using BNG;
public class DisableNetworkRB : MonoBehaviour
{
    public Transform GrabbableObjectVisuals;
    public Transform GrabbableObject;

    public NetworkRigidbody nrb;

    public void DisableNRB()
    {
         nrb.enabled = false;
        
       // nrb.InterpolationDataSource = NetworkBehaviour.InterpolationDataSources.NoInterpolation;
    }

    public void EnableNRB()
    {
        GrabbableObjectVisuals.gameObject.GetComponent<MeshRenderer>().enabled = false;
        //ReleaseVisuals();
        //nrb.enabled = true;
        //nrb.InterpolationDataSource = NetworkBehaviour.InterpolationDataSources.Auto;
        ReleaseVisuals();
    }

    public void ReleaseVisuals()
    {

      // GrabbableObjectVisuals.gameObject.GetComponent<MeshRenderer>().enabled = false;
       //GrabbableObjectVisuals.SetPositionAndRotation(GrabbableObject.position, GrabbableObject.rotation);
        Invoke(nameof(MakeVisable), 0.1f);
    }

    void MakeVisable()
    {
        GrabbableObjectVisuals.gameObject.GetComponent<MeshRenderer>().enabled = true;
    }
}
