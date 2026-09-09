namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 재화 시스템과의 계약 인터페이스
    /// GachaManager가 직접 GoldManager를 알지 않도록 함
    /// 나중에 GoldManager → CurrencyManager로 교체 가능하게 설계
    /// </summary>
    public interface ICurrencyProvider
    {
        /// <summary>
        /// 골드 소비 (뽑기 등)
        /// </summary>
        /// <param name="amount">소비할 골드 양</param>
        /// <returns>성공 여부 (골드 부족 시 false)</returns>
        bool SpendGold(int amount);

        /// <summary>
        /// 골드 획득 (경영 등)
        /// </summary>
        /// <param name="amount">획득할 골드 양</param>
        void AddGold(int amount);

        /// <summary>
        /// 아이템(물고기/무기/가구/레시피) 획득
        /// </summary>
        /// <param name="itemId">아이템 고유 ID (예: "weapon_001")</param>
        /// <param name="count">개수</param>
        void AddItem(string itemId, int count);

        /// <summary>
        /// 현재 골드 조회
        /// </summary>
        /// <returns>현재 보유 골드</returns>
        int GetCurrentGold();
    }
}
