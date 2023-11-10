using UnityEngine;
using Photon.Voice.Unity;
using Photon.Voice;
using Fusion;
using Photon.Voice.Fusion;

namespace Chiligames.MetaAvatarsFusion
{
    public class SetMicrophone : MonoBehaviour
    {
        //For making sure that microphone is found and set to "Recorder" component from Photon Voice
        private void Start()
        {
            if (GetComponent<NetworkBehaviour>().Object.HasStateAuthority)
            {
                string[] devices = Microphone.devices;
                if (devices.Length > 0)
                {
                    var recorder = GetComponent<Recorder>();
                    recorder.MicrophoneDevice = new DeviceInfo(devices[0]);
                    var fusionVoiceClient = FindObjectOfType<FusionVoiceClient>();
                }
            }
        }
    }
}
