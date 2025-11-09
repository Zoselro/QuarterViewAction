using UnityEngine;

public class StartZone : MonoBehaviour
{
    [SerializeField] private GameManager manager;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            manager.StageStart();
        }
    }
}
