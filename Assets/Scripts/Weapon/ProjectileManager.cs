using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    
    [SerializeField] bool DebugProjectiles = false;

    private struct Projectile
    {
        public ProjectileData projData;

        public Vector3 velocity;

        public Vector3 startPos;
    }

    private List<Projectile> projectiles = new List<Projectile>();

    public void ShootProjectile(Vector3 _location, Quaternion _rotation, ProjectileData _projData)
    {
        Projectile tempProjectile = new Projectile();

        tempProjectile.projData = _projData;

        Vector3 foward = _rotation * Vector3.forward;

        tempProjectile.velocity = foward * _projData.Speed;

        tempProjectile.startPos = _location;

        projectiles.Add(tempProjectile);
    }

    private void UpdateProjectiles()
    {
        for (int i = projectiles.Count - 1; i >= 0; i--)
        {
            Projectile proj = projectiles[i];

            Vector3 endPos = proj.startPos + (proj.velocity * Time.deltaTime);

            RaycastHit hit;

            if (DebugProjectiles)
            {
                Debug.DrawLine(proj.startPos, endPos, Color.red, 0.5f);
            }
            
            //projectile collided
            if (Physics.Linecast(proj.startPos, endPos, out hit)) 
            {
                if (DebugProjectiles)
                {
                    Debug.Log(hit.collider.gameObject.name);
                }

                IDamage idmg = hit.collider.GetComponent<IDamage>();

                if(idmg != null)
                {
                    idmg.takeDamage((int)proj.projData.Damage);
                }

                projectiles.RemoveAt(i);
                Debug.Log("Removed Projectile");
            }
            //projectiles life ran out
            else if(proj.projData.LifeTime <= 0)
            {
                projectiles.RemoveAt(i);
                Debug.Log("Removed Projectile from lifetime");
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
}
