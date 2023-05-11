using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerWeaponController : NetworkBehaviour, IBeforeUpdate
{
    public Quaternion localQuaternionPivotRot { get; private set; }

    [SerializeField] private Camera localCam;
    [SerializeField] private Transform pivotToRotate;

    [Networked] private Quaternion currentPlayerPivotRotation { get; set; } // bütün playerlarda sync etmek için
    public void BeforeUpdate()
    {
        if (Runner.LocalPlayer == Object.HasInputAuthority)
        {
            var direction = localCam.ScreenToWorldPoint(Input.mousePosition) - transform.position;

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            localQuaternionPivotRot = Quaternion.AngleAxis(angle, Vector3.forward); // z ekseninde rotasyon

        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.TryGetInputForPlayer<PlayerData>(Object.InputAuthority, out var input))
        {
            currentPlayerPivotRotation = input.GunPivotRotation; // this will get sync across all player

        }
        pivotToRotate.rotation = currentPlayerPivotRotation;
    }


}
