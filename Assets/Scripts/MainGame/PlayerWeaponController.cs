using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerWeaponController : NetworkBehaviour, IBeforeUpdate
{
    public Quaternion localQuaternionPivotRot { get; private set; }
    [SerializeField] private float delayBetweenShots = 0.18f;
    [SerializeField] private ParticleSystem muzzleEffect;

    [SerializeField] private Camera localCam;
    [SerializeField] private Transform pivotToRotate;
    [Networked, HideInInspector] public NetworkBool isHoldingShootingKey { get; private set; }

    [Networked(OnChanged = nameof(OnMuzzleEffectStateChanged))] private NetworkBool playMuzzleEffect { get; set; }

    [Networked] private Quaternion currentPlayerPivotRotation { get; set; } // bütün playerlarda sync etmek için

    [Networked] private NetworkButtons buttonsPrev { get; set; } // button previous
    [Networked] private TickTimer shootCoolDown { get; set; }
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
        //host and server aynı
        //check if you are the local player or the host
        if (Runner.TryGetInputForPlayer<PlayerData>(Object.InputAuthority, out var input))
        {
            CheckShootInput(input);
            // eğer bunun içinde rotasyonu yapsaydım other clients senkronize olmucaktı
            currentPlayerPivotRotation = input.GunPivotRotation; // this will get sync across all player
            buttonsPrev = input.NetworkButtons;

        }
        pivotToRotate.rotation = currentPlayerPivotRotation;
    }
    private void CheckShootInput(PlayerData input) // burda karşılaştırm yapıyoruz
    {
        var currentButtons = input.NetworkButtons.GetPressed(buttonsPrev);
        isHoldingShootingKey = currentButtons.WasReleased(buttonsPrev, PlayerController.PlayerInputButtons.Shoot);

        if (currentButtons.WasReleased(buttonsPrev, PlayerController.PlayerInputButtons.Shoot) && shootCoolDown.ExpiredOrNotRunning(Runner))
        {
            shootCoolDown = TickTimer.CreateFromSeconds(Runner, delayBetweenShots); // delayin olduğ yer
            Debug.Log("Shoot");

        }
        else
        {
            // todo close the particular effect

        }

    }
    private static void OnMuzzleEffectStateChanged(Changed<PlayerWeaponController> changed)
    {
        var currentState = changed.Behaviour.playMuzzleEffect;

        changed.LoadOld();
        var oldState = changed.Behaviour.playMuzzleEffect;

        if (oldState != currentState)
        {
            changed.Behaviour.PlayOrStopMuzzleEffect(currentState);
        }
    }
    private void PlayOrStopMuzzleEffect(bool play)
    {
        if (play)
        {
            muzzleEffect.Play();
        }
        else
        {
            muzzleEffect.Stop();
        }
    }


}
