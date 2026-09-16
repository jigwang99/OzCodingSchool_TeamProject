using System.Collections.Generic;
using UnityEngine;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// GachaManager의 결과 이벤트를 받아
    /// GachaStatManager에 전달한다.
    /// </summary>
    public class GachaStatAdapter : MonoBehaviour
    {
        [SerializeField]
        private GachaManager gachaManager;

        [SerializeField]
        private GachaStatManager statManager;

        private void Awake()
        {
            if (gachaManager == null)
                gachaManager = GachaManager.Instance;

            if (statManager == null)
                statManager = GachaStatManager.Instance;
        }

        private void OnEnable()
        {
            if (gachaManager != null)
            {
                gachaManager.OnGachaItemsDrawn
                    += HandleGachaItemsDrawn;
            }
        }

        private void OnDisable()
        {
            if (gachaManager != null)
            {
                gachaManager.OnGachaItemsDrawn
                    -= HandleGachaItemsDrawn;
            }
        }

        private void HandleGachaItemsDrawn(
            List<GachaItem> results)
        {
            if (statManager == null)
            {
                Debug.LogError(
                    "[가챠 스탯 Adapter] " +
                    "GachaStatManager가 없습니다."
                );

                return;
            }

            statManager.ApplyGachaResults(results);
        }
    }
}