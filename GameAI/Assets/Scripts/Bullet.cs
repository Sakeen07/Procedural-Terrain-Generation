using UnityEngine;

public class Bullet : MonoBehaviour
{
    public enum BulletType { NPC, Player }
    public BulletType bulletType;
    public int damage = 20;

    void OnTriggerEnter(Collider other)
    {
        switch(bulletType)
        {
            case BulletType.Player:
                if (other.CompareTag("NPC"))
                {
                    Debug.Log("Player bullet hit NPC!");
                    NPC npc = other.GetComponent<NPC>();
                    if (npc != null)
                    {
                        npc.TakeDamage(damage);
                    }
                    Destroy(gameObject);
                }
                else if (other.CompareTag("NewNPC"))
                {
                    Debug.Log("Player bullet hit NewNPC!");
                    NewNPC newNpc = other.GetComponent<NewNPC>();
                    if (newNpc != null)
                    {
                        newNpc.TakeDamage(damage);
                    }
                    Destroy(gameObject);
                }
                break;

            case BulletType.NPC:
                if (other.CompareTag("Player"))
                {
                    Debug.Log("NPC bullet hit Player!");
                    GameUI.Instance.TakeDamage(damage);
                    Destroy(gameObject);
                }
                break;
        }
    }
}