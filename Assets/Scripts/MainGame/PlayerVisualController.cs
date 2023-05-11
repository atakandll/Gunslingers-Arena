using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    [SerializeField] private Animator anim;

    private readonly int isMovingHash = Animator.StringToHash("isWalking");

    public void RendererVisuals(Vector2 velocity)
    {
        var isMoving = velocity.x > 0.1f || velocity.x < -0.1f;

        anim.SetBool(isMovingHash, isMoving);
    }
}
