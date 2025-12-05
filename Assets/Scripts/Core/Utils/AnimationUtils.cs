using UnityEngine;

public static class AnimationUtils
{
    public static float TimeAnimationClip(Animator animator, string nameClip)
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
            if (clip.name == nameClip)
                return clip.length;
        return 0;
    }
}
