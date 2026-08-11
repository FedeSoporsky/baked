using UnityEngine;

public class ClubEnterTriggerBehavior : MonoBehaviour
{
    [SerializeField]
    GameManagerBehavior gameManagerBehavior;

    private void OnTriggerEnter(Collider other)
    {
        gameManagerBehavior.EnterClub();
    }
}
