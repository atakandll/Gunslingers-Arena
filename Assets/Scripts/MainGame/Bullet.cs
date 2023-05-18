using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private int bulletDamage = 10;
    [SerializeField] private float moveSpeed = 20;
    [SerializeField] private float lifeTimeAmount = 0.8f;

    [Networked] private NetworkBool didHitSomething { get; set; }
    [Networked] private TickTimer lifeTimeTimer { get; set; }
    private Collider2D coll;

    public override void Spawned()
    {
        coll = GetComponent<Collider2D>();

        lifeTimeTimer = TickTimer.CreateFromSeconds(Runner, lifeTimeAmount); // returns new ticktimer with the target tick calculated using amount of Ticks provided and current simulation tick.
    }
    public override void FixedUpdateNetwork()
    {
        if (!didHitSomething)
        {
            CheckIfHitGround();
            CheckIfWeHitAPlayer();

        }


        if (lifeTimeTimer.ExpiredOrNotRunning(Runner) == false && !didHitSomething) // eğer süres, geçmemiş ve çalışıyorsa
        {
            transform.Translate(transform.right * moveSpeed * Runner.DeltaTime, Space.World);

        }
        if (lifeTimeTimer.Expired(Runner) || didHitSomething) // süresi geçmişse
        {
            lifeTimeTimer = TickTimer.None;
            Runner.Despawn(Object); // obje yokoldu
        }
    }

    private void CheckIfHitGround()
    {
        // Find all colliders touching or inside of the given box.
        var groundCollider = Runner.GetPhysicsScene2D().OverlapBox(transform.position, coll.bounds.size, 0, groundLayerMask);

        if (groundCollider != default)
        {
            didHitSomething = true;

        }

    }
    private List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
    private void CheckIfWeHitAPlayer()
    {
        Runner.LagCompensation.OverlapBox(transform.position, coll.bounds.size, Quaternion.identity, Object.InputAuthority, hits, playerLayerMask);

        if (hits.Count > 0) // meaning we have something inside
        {
            foreach (var item in hits)
            {
                if (item.Hitbox != null)
                {
                    var player = item.Hitbox.GetComponentInParent<PlayerController>();
                    var didNotHitOurOwnPlayer = player.Object.InputAuthority.PlayerId != Object.InputAuthority.PlayerId; // we did not hit our own player.

                    if (didNotHitOurOwnPlayer && player.PlayerIsAlive) // if we dit not hit our player and player that we hit it is alive
                    {
                        if (Runner.IsServer)
                        {
                            //todo damage player
                            player.GetComponent<PlayerHealthController>().Rpc_ReducePlayerHealth(bulletDamage);

                        }

                        didHitSomething = true;
                        break;

                    }





                }

            }

        }

    }
}
