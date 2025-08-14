using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorOverrider : MonoBehaviour
{

    protected Animator animator;
    protected AnimatorOverrideController animatorOverrideController;

    public AnimationClip headNAKEDAnimationClip;
    public AnimationClip chestNAKEDAnimationClip;
    public AnimationClip armsNAKEDAnimationClip;
    public AnimationClip legsNAKEDAnimationClip;


    // Start is called before the first frame update
    void Start()
    {
        animator = GameObject.Find("AnimatorController").GetComponent<Animator>();

        //create a new animator override controller

        animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);

        animator.runtimeAnimatorController = animatorOverrideController;

    }


    public void UnEquipAnimation(ItemType itemType)
    {
        if(itemType == ItemType.Head)
        {
            animatorOverrideController["Head--Idle--Naked"] = headNAKEDAnimationClip;
        }

        if (itemType == ItemType.Chest)
        {
            animatorOverrideController["Chest--Idle--Naked"] = chestNAKEDAnimationClip;
        }


        if (itemType == ItemType.Arms)
        {
            animatorOverrideController["Arms--Idle--Naked"] = armsNAKEDAnimationClip;
        }

        if (itemType == ItemType.Legs)
        {
            animatorOverrideController["Legs--Idle--Naked"] = legsNAKEDAnimationClip;
        }

    }


    public void EquipAnimation(ItemType itemType, AnimationClip Idle/*, AnimationClip Up, AnimationClip Down, AnimationClip right, AnimationClip Right*/)
    {
        if (itemType == ItemType.Head)
        {
            animatorOverrideController["Head--Idle--Naked"] = Idle;
        }

        if (itemType == ItemType.Chest)
        {
            animatorOverrideController["Chest--Idle--Naked"] = Idle;
        }


        if (itemType == ItemType.Arms)
        {
            animatorOverrideController["Arms--Idle--Naked"] = Idle;
        }

        if (itemType == ItemType.Legs)
        {
            animatorOverrideController["Legs--Idle--Naked"] = Idle;
        }

    }



}
