using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    private Animator animator;
    private ChairController controller;

    private void OnEnable()
    {
        animator = GetComponent<Animator>();
        controller = transform.root.GetComponentInChildren<ChairController>();
    }

    private void Update()
    {
        if (animator.GetBool("WallKick") && animator.GetBool("ChargingKick"))
        {
            FinishedWallKick();
            ChargeReleased();
        }
    }

    public void FinishedWallKick()
    {
        animator.SetBool("WallKick", false);
        animator.SetBool("Kicked", false);
        controller.canCharge = true;
    }

    public void FinishedFloorKick()
    {
        animator.SetBool("Kicked", false);
        controller.canCharge = true;
    }

    public void FinishedFlip()
    {
        animator.SetBool("shouldFlip", false);
        animator.SetBool("shouldIdle", true);
        controller.updatePlacementTextWish = true;
        animator.SetBool("shouldIdle", false);
    }

    public void ChargeReleased()
    {
        animator.SetBool("ChargingKick", false);
    }

    public void ApplyKick()
    {
        controller.ApplyKickForce();
    }
}
