using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;

public class PlayerHealthController : NetworkBehaviour
{
    [SerializeField] private LayerMask deathGroundLayerMask;
    [SerializeField] private Animator bloodScreenAnimator;
    [SerializeField] private PlayerCameraController playerCameraController;
    [SerializeField] private Image fillAmountImage;
    [SerializeField] TextMeshProUGUI healtAmountText;
    [Networked(OnChanged = nameof(HealthAmountChanged))] private int currentHealthAmount { get; set; }
    private const int MAX_HEALTH_AMOUNT = 100;
    private PlayerController playerController;
    private Collider2D coll;

    public override void Spawned() // spawned will be called on the host
    {
        coll = GetComponent<Collider2D>();
        playerController = GetComponent<PlayerController>();
        currentHealthAmount = MAX_HEALTH_AMOUNT; // canı ilk başkta 100 yaptık.

    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.IsServer && playerController.PlayerIsAlive)
        {
            var didHitCollider = Runner.GetPhysicsScene2D().OverlapBox(transform.position, coll.bounds.size, 0, deathGroundLayerMask);

            if (didHitCollider != default)
            {
                Rpc_ReducePlayerHealth(MAX_HEALTH_AMOUNT);
            }
        }



    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void Rpc_ReducePlayerHealth(int damage)
    {
        currentHealthAmount -= damage;
    }

    private static void HealthAmountChanged(Changed<PlayerHealthController> changed) // we wanna update our visuals and do a player data animation if he is dead and so on.
    {
        var currentHealth = changed.Behaviour.currentHealthAmount;

        changed.LoadOld(); // load previous data

        var oldHealthAmount = changed.Behaviour.currentHealthAmount;

        //only if the current health is not the same as the previous one
        if (currentHealth != oldHealthAmount)
        {
            changed.Behaviour.UpdateVisuals(currentHealth); // we will update visuals

            // We did not respawn or just spawned.
            if (currentHealth != MAX_HEALTH_AMOUNT)
            {
                changed.Behaviour.PlayerGotHit(currentHealth);

            }

        }


    }
    private void UpdateVisuals(int healthAmount)
    {
        var num = (float)healthAmount / MAX_HEALTH_AMOUNT;
        fillAmountImage.fillAmount = num;

        healtAmountText.text = $"{healthAmount}/{MAX_HEALTH_AMOUNT}";
    }
    private void PlayerGotHit(int healtAmount)
    {
        var isLocalPlayer = Runner.LocalPlayer == Object.HasInputAuthority;

        if (isLocalPlayer)
        {
            //todo do blood hit aniamtion, shake camera etc.
            Debug.Log("Local player got hit");
            bloodScreenAnimator.Play("BloodScreenHit");

            var shakeAmount = new Vector3(0.2f, 0.1f);
            playerCameraController.ShakeCamera(shakeAmount);
        }
        if (healtAmount <= 0)
        {
            //todo kill the player
            playerController.KillPlayer();
            Debug.Log("Player is dead");
        }
    }

    public void ResetHealthAmountToMax() // respawn olurken canın resetlenmesini sağlıyor.
    {
        currentHealthAmount = MAX_HEALTH_AMOUNT;

    }

}
