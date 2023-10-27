using UnityEngine;
using UnityEngine.XR;
using TMPro;

[RequireComponent(typeof(InputData))]
public class DisplayInputData : MonoBehaviour
{
    private InputData _inputData;

    private void Start()
    {
        _inputData = GetComponent<InputData>();
    }


    
    float output;
    private float fistMass = 80f;
    public float CalculateHitForce(bool isRight)
    {
        if(!isRight){
            if (_inputData._leftController.TryGetFeatureValue(CommonUsages.deviceAcceleration, out Vector3 leftAcceleration))
            {
                float leftForce = leftAcceleration.magnitude * fistMass;
        
                
                output = leftForce;
            }

        }
        
        if(isRight){
            if (_inputData._rightController.TryGetFeatureValue(CommonUsages.deviceAcceleration, out Vector3 rightAcceleration))
            {
                float rightForce = rightAcceleration.magnitude * fistMass;
           
                output = rightForce;
            }

        }
        
        return output;
    }

 
    
    
}