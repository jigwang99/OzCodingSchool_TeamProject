// StageManager가 진행도를 변경한 뒤 전달하는 이번 전투 결과의 스냅샷.
public readonly struct StageResult
{
    public readonly bool IsClear;
    public readonly int CompletedStage;
    public readonly int NextStage;
    public readonly string CompletedStageName;
    public readonly string NextStageName;
    public readonly bool RetryWasEnabled;
    public readonly float Delay;

    public StageResult(bool isClear, int completedStage, int nextStage,
        string completedStageName, string nextStageName, bool retryWasEnabled, float delay)
    {
        IsClear = isClear;
        CompletedStage = completedStage;
        NextStage = nextStage;
        CompletedStageName = completedStageName;
        NextStageName = nextStageName;
        RetryWasEnabled = retryWasEnabled;
        Delay = delay;
    }

    public string Title => IsClear ? "스테이지 클리어!" : "전투 패배";

    public string Message
    {
        get
        {
            if (IsClear)
            {
                if (NextStage > CompletedStage)
                    return $"다음 스테이지 {NextStageName}에 도전합니다.";

                return RetryWasEnabled
                    ? $"{NextStageName} 반복 사냥을 계속합니다."
                    : $"마지막 스테이지입니다.\n{NextStageName} 반복 사냥을 계속합니다.";
            }

            if (NextStage < CompletedStage)
                return $"이전 스테이지 {NextStageName}에서 재도전합니다.\n재도전 모드로 전환되었습니다.";

            return RetryWasEnabled
                ? $"현재 스테이지 {NextStageName}에서 재도전합니다."
                : $"첫 스테이지 {NextStageName}에서 재도전합니다.\n재도전 모드로 전환되었습니다.";
        }
    }
}
