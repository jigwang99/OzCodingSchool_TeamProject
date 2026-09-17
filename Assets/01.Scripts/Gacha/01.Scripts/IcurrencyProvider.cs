namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// Gacha 시스템이 재화 시스템과 연결되기 위한 인터페이스
    /// 골드는 공용 BigNumber를 사용한다.
    /// </summary>
    public interface ICurrencyProvider
    {
        /// <summary>
        /// 골드 소비
        /// </summary>
        /// <param name="amount">소비할 골드</param>
        /// <returns>소비 성공 여부</returns>
        bool SpendGold(BigNumber amount);

        /// <summary>
        /// 골드 획득
        /// </summary>
        /// <param name="amount">획득할 골드</param>
        void AddGold(BigNumber amount);

        /// <summary>
        /// 현재 골드 조회
        /// </summary>
        BigNumber GetCurrentGold();
    }
}