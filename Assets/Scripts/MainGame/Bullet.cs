using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float moveSpeed = 20;
    [SerializeField] private float lifeTimeAmount = 0.8f;

    [Networked] private NetworkBool didHitSomething { get; set; }
    [Networked] private TickTimer lifeTimeTimer { get; set; }
    private Collider2D collider2D;

    public override void Spawned()
    {
        collider2D = GetComponent<Collider2D>();

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
        // mermi için raycast yaptık
        var groundCollider = Runner.GetPhysicsScene2D().OverlapBox(transform.position, collider2D.bounds.size, 0, groundLayerMask);

        if (groundCollider != default)
        {
            didHitSomething = true;

        }

    }
    private List<LagCompensatedHit> hits = new List<LagCompensatedHit>();
    private void CheckIfWeHitAPlayer()
    {
        Runner.LagCompensation.OverlapBox(transform.position, collider2D.bounds.size, Quaternion.identity, Object.InputAuthority, hits, playerLayerMask);

        if (hits.Count > 0) // meaning we have something inside
        {
            foreach (var item in hits)
            {
                if (item.Hitbox != null)
                {
                    var player = item.Hitbox.GetComponentInParent<NetworkObject>();
                    var didNotHitOurOwnPlayer = player.InputAuthority.PlayerId != Object.InputAuthority.PlayerId; // we did not hit our own player.

                    if (didNotHitOurOwnPlayer)
                    {
                        //todo damage player
                        didHitSomething = true;
                        break;
                    }



                }

            }

        }

    }
}
