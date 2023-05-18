using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource ImpulseSource;
    [SerializeField] private CinemachineConfiner2D confiner2D;
    private void Start()
    {

        confiner2D.m_BoundingShape2D = GlobalManagers.instance.gameManager.CameraBounds;



    }

    public void ShakeCamera(Vector3 shakeAmount)
    {
        ImpulseSource.GenerateImpulse(shakeAmount);
    }

}
