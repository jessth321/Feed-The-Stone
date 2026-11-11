using UnityEngine;

public class PlayerAnimEventController : MonoBehaviour
{
    PlayerController pc;
    public AnimationEvent animEvent = new AnimationEvent();

    public TrailRenderer swordTrailRenderer;

    public void Awake()
    {
        pc = FindFirstObjectByType<PlayerController>();

        swordTrailRenderer = pc.swordTrail;

    }

    public void SetTrailActive()
    {
        swordTrailRenderer.emitting = !swordTrailRenderer.emitting;
    }

    public void CheckAttackExit()
    {
        pc.CheckExitLightAttack();
    }

    public void SpawnHeavyBlast()
    {
        pc.SpawnHeavyBlast();

    }
}
