using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils  // helper class for duplication
{
    public static IEnumerator PlayAnimAndSetStateWhenFinished(GameObject parent, Animator animator, string clipName, bool activeStateAtTheEnd = true)
    {
        animator.Play(clipName);

        var animationLengt = animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSecondsRealtime(animationLengt);

        parent.SetActive(activeStateAtTheEnd);
    }
}
