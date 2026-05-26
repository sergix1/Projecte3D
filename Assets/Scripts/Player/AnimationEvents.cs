using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    public ClickToMove player;

    public void FireAttack()
    {
        if (player != null)
            player.FirePendingAttack();
    }

    public void FinishAttack()
    {
        if (player != null)
            player.EndAttack();
    }
}
