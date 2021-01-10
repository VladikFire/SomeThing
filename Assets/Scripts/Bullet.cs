using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private PlayerCharacterController Char;
    [SerializeField]
    private float _bulletSpeed = 10f;
    [SerializeField]
    private float _damage;
    private Vector3 Bullettr;
    void Start()
    {
        Char = Char.GetComponent<PlayerCharacterController>();
        Bullettr = Char._ray.normalized;
        DamagedBody();
    }
    void Update()
    {
        transform.Translate(Bullettr * _bulletSpeed * Time.deltaTime);
        DamagedBody();
    }
    public void DamagedBody()
    {
        RaycastHit raycastHit;
        Ray ray = new Ray(transform.position ,Bullettr);
        if(Physics.Raycast(ray,out raycastHit,_bulletSpeed*Time.deltaTime))
        {
            if((transform.position-raycastHit.point).magnitude<_bulletSpeed*Time.deltaTime)
            {
                Destroy(transform.gameObject);
                Debug.Log(transform.gameObject + "  destroed  +" + raycastHit.transform.gameObject);
                if(raycastHit.transform.gameObject.GetComponent<Damageable>())
                raycastHit.transform.gameObject.GetComponent<Damageable>().HealthDamage(_damage);
            }
        }
    }
    private void GravityForce()
    {

    }
}

