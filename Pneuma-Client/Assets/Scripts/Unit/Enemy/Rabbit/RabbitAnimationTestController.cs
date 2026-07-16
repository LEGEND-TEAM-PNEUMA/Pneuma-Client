using UnityEngine;

public class RabbitAnimationTestController : MonoBehaviour
{
    [SerializeField]
    private RabbitAnimationController rabbitAnimation;

    public void PlayIdle()
    {
        rabbitAnimation.ResetAnimationState();
    }

    public void PlayAttack()
    {
        rabbitAnimation.PlayAttack();
    }

    public void PlayHit()
    {
        rabbitAnimation.PlayHit();
    }

    public void PlayDefeat()
    {
        rabbitAnimation.PlayDefeat();
    }
}