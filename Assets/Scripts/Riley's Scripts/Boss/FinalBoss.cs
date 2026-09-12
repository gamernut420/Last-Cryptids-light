using System.Collections;
using System.Collections.Generic;
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

    [Header("Objective")]
    [SerializeField] private ObjectiveData bossObjective;

    [Header("Health")]
    [SerializeField] private float maxHP = 1000f;
    private float currentHP;

    [Header("Boss Phase")]
    [SerializeField] private float phase2HP = 500f;
    [SerializeField] private float phase3HP = 200f;
    [SerializeField] private float phase2TransitionDelay = 5f;

    private bool phase2Transitioning;
    private bool phase2Triggered;

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

    bool isShooting = false;
    private float rangedTimer = 5f;
    private bool chargingRangedAttack;

    [Header("Enemy Summoning")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnRadius = 30f;
    [SerializeField] private float minSpawnDistance = 5f;

    [SerializeField] private int maxEnemiesAlive = 6;
    [SerializeField] private int enemiesPerSummon = 2;

    [SerializeField] private float summonCooldown = 10f;
    [SerializeField] private LayerMask groundLayer;

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool waitingToSummon;
    private bool maxEnemies;
    private float summonTimer;

    [Header("Teleporting")]
    [SerializeField] private float teleportDistance = 30f;
    [SerializeField] private float teleportNearPlayerDistance = 8f;
    [SerializeField] private float teleportCooldown = 20f;
    
    private float teleportTimer;

    [Header("Energy Fields")]
    [SerializeField] private GameObject energyFieldPrefab;
    [SerializeField] private Transform[] energyFieldSpawnPoints;
    [SerializeField] int energyFieldsPerAttack = 2;
    [SerializeField] float energyFieldCooldown = 10f;
    [SerializeField] float energyFieldDuration = 6f;

    private float energyFieldTimer;

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
        //added by sean
        ApplyDifficultyScaling();
        //end added by sean
        currentHP = maxHP;

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

    //added by sean
    public void ApplyDifficultyScaling()
    {
        DifficultyManager difficultyManager = DifficultyManager.GetInstance();
        if (difficultyManager == null)
        {
            return;
        }
        maxHP = difficultyManager.GetScaledEnemyHealth(maxHP);
        phase2HP = difficultyManager.GetScaledEnemyHealth(phase2HP);
        phase3HP = difficultyManager.GetScaledEnemyHealth(phase3HP);
        phase1Speed = difficultyManager.GetScaledEnemySpeed(phase1Speed);
        phase2Speed = difficultyManager.GetScaledEnemySpeed(phase2Speed);
        phase3Speed = difficultyManager.GetScaledEnemySpeed(phase3Speed);
        maxEnemiesAlive = difficultyManager.GetScaledEnemyCount(maxEnemiesAlive);
        enemiesPerSummon = difficultyManager.GetScaledEnemyCount(enemiesPerSummon);
        energyFieldsPerAttack = difficultyManager.GetScaledEnemyCount(energyFieldsPerAttack);
    }
    //end added by sean

    // Update is called once per frame
    void Update()
    {
        if (PlayerTransform == null) return;

        if (currentPhase == BossPhase.Dead) return;

        meleeTimer -= Time.deltaTime;
        rangedTimer -= Time.deltaTime;
        teleportTimer -= Time.deltaTime;
        energyFieldTimer -= Time.deltaTime;

        CheckPhase();
        UpdateSummonTimer();
        if (spawnedEnemies.Count >= maxEnemiesAlive)
        {
            maxEnemies = true;
        }

        switch (currentPhase)
        {
            case BossPhase.Phase1:
                Phase1Behavior();
                break;
            case BossPhase.Phase2:
                Phase2Behavior();
                break;
            case BossPhase.Phase3:
                Phase3Behavior();
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

        if (isShooting) return;

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
            if (!phase2Triggered)
            {
                StartCoroutine(Phase2Transition());
            }
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

    private IEnumerator Phase2Transition()
    {
        phase2Transitioning = true;
        phase2Triggered = true;

        Debug.Log("Rift Warden is syphoning the power from the rift machines!");

        agent.isStopped = true;

        yield return new WaitForSeconds(phase2TransitionDelay);

        if (PlayerTransform == null)
        {
            phase2Transitioning = false;
            agent.isStopped = false;
            yield break;
        }

        Debug.Log("Rift Power activated! Boss is teleporting");

        TeleportNearPlayer();

        meleeTimer = 0f;
        rangedTimer = 0f;
        energyFieldTimer = 0f;
        teleportTimer = teleportCooldown;

        agent.isStopped = false;
        phase2Transitioning = false;
    }

    private void Phase2Behavior()
    {
        if (phase2Transitioning) return;

        agent.speed = phase2Speed;
        rangedAttackCooldown = 10f;

        ChasePlayer();

        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        float teleportChance = Random.Range(0, 100);

        if (teleportChance < 10)
        {
            CheckTeleport();
        }

        if (energyFieldTimer <= 0f)
        {
            StartCoroutine(EnergyFieldAttack());
            energyFieldTimer = energyFieldCooldown;
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

    private void Phase3Behavior()
    {
        agent.speed = phase3Speed;
        attackCooldown *= 0.25f;
        energyFieldCooldown *= 0.2f;

        Debug.Log("The Rift Warden is enraged and going Beserk!");

        ChasePlayer();

        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);

        float teleportChance = Random.Range(0, 100);

        if (teleportChance < 10)
        {
            CheckTeleport();
        }

        if (energyFieldTimer <= 0f)
        {
            StartCoroutine(EnergyFieldAttack());
            energyFieldTimer = energyFieldCooldown;
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

    //SUMMON ENEMIES
    private void SummonEnemies()
    {
        CleanEnemyList();

        if (spawnedEnemies.Count >= maxEnemiesAlive) return;


        if (waitingToSummon) return;

        int enemiesToSpawn = Mathf.Min(enemiesPerSummon, maxEnemiesAlive - spawnedEnemies.Count);
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minSpawnDistance, spawnRadius);

        Vector3 spawnPosition = new Vector3(transform.position.x + randomCircle.x, transform.position.y + 5f, transform.position.z + randomCircle.y);

        RaycastHit hit;
        if (Physics.Raycast(spawnPosition, Vector3.down, out hit, 30f, groundLayer))
        {
            Vector3 directionToTarget = transform.position - hit.point;
            directionToTarget.y = 0;
            Quaternion spawnRotation = directionToTarget != Vector3.zero
                ? Quaternion.LookRotation(directionToTarget)
                : Quaternion.identity;

            GameObject newEnemy = Instantiate(enemyPrefab, hit.point, spawnRotation);

            BasicEnemy enemyAI = newEnemy.GetComponent<BasicEnemy>();
            if (enemyAI != null)
            {
                enemyAI.SetBossEnemy();
            }

            spawnedEnemies.Add(newEnemy);
        }
    }

    public void takeDamage(int amount)
    {
        if (currentPhase == BossPhase.Dead) return;

        if (phase2Transitioning) return;

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

    private void CleanEnemyList()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(i);
            }
        }
    }

    private void UpdateSummonTimer()
    {
        CleanEnemyList();

        if (spawnedEnemies.Count > 0 && maxEnemies)
        {
            waitingToSummon = false;
            summonTimer = summonCooldown;
            return;
        }

        if (!waitingToSummon)
        {
            waitingToSummon = true;
            summonTimer = summonCooldown;
            maxEnemies = false;

            Debug.Log("All summoned enemies defeated. Boss will summon again in " + summonCooldown + " seconds.");
        }

        summonTimer -= Time.deltaTime;

        if (summonTimer <= 0)
        {
            waitingToSummon = false;
            Debug.Log("Rift Warden is summoning more enemies!");
            SummonEnemies();
        }
    }

    private void Die()
    {
        currentPhase = BossPhase.Dead;
        agent.isStopped = true;
        
        if (bossObjective != null)
        {
            ObjectiveManager.Instance.CompleteObjective(bossObjective.objectiveID);
        }

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
        isShooting = true;
        chargingRangedAttack = true;

        Debug.Log("Boss is charging ranged attack!");

        agent.isStopped = true;
        float chargeTime = 0f;

        if (PlayerTransform == null)
        {
            agent.isStopped = false;
            chargingRangedAttack = false;
            isShooting = false;
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
        BossProjectileCollision sonicBoom = newProjectile.GetComponent<BossProjectileCollision>();

        if (hitboxCollider == null)
        {
            Debug.LogError("Projectile needs a Box Collider!");
            Destroy(newProjectile);

            agent.isStopped = false;
            chargingRangedAttack = false;
            isShooting = false;
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
        isShooting = false;
    }

    private IEnumerator EnergyFieldAttack()
    {
        if (energyFieldSpawnPoints == null || energyFieldSpawnPoints.Length == 0)
            yield break;

        if (energyFieldPrefab == null)
            yield break;
        
        Debug.Log("Rift Boss is creating energy fields!");

        List<Transform> availablePoints = new List<Transform>(energyFieldSpawnPoints);

        int fieldsToSpawn = Mathf.Min(energyFieldsPerAttack, availablePoints.Count);

        for (int i = 0; i < fieldsToSpawn; i++)
        {
            int randomIndex = Random.Range(0, availablePoints.Count);
            Transform spawnPoint = availablePoints[randomIndex];

            availablePoints.RemoveAt(randomIndex);

            GameObject field = Instantiate(energyFieldPrefab, spawnPoint.position, spawnPoint.rotation);

            Destroy(field, energyFieldDuration);

            yield return new WaitForSeconds(0.25f);
        }
    }

    
}
