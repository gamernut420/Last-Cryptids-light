using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyAudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Attack Sounds")]
    [SerializeField] private AudioClip[] meleeAttackSounds;
    [SerializeField] private AudioClip[] projectileAttackSounds;
    [SerializeField] private AudioClip[] energyFieldAttackSounds;
    [SerializeField] private AudioClip[] summonSounds;
    [SerializeField] private AudioClip[] teleportSounds;
    [SerializeField] private AudioClip[] phaseChangeSounds;

    [Header("Other Enemy Sounds")]
    [SerializeField] private AudioClip[] hurtSounds;
    [SerializeField] private AudioClip[] deathSounds;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float attackVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float hurtVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float deathVolume = 1f;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    public void PlayMeleeAttack()
    {
        PlayRandomSound(meleeAttackSounds, attackVolume);
    }

    public void PlayProjectileAttack()
    {
        PlayRandomSound(projectileAttackSounds, attackVolume);
    }

    public void PlayEnergyFieldAttack()
    {
        PlayRandomSound(energyFieldAttackSounds, attackVolume);
    }

    public void PlaySummon()
    {
        PlayRandomSound(summonSounds, attackVolume);
    }

    public void PlayTeleport()
    {
        PlayRandomSound(teleportSounds, attackVolume);
    }

    public void PlayPhaseChange()
    {
        PlayRandomSound(phaseChangeSounds, attackVolume);
    }

    public void PlayHurt()
    {
        PlayRandomSound(hurtSounds, hurtVolume);
    }

    public void PlayDeath()
    {
        PlayRandomSound(deathSounds, deathVolume);
    }

    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }
    }

    private void PlayRandomSound(AudioClip[] clips, float volume)
    {
        if (audioSource == null || clips == null || clips.Length == 0)
        {
            return;
        }

        int startingIndex = Random.Range(0, clips.Length);

        for (int offset = 0; offset < clips.Length; offset++)
        {
            int index = (startingIndex + offset) % clips.Length;

            if (clips[index] != null)
            {
                audioSource.PlayOneShot(clips[index], volume);
                return;
            }
        }
    }
}
