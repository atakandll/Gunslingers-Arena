using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float moveSpeed = 20;
    [SerializeField] private float lifeTimeAmount = 0.8f;

    [Networked] private NetworkBool didHitSomething { get; set; }
    [Networked] private TickTimer lifeTimeTimer { get; set; }
    private Collider2D collider2D;

    public override void Spawned()
    {
        collider2D = GetComponent<Collider2D>();
        lifeTimeTimer = TickTimer.CreateFromSeconds(Runner, lifeTimeAmount);
    }
    public override void FixedUpdateNetwork()
    {
        CheckIfHitGround();

        if (lifeTimeTimer.ExpiredOrNotRunning(Runner) == false && !didHitSomething) // eğer süres, geçmemiş ve çalışıyorsa
        {
            transform.Translate(transform.right * moveSpeed * Runner.DeltaTime, Space.World);

        }
        if (lifeTimeTimer.Expired(Runner) || didHitSomething) // süresi geçmişse
        {
            Runner.Despawn(Object); // obje yokoldu
        }
    }

    private void CheckIfHitGround()
    {
        var groundCollider = Runner.GetPhysicsScene2D().OverlapBox(transform.position, collider2D.bounds.size, 0, groundLayerMask);

        if (groundCollider != default)
        {
            didHitSomething = true;

        }

    }
}
