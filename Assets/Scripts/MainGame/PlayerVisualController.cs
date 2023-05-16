using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private Transform pivotGunTr;
    [SerializeField] private Transform canvasTr;

    private readonly int isMovingHash = Animator.StringToHash("isWalking");
    private readonly int isShootingHash = Animator.StringToHash("isShooting");



    private bool isFacingRight = true;
    private Vector3 originalPlayerScale;
    private Vector3 originalGunPivotScale;
    private Vector3 originalCanvasScale;
    private bool init;

    private void Start()
    {
        originalPlayerScale = this.transform.localScale;
        originalCanvasScale = canvasTr.transform.localScale;
        originalGunPivotScale = pivotGunTr.transform.localScale;

        const int SHOOTING_LAYER_INDEX = 1;
        anim.SetLayerWeight(SHOOTING_LAYER_INDEX, 1);

        init = true;
    }
    public void TriggerDieAnimation()
    {
        const string TRIGGER = "Die";
        anim.SetTrigger(TRIGGER);

    }
    public void TriggerRespawnAnimation()
    {
        const string RESPAWN = "Respawn";
        anim.SetTrigger(RESPAWN);
    }

    public void RendererVisuals(Vector2 velocity, bool isShooting) // this called after simulation
    {
        if (!init) return;

        var isMoving = velocity.x > 0.1f || velocity.x < -0.1f;

        anim.SetBool(isMovingHash, isMoving);
        anim.SetBool(isShootingHash, isShooting);
    }
    public void UpdateScaleTrasform(Vector2 velocity) // this called during the simulation
    {
        if (!init) return;

        if (velocity.x > 0.1f)
        {
            isFacingRight = true;

        }
        else if (velocity.x < -0.1f)
        {
            isFacingRight = false;
        }

        SetObjectLocalScaleBasedOnDir(gameObject, originalPlayerScale);
        SetObjectLocalScaleBasedOnDir(canvasTr.gameObject, originalCanvasScale);
        SetObjectLocalScaleBasedOnDir(pivotGunTr.gameObject, originalGunPivotScale);
    }

    private void SetObjectLocalScaleBasedOnDir(GameObject obj, Vector3 originalScale)
    {
        var yValue = originalScale.y;

        var xValue = isFacingRight ? originalScale.x : -originalScale.x;

        obj.transform.localScale = new Vector3(xValue, yValue, originalScale.z);
    }
}
