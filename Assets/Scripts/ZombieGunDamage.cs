using UnityEngine;

public class ZombieGunDamage : MonoBehaviour
{
    public GameObject zombieDamageObj;
    public void SendGunDamage(Vector3 hitPoint, int damage)
    {
        zombieDamageObj.GetComponent<ZombieDamage>().gunDamage(hitPoint, damage);
    }
}
