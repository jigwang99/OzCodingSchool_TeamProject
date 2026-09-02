using UnityEngine;
using PixelRestaurant.Data;

namespace PixelRestaurant.Managers
{
    /// <summary>
    /// 가챠 결과 로직 처리
    /// 뽑은 아이템을 처리하고 UI에 표시
    /// </summary>
    public class GachaResultHandler : MonoBehaviour
    {
        /// <summary>
        /// 뽑기 결과 처리
        /// </summary>
        public static void HandleGachaResult(GachaItem item, Transform parent, GameObject prefab)
        {
            if (item == null)
            {
                Debug.LogWarning("[가챠 결과] 아이템이 null입니다.");
                return;
            }

            // 프리팹 인스턴스 생성
            GameObject prefabToUse = item.displayPrefab ?? prefab;
            if (prefabToUse == null)
            {
                Debug.LogError("[가챠 결과] 사용할 프리팹이 없습니다.");
                return;
            }

            GameObject resultItem = Instantiate(prefabToUse, parent);
            resultItem.name = $"{item.itemName}(Clone)";

            // 아이템 정보 표시
            GachaResultItemDisplay itemDisplay = resultItem.GetComponent<GachaResultItemDisplay>();
            if (itemDisplay != null)
            {
                itemDisplay.SetItemInfo(item);
            }
            else
            {
                Debug.LogWarning($"[가챠 결과] {resultItem.name}에 GachaResultItemDisplay 컴포넌트가 없습니다.");
            }
        }
    }
}
