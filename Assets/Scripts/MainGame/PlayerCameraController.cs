using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource ImpulseSource;
    public void ShakeCamera(Vector3 shakeAmount)
    {
        ImpulseSource.GenerateImpulse(shakeAmount);
    }

}
