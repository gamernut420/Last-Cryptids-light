using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    
    [SerializeField] bool DebugProjectiles = false;

    private struct Projectile
    {
        public ProjectileData projData;

        public float damage;

        public float speed;

        public GameObject tracer;

        public Vector3 velocity;

        public Vector3 startPos;
    }

    private List<Projectile> projectiles = new List<Projectile>();

    public void ShootProjectile(Vector3 _location, Quaternion _rotation, ProjectileData _projData, float _damage, float _speed)
    {
        Projectile tempProjectile = new Projectile();

        tempProjectile.projData = _projData;

        tempProjectile.damage = _damage;

        tempProjectile.speed = _speed;

        tempProjectile.tracer = Instantiate(_projData.TracerPrefab, _location, _rotation);

        Vector3 foward = _rotation * Vector3.forward;

        tempProjectile.velocity = foward * _speed;

        tempProjectile.startPos = _location;

        projectiles.Add(tempProjectile);
    }

    private void UpdateProjectiles()
    {
        for (int i = projectiles.Count - 1; i >= 0; i--)
        {
            Projectile proj = projectiles[i];

            Vector3 endPos = proj.startPos + (proj.velocity * Time.deltaTime);

            proj.tracer.transform.position = proj.startPos;

            RaycastHit hit;

            if (DebugProjectiles)
            {
                Debug.DrawLine(proj.startPos, endPos, Color.red, 0.5f);
            }
            
            //projectile collided
            if (Physics.Linecast(proj.startPos, endPos, out hit)) 
            {
                proj.tracer.transform.position = endPos;

                if (DebugProjectiles)
                {
                    Debug.Log(hit.collider.gameObject.name);
                }

                IDamage idmg = hit.collider.GetComponent<IDamage>();

                if(idmg != null)
                {
                    idmg.takeDamage((int)proj.damage);
                }

                RemoveProjectile(i);
            }
            //projectiles life ran out
            else if(proj.projData.LifeTime <= 0)
            {
                RemoveProjectile(i);

                if (DebugProjectiles)
                {
                    Debug.Log("Lifetime ran");
                }
            }
            //projectile should continue
            else
            {
                proj.startPos = endPos;

                proj.velocity -= new Vector3(0f, 9.8f * proj.projData.GravityScale, 0f) * Time.deltaTime;

                proj.projData.LifeTime -= Time.deltaTime;

                projectiles[i] = proj;
            }
        }
    }

    private void Update()
    {
        UpdateProjectiles();
    }

    private void RemoveProjectile(int index)
    {
        //Destroy(projectiles[index].tracer);

        projectiles.RemoveAt(index);
    }
}
