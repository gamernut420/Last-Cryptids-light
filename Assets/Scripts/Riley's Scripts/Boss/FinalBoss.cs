using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class FinalBoss : MonoBehaviour, IDamage
{
    public enum BossPhase
    {
        Phase1,
        Phase2,
        Phase3,
        Dead
    }

    public BossPhase currentPhase = BossPhase.Phase1;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] private float turnSpeed = 8f;

    [Header("Health")]
    [SerializeField] private float maxHP = 1000f;
    private float currentHP;

    [Header("Boss Phase")]
    [SerializeField] private float phase2HP = 500f;
    [SerializeField] private float phase3HP = 200f;

    [Header("Movement")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float phase1Speed = 3.5f;
    [SerializeField] private float phase2Speed = 5f;
    [SerializeField] private float phase3Speed = 7f;

    [Header("Melee Attack")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float meleeHitboxDuration = 0.5f;
    [SerializeField] private GameObject meleeHitbox;

    private float meleeTimer;

    [Header("Ranged Attack")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform projectileSpawnPoint;

    [SerializeField] private float rangedAttackCooldown = 15f;
    [SerializeField] private float rangedAttackChargeTime = 1f;
    [SerializeField] private float rangedAttackDuration = 2f;

    [SerializeField] private float projectileWidth = 2f;
    [SerializeField] private float projectileHeight = 2f;
    [SerializeField] private float hitboxExtendSpeed = 20f;

    private float rangedTimer = 5f;
    private bool chargingRangedAttack;

    [Header("Enemy Summoning")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int enemiesPerSummon = 2;

    [Header("Teleporting")]
    [SerializeField] private float teleportDistance = 30f;
    [SerializeField] private float teleportNearPlayerDistance = 8f;
    [SerializeField] private float teleportCooldown = 20;

    private float teleportTimer;

    private Transform PlayerTransform
    {
        get
        {
            if (gameManager.instance != null && gameManager.instance.player != null)
            {
                return gameManager.instance.player.transform;
            }
            return null;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHP = phase2HP;

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (meleeHitbox != null)
        {
            meleeHitbox.SetActive(false);
        }

        agent.speed = phase1Speed;

        
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerTransform == null) return;

        if (currentPhase == BossPhase.Dead) return;

        meleeTimer -= Time.deltaTime;
        rangedTimer -= Time.deltaTime;
        teleportTimer -= Time.deltaTime;

        CheckPhase();

        switch (currentPhase)
        {
            case BossPhase.Phase1:
                Phase1Behavior();
                break;
            case BossPhase.Phase2:
                Phase2Behavior();
                break;
        }
    }

    private void FacePlayer()
    {
        if (PlayerTransform == null) return;

        Vector3 direction = (PlayerTransform.position - projectileSpawnPoint.position).normalized;
        direction.y = 0;

        if (direction.sqrMagnitude <= 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    private void CheckTeleport()
    {
        if (PlayerTransform == null) return;

        if (teleportTimer > 0f) return;

        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        if (distanceToPlayer >= teleportDistance)
        {
            TeleportNearPlayer();
        }
    }

    private void CheckPhase()
    {
        if (currentHP <= 0)
        {
            currentPhase = BossPhase.Dead;
        }
        else if (currentHP <= phase3HP)
        {
            currentPhase = BossPhase.Phase3;
        }
        else if (currentHP <= phase2HP)
        {
            currentPhase = BossPhase.Phase2;
        }
        else
        {
            currentPhase = BossPhase.Phase1;
        }
    }

    // PHASE 1
    private void Phase1Behavior()
    {
        agent.speed = phase1Speed;
        ChasePlayer();

        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        if (distanceToPlayer <= 5f)
        {
            MeleeAttack();
        }
        else
        {
            RangedAttack();
        }
    }

    private void Phase2Behavior()
    {
        agent.speed = phase2Speed;
        ChasePlayer();

        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        float teleportChance = Random.Range(0, 100);

        if (teleportChance < 10)
        {
            CheckTeleport();
        }

        if (distanceToPlayer <= 5f)
        {
            MeleeAttack();
        }
        else
        {
            RangedAttack();
        }
    }

    // MOVEMENT
    private void ChasePlayer()
    {
        if (PlayerTransform == null) return;

        agent.SetDestination(PlayerTransform.position);
    }

    // MELEE ATTACK
    private void MeleeAttack()
    {
        if (meleeTimer > 0) return;

        Debug.Log("Boss Melee Attack!");

        StartCoroutine(MeleeAttackRoutine());

        meleeTimer = attackCooldown;
    }

    // RANGED ATTACK
    private void RangedAttack()
    {
        if (rangedTimer > 0) return;

        if (chargingRangedAttack) return;

        if (projectile == null || projectileSpawnPoint == null) return;

        StartCoroutine(RangedAttackRoutine());
    }

    // TELEPORT NEAR PLAYER
    private void TeleportNearPlayer()
    {
        if (PlayerTransform == null) return;

        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        Vector3 teleportPosition = PlayerTransform.position + new Vector3(randomDirection.x, 0, randomDirection.y) * teleportNearPlayerDistance;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(teleportPosition, out hit, 5f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            FacePlayer();
            agent.Warp(hit.position);

            teleportTimer = teleportCooldown;

            Debug.Log("Rift Boss teleported near player!");
        }
    }

    public void takeDamage(int amount)
    {
        if (currentPhase == BossPhase.Dead) return;

        currentHP -= amount;
        Debug.Log("Rift Boss Health:" + currentHP + "/" + maxHP);

        CheckPhase();
        
        if (currentPhase == BossPhase.Phase2)
        {
            Debug.Log("Rift Boss has entered Phase 2!");
        }

        if (currentPhase == BossPhase.Phase3)
        {
            Debug.Log("Rift Boss has entered Phase 3!");
        }
        
        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        currentPhase = BossPhase.Dead;

        agent.isStopped = true;

        Debug.Log("Rift Boss Defeated!");

        Destroy(gameObject, 3f);
    }

    private IEnumerator MeleeAttackRoutine()
    {
        yield return new WaitForSeconds(0.3f);
        meleeHitbox.SetActive(true);
        yield return new WaitForSeconds(meleeHitboxDuration);
        meleeHitbox.SetActive(false);
    }

    private IEnumerator RangedAttackRoutine()
    {
        chargingRangedAttack = true;

        Debug.Log("Boss is charging ranged attack!");

        agent.isStopped = true;
        float chargeTime = 0f;

        if (PlayerTransform == null)
        {
            agent.isStopped = false;
            chargingRangedAttack = false;
            yield break;
        }

        while (chargeTime < rangedAttackChargeTime)
        {
            FacePlayer();
            chargeTime += Time.deltaTime;
            yield return null;
        }

        FacePlayer();

        yield return null;

        Vector3 direction = (PlayerTransform.position - projectileSpawnPoint.position).normalized;
        GameObject newProjectile = Instantiate(projectile, projectileSpawnPoint.position, Quaternion.LookRotation(direction));
        BoxCollider hitboxCollider = newProjectile.GetComponent<BoxCollider>();
        Transform projectileVisual = newProjectile.transform.Find("RiftBeam Visual");
        ProjectileCollision sonicBoom = newProjectile.GetComponent<ProjectileCollision>();

        if (hitboxCollider == null)
        {
            Debug.LogError("Projectile needs a Box Collider!");
            Destroy(newProjectile);

            agent.isStopped = false;
            chargingRangedAttack = false;
            yield break;
        }

        float newLength = 0.1f;

        hitboxCollider.size = new Vector3(projectileWidth, projectileHeight, newLength);
        hitboxCollider.center = new Vector3(projectileWidth, projectileHeight, newLength / 2f);
        projectileVisual.localScale = new Vector3(projectileWidth, projectileHeight, newLength);
        projectileVisual.localPosition = new Vector3(projectileWidth, projectileHeight, newLength / 2f);
        float attackTime = 0f;

        while (attackTime < rangedAttackDuration)
        {
            //Extends hitbox with the projectile's forward direction
            newLength += hitboxExtendSpeed * Time.deltaTime;

            hitboxCollider.size = new Vector3(projectileWidth, projectileHeight, newLength);
            hitboxCollider.center = new Vector3(0f, 0f, newLength / 2f);

            //Extends visual cube
            if (projectileVisual != null)
            {
                projectileVisual.localScale = new Vector3(projectileWidth, projectileHeight, newLength);
                projectileVisual.localPosition = new Vector3(0f, 0f, newLength / 2f);
            }

            attackTime += Time.deltaTime;

            yield return null;
        }

        Debug.Log("Sonic Boom ended");

        if (sonicBoom != null)
        {
            sonicBoom.StartMoving();
        }
        
        rangedTimer = rangedAttackCooldown;
        agent.isStopped = false;
        chargingRangedAttack = false;
    }
}
