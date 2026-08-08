using UnityEngine;

[System.Serializable]
public struct ProjectileData
{
    [Min(0)]
    public float Damage;

    [Min(1f)]
    public float Speed;

    public float GravityScale;

    [Min(0f)]
    public float LifeTime;
}