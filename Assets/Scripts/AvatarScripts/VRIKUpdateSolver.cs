using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
using Fusion;
public class VRIKUpdateSolver : NetworkBehaviour
{
    public VRIK vrik;

    public override void Spawned()
    {
        vrik = GetComponent<VRIK>();
    }

    public override void Render()
    { 
        vrik.solver.Update();
    }
}
