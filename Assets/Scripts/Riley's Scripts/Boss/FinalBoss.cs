using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FinalBoss : MonoBehaviour, IDamage
{
    // ADDED FOR BOSS HEALTH BAR:
    // Sends current and maximum boss health to the HUD.
    public static System.Action<float, float> BossHealthChanged;

    // ADDED FOR BOSS HEALTH BAR:
    // Tells the HUD when the boss encounter starts.
    public static System.Action<float, float> BossFightStarted;

    // ADDED FOR BOSS HEALTH BAR:
    // Tells the HUD when the boss has been defeated.
    public static System.Action BossDefeated;


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
    [SerializeField] private Animator animator;


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
    [SerializeField] private float meleeRange = 4f;
    [SerializeField] private GameObject meleeHitbox;
    [SerializeField] private GameObject meleeHitboxHeavy;
    [SerializeField] private int lightAttackChance;

    private float meleeTimer;
    private int attack;


    [Header("Ranged Attack")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform projectileSpawnPoint;
    //[SerializeField] private Transform beamCore;
    [SerializeField] private ParticleSystem energyBeam;
    [SerializeField] private ParticleSystem energyParticles;
    [SerializeField] private ParticleSystem sparks;

    [SerializeField] private float rangedAttackCooldown = 15f;
    [SerializeField] private float rangedAttackChargeTime = 1f;
    [SerializeField] private float rangedAttackDuration = 2f;

    [SerializeField] private float projectileWidth = 2f;
    [SerializeField] private float projectileHeight = 2f;
    [SerializeField] private float hitboxExtendSpeed = 20f;

    bool isShooting = false;
    private float rangedTimer = 5f;
    private bool chargingRangedAttack;

    [Header("Ranged Animation (Rage)")]
    [SerializeField] private string rangeAnimationName = "Rage";
    [SerializeField] private float rangePauseTime = 0f;

    private bool rangeAnimationPaused = false;
    private bool beamFired;

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
            if (gameManager.instance != null &&
                gameManager.instance.player != null)
            {
                return gameManager.instance.player.transform;
            }

            return null;
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHP = maxHP;


        // ADDED FOR BOSS HEALTH BAR:
        // Show the boss HUD and initialize it at full health.
        BossFightStarted?.Invoke(
            currentHP,
            maxHP
        );

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }


        if (meleeHitbox != null)
        {
            meleeHitbox.SetActive(false);
        }
        if (meleeHitboxHeavy != null)
        {
            meleeHitboxHeavy.SetActive(false);
        }


        agent.speed = phase1Speed;
        agent.stoppingDistance = meleeRange;
    }


    // Update is called once per frame
    void Update()
    {
        if (PlayerTransform == null) return;

        if (currentPhase == BossPhase.Dead) return;

        if (animator != null && agent != null && !chargingRangedAttack)
        {
            if (agent.velocity.magnitude > 0.05f)
                animator.SetFloat("Walk", 1f);
            else
                animator.SetFloat("Walk", 0f);
        }

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


        Quaternion targetRotation =
            Quaternion.LookRotation(direction);


        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
    }


    private void CheckTeleport()
    {
        if (PlayerTransform == null) return;

        if (teleportTimer > 0f) return;

        if (isShooting) return;


        float distanceToPlayer =
            Vector3.Distance(
                transform.position,
                PlayerTransform.position
            );


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
                StartCoroutine(
                    Phase2Transition()
                );
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


        float distanceToPlayer =
            Vector3.Distance(
                transform.position,
                PlayerTransform.position
            );


        if (distanceToPlayer < meleeRange - 0.1f)
        {
            MeleeAttack();
        }
        else if (distanceToPlayer >= 10f)
        {
            RangedAttack();
        }
    }


    private IEnumerator Phase2Transition()
    {
        phase2Transitioning = true;
        phase2Triggered = true;


        Debug.Log(
            "Rift Warden is syphoning the power from the rift machines!"
        );


        agent.isStopped = true;


        yield return new WaitForSeconds(
            phase2TransitionDelay
        );


        if (PlayerTransform == null)
        {
            phase2Transitioning = false;
            agent.isStopped = false;

            yield break;
        }


        Debug.Log(
            "Rift Power activated! Boss is teleporting"
        );


        TeleportNearPlayer();


        meleeTimer = 0f;
        rangedTimer = 10f;
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


        float distanceToPlayer =
            Vector3.Distance(
                transform.position,
                PlayerTransform.position
            );


        float teleportChance =
            Random.Range(0, 100);


        if (teleportChance < 10)
        {
            CheckTeleport();
        }


        if (energyFieldTimer <= 0f)
        {
            StartCoroutine(
                EnergyFieldAttack()
            );

            energyFieldTimer =
                energyFieldCooldown;
        }


        if (distanceToPlayer < meleeRange - 0.1f)
        {
            MeleeAttack();
        }
        else if (distanceToPlayer >= 10f)
        {
            RangedAttack();
        }
    }


    private void Phase3Behavior()
    {
        agent.speed = phase3Speed;

        attackCooldown *= 0.25f;
        energyFieldCooldown *= 0.2f;


        Debug.Log(
            "The Rift Warden is enraged and going Beserk!"
        );


        ChasePlayer();


        float distanceToPlayer =
            Vector3.Distance(
                transform.position,
                PlayerTransform.position
            );


        float teleportChance =
            Random.Range(0, 100);


        if (teleportChance < 10)
        {
            CheckTeleport();
        }


        if (energyFieldTimer <= 0f)
        {
            StartCoroutine(
                EnergyFieldAttack()
            );

            energyFieldTimer =
                energyFieldCooldown;
        }


        if (distanceToPlayer < meleeRange - 0.1f)
        {
            MeleeAttack();
        }
        else if (distanceToPlayer >= 10f)
        {
            RangedAttack();
        }
    }


    // MOVEMENT
    private void ChasePlayer()
    {
        if (PlayerTransform == null) return;

        agent.stoppingDistance = meleeRange;

        agent.SetDestination(PlayerTransform.position);
    }


    // MELEE ATTACK
    private void MeleeAttack()
    {
        if (meleeTimer > 0f) return;

        attack = Random.Range(0, 100);
        FacePlayer();
        Vector3 directionToPlayer = (PlayerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle > 3f) return;
        
        Debug.Log("Boss Melee Attack");

        if (agent != null)
            agent.isStopped = true;

        if (attack < lightAttackChance)
            animator.SetTrigger("Hit2");
        else
            animator.SetTrigger("Hit1");
        meleeTimer = attackCooldown;
    }

    public void MeleeHit()
    {
        if (currentPhase == BossPhase.Dead)
            return;

        if (meleeHitbox != null)
        {
            if (attack < lightAttackChance)
                meleeHitbox.SetActive(true);
            else
                meleeHitboxHeavy.SetActive(true);
        }
    }

    public void EndMeleeHit()
    {
        if (meleeHitbox != null)
        {
            if (attack < lightAttackChance)
                meleeHitbox.SetActive(false);
            else
                meleeHitboxHeavy.SetActive(false);
        }

        if (agent != null)
            agent.isStopped = false;
    }

    private void StartRangeAnimation()
    {
        if (animator == null)
            return;

        beamFired = false;
        rangeAnimationPaused = false;
        animator.speed = 1f;
        animator.ResetTrigger("Rage");
        animator.SetTrigger("Rage");
    }

    private void PauseRangeAnimation()
    {
        if (animator == null)
            return;

        animator.speed = 0f;
        rangeAnimationPaused = true;
    }

    private void ResumeRangeAnimation()
    {
        if (animator == null)
            return;

        animator.speed = 1f;
        rangeAnimationPaused = false;
    }

    public void FireBeam()
    {
        if (currentPhase == BossPhase.Dead)
            return;

        if (!chargingRangedAttack)
            return;

        if (beamFired)
            return;

        beamFired = true;

        SpawnBeam();
        PauseRangeAnimation();
    }

    private void SpawnBeam()
    {
        if (PlayerTransform == null)
            return;

        FacePlayer();

        Vector3 direction = (PlayerTransform.position - projectileSpawnPoint.position).normalized;
        GameObject newProjectile = Instantiate(projectile, projectileSpawnPoint.position, Quaternion.LookRotation(direction));
        BoxCollider hitboxCollider = newProjectile.GetComponent<BoxCollider>();
        ProjectileCollision sonicBoom = newProjectile.GetComponent<ProjectileCollision>();
        //Transform core = newProjectile.transform.Find("Beam Core");
        ParticleSystem core = newProjectile.transform.Find("Beam Particle")?.GetComponent<ParticleSystem>();
        ParticleSystem energy = newProjectile.transform.Find("Beam Energy")?.GetComponent<ParticleSystem>();
        ParticleSystem sparkEffect = newProjectile.transform.Find("Beam Sparks")?.GetComponent<ParticleSystem>();

        if (hitboxCollider == null)
        {
            Debug.LogError("Projectile needs a box collider!");
            Destroy(newProjectile);
            return;
        }

        float newLength = 0.1f;
        hitboxCollider.size = new Vector3(projectileWidth, projectileHeight, newLength);
        hitboxCollider.center = new Vector3(0f, 0f, newLength / 2f);

        StartCoroutine(ExtendBeam(newProjectile, hitboxCollider, core, energy, sparkEffect, sonicBoom));
    }

    private IEnumerator ExtendBeam(GameObject beam, BoxCollider hitboxCollider, ParticleSystem core, ParticleSystem energy, ParticleSystem sparks, ProjectileCollision sonicBoom)
    {
        float newLength = 0.1f;
        float attackTime = 0f;

        while (attackTime < rangedAttackDuration)
        {
            if (beam == null)
            {
                chargingRangedAttack = false;
                yield break;
            }

            newLength += hitboxExtendSpeed * Time.deltaTime;
            hitboxCollider.size = new Vector3(projectileWidth, projectileHeight, newLength);
            hitboxCollider.center = new Vector3(0f, 0f, newLength / 2f);
            
            if (core != null)
            {
                ParticleSystem.ShapeModule shape = core.shape;
                shape.scale = new Vector3(projectileWidth, projectileHeight, newLength);
                core.transform.localPosition = new Vector3(0f, 0f, newLength / 2f);
            }

            if (energy != null)
            {
                ParticleSystem.ShapeModule shape = energy.shape;
                shape.scale = new Vector3(projectileWidth, projectileHeight, newLength);
                energy.transform.localPosition = new Vector3(0f, 0f, newLength / 2f);
            }

            if (sparks != null)
            {
                ParticleSystem.ShapeModule shape = sparks.shape;
                shape.scale = new Vector3(projectileWidth, projectileHeight, newLength);
                sparks.transform.localPosition = new Vector3(0f, 0f, newLength / 2f);
            }

            attackTime += Time.deltaTime;
            yield return null;
        }
        Debug.Log("Sonic Boom ended");

        if (sonicBoom != null)
            sonicBoom.StartMoving();

        agent.isStopped = false;
        chargingRangedAttack = false;
        ResumeRangeAnimation();
    }

    // RANGED ATTACK
    private void RangedAttack()
    {
        if (rangedTimer > 0) return;

        if (chargingRangedAttack) return;

        if (isShooting) return;

        if (projectile == null ||
            projectileSpawnPoint == null)
        {
            return;
        }

        StartCoroutine(RangedAttackRoutine());
    }


    // TELEPORT NEAR PLAYER
    private void TeleportNearPlayer()
    {
        if (PlayerTransform == null) return;


        Vector2 randomDirection =
            Random.insideUnitCircle.normalized;


        Vector3 teleportPosition =
            PlayerTransform.position +
            new Vector3(
                randomDirection.x,
                0,
                randomDirection.y
            ) *
            teleportNearPlayerDistance;


        NavMeshHit hit;


        if (NavMesh.SamplePosition(
                teleportPosition,
                out hit,
                5f,
                NavMesh.AllAreas))
        {
            transform.position =
                hit.position;


            FacePlayer();


            agent.Warp(
                hit.position
            );


            teleportTimer =
                teleportCooldown;


            Debug.Log(
                "Rift Boss teleported near player!"
            );
        }
    }


    // SUMMON ENEMIES
    private void SummonEnemies()
    {
        CleanEnemyList();


        if (spawnedEnemies.Count >=
            maxEnemiesAlive)
        {
            return;
        }


        if (waitingToSummon) return;


        int enemiesToSpawn =
            Mathf.Min(
                enemiesPerSummon,
                maxEnemiesAlive -
                spawnedEnemies.Count
            );


        for (int i = 0;
             i < enemiesToSpawn;
             i++)
        {
            SpawnEnemy();
        }
    }


    private void SpawnEnemy()
    {
        Vector2 randomCircle =
            Random.insideUnitCircle.normalized *
            Random.Range(
                minSpawnDistance,
                spawnRadius
            );


        Vector3 spawnPosition =
            new Vector3(
                transform.position.x +
                randomCircle.x,

                transform.position.y +
                5f,

                transform.position.z +
                randomCircle.y
            );


        RaycastHit hit;


        if (Physics.Raycast(
                spawnPosition,
                Vector3.down,
                out hit,
                30f,
                groundLayer))
        {
            Vector3 directionToTarget =
                transform.position -
                hit.point;


            directionToTarget.y = 0;


            Quaternion spawnRotation =
                directionToTarget != Vector3.zero
                    ? Quaternion.LookRotation(
                        directionToTarget
                    )
                    : Quaternion.identity;


            GameObject newEnemy =
                Instantiate(
                    enemyPrefab,
                    hit.point,
                    spawnRotation
                );


            BasicEnemy enemyAI =
                newEnemy.GetComponent<BasicEnemy>();


            if (enemyAI != null)
            {
                enemyAI.SetBossEnemy();
            }


            spawnedEnemies.Add(
                newEnemy
            );
        }
    }


    public void takeDamage(int amount)
    {
        if (currentPhase == BossPhase.Dead)
        {
            return;
        }


        if (phase2Transitioning)
        {
            return;
        }


        currentHP -= amount;


        // ADDED FOR BOSS HEALTH BAR:
        // Update the HUD every time boss health changes.
        BossHealthChanged?.Invoke(
            Mathf.Max(
                currentHP,
                0f
            ),
            maxHP
        );


        Debug.Log(
            "Rift Boss Health:" +
            currentHP +
            "/" +
            maxHP
        );


        CheckPhase();


        if (currentPhase == BossPhase.Phase2)
        {
            Debug.Log(
                "Rift Boss has entered Phase 2!"
            );
        }


        if (currentPhase == BossPhase.Phase3)
        {
            Debug.Log(
                "Rift Boss has entered Phase 3!"
            );
        }


        if (currentHP <= 0)
        {
            Die();
        }
    }


    private void CleanEnemyList()
    {
        for (
            int i =
                spawnedEnemies.Count - 1;

            i >= 0;

            i--)
        {
            if (spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(
                    i
                );
            }
        }
    }


    private void UpdateSummonTimer()
    {
        CleanEnemyList();


        if (spawnedEnemies.Count > 0 &&
            maxEnemies)
        {
            waitingToSummon =
                false;


            summonTimer =
                summonCooldown;


            return;
        }


        if (!waitingToSummon)
        {
            waitingToSummon =
                true;


            summonTimer =
                summonCooldown;


            maxEnemies =
                false;


            Debug.Log(
                "All summoned enemies defeated. " +
                "Boss will summon again in " +
                summonCooldown +
                " seconds."
            );
        }


        summonTimer -=
            Time.deltaTime;


        if (summonTimer <= 0)
        {
            waitingToSummon =
                false;


            Debug.Log(
                "Rift Warden is summoning more enemies!"
            );


            SummonEnemies();
        }
    }


    private void Die()
    {
        currentPhase = BossPhase.Dead;

        chargingRangedAttack = false;
        isShooting = false;
        beamFired = true;

        if (agent != null)
            agent.isStopped = true;

        if (meleeHitbox != null)
            meleeHitbox.SetActive(false);

        if (meleeHitboxHeavy != null)
            meleeHitboxHeavy.SetActive(false);

        if (energyBeam != null)
            energyBeam.Stop();

        if (energyParticles != null)
            energyParticles.Stop();

        if (sparks != null)
            sparks.Stop();

        if (animator != null)
        {
            animator.speed = 1f;

            animator.ResetTrigger("Die");
            animator.SetTrigger("Die");

            StartCoroutine(FreezeDeathAnimation());
        }
        else
        {
            Destroy(gameObject, 3f);
        }

        if (bossObjective != null)
        {
            ObjectiveManager.Instance.CompleteObjective(bossObjective.objectiveID);
        }


        // ADDED FOR BOSS HEALTH BAR:
        // Hide the HUD when the boss dies.
        BossDefeated?.Invoke();

        Debug.Log("Rift Boss Defeated!");

        Destroy(gameObject, 3f);
    }

    private IEnumerator FreezeDeathAnimation()
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Die"));

        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= .2f);

        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            return currentState.IsName("Die") && currentState.normalizedTime >= 0.20f;
        });

        animator.Play("Die", 0, 1f);

        animator.speed = 0f;

        Debug.Log("Boss death animation finished and is frozen.");

        yield return new WaitForSeconds(3f);

        Destroy(gameObject);
    }

    private IEnumerator RangedAttackRoutine()
    {
        isShooting = true;
        chargingRangedAttack = true;

        Debug.Log("Boss is charging ranged attack!");

        agent.isStopped = true;

        if (PlayerTransform == null)
        {
            agent.isStopped = false;
            chargingRangedAttack = false;
            isShooting = false;
            yield break;
        }

        FacePlayer();
        StartRangeAnimation();
        
        //yield return new WaitUntil(() => !chargingRangedAttack);

        while (chargingRangedAttack)
        {
            if (currentPhase == BossPhase.Dead)
            {
                Debug.Log("Ranged attack cancelled because boss died.");

                chargingRangedAttack = false;
                isShooting = false;

                yield break;
            }

            yield return null;
        }

        if (currentPhase == BossPhase.Dead)
            yield break;

        if (agent != null)
            agent.isStopped = false;

        isShooting = false;
        rangedTimer = rangedAttackCooldown;
    }


    private IEnumerator EnergyFieldAttack()
    {
        if (energyFieldSpawnPoints == null ||
            energyFieldSpawnPoints.Length == 0)
        {
            yield break;
        }


        if (energyFieldPrefab == null)
        {
            yield break;
        }


        Debug.Log(
            "Rift Boss is creating energy fields!"
        );


        List<Transform> availablePoints =
            new List<Transform>(
                energyFieldSpawnPoints
            );


        int fieldsToSpawn =
            Mathf.Min(
                energyFieldsPerAttack,
                availablePoints.Count
            );


        for (
            int i = 0;
            i < fieldsToSpawn;
            i++)
        {
            int randomIndex =
                Random.Range(
                    0,
                    availablePoints.Count
                );


            Transform spawnPoint =
                availablePoints[
                    randomIndex
                ];


            availablePoints.RemoveAt(
                randomIndex
            );


            GameObject field =
                Instantiate(
                    energyFieldPrefab,
                    spawnPoint.position,
                    spawnPoint.rotation
                );


            Destroy(
                field,
                energyFieldDuration
            );


            yield return new WaitForSeconds(
                0.25f
            );
        }
    }
}