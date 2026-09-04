using UnityEngine;

[System.Serializable]
public struct ProjectileData
{
    [Min(1)]
    public int Gauge;

    [Range(0f, 1f)]
    public float SpreadReduction;

    public float GravityScale;

    [Min(0f)]
    public float LifeTime;

    [Header("----- VFX -----")]
    public GameObject TracerPrefab;
}