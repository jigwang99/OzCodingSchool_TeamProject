using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private PlayercatController playerCat;
    [SerializeField] private EnemyController enemy;

    void Start()
    {
        if (playerCat == null || enemy == null)
        {
            Debug.LogError("[CombatManager] 유닛이 할당되지 않았습니다.");
            return;
        }

        playerCat.SetTarget(enemy);
        enemy.SetTarget(playerCat);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
