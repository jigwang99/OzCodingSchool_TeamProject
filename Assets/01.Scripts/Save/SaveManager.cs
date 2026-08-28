using System.IO;
using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    private string saveFilePath;

    protected override void Awake()
    {
        base.Awake();

        //persistentDataPath를 사용해 플랫폼별 안전한 저장 경로 지정
        saveFilePath = Path.Combine(Application.persistentDataPath, "playData.json");
    }

    //데이터 저장
    public void Save()
    {
        PlayerData data = GameManager.instance.PlayerData;

        //마지막 저장 시간 갱신
        data.lastSaveTime = System.DateTime.Now.ToBinary().ToString();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"[SaveManager] 게임 저장 완료: {saveFilePath}");
    }

    //데이터 불러오기
    //GameManager의 Awake에서 저장된 파일이 있으면 Load함수 불러서 SetPlayerData함수 매개변수로 넣어주기
    //저장된 파일이 없으면 CreateNewPlayerData로 새 데이터 생성하기
    PlayerData Load()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log("[SaveManager] 게임 불러오기 성공");
            return data;
        }
        else
        {
            Debug.Log("[SaveManager] 저장된 파일이 없습니다.");
            return null;
        }
    }
}
