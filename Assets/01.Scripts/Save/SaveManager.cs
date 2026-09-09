using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    private string saveFilePath;
    private float saveInterval = 120f;

    protected override void Awake()
    {
        base.Awake();

        //persistentDataPath를 사용해 플랫폼별 안전한 저장 경로 지정
        saveFilePath = Path.Combine(Application.persistentDataPath, "playData.json");
    }

    void Start()
    {
        StartCoroutine(AutoSaveCoroutine());
    }

    //데이터 저장
    public void Save()
    {
        try
        {
            if (GameManager.instance == null)
            {
                Debug.LogError("[SaveManager] GameManager가 존재하지 않아 저장할 수 없습니다.");
                return;
            }

            PlayerData data = GameManager.instance.PlayerData;

            if (data == null)
            {
                Debug.LogError("[SaveManager] PlayerData가 존재하지 않아 저장할 수 없습니다.");
                return;
            }

            // 마지막 저장 시간 갱신
            data.lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //data.lastSaveTime = System.DateTime.Now.ToBinary().ToString();

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFilePath, json);

            Debug.Log($"[SaveManager] 게임 저장 완료: {saveFilePath}");
        }
        catch (IOException e)
        {
            Debug.LogError($"[SaveManager] 파일 저장 중 오류 발생: {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] 저장 중 예상하지 못한 오류 발생: {e}");
        }
    }

    //데이터 불러오기
    public PlayerData Load()
    {
        try
        {
            if (!File.Exists(saveFilePath))
            {
                Debug.Log("[SaveManager] 저장된 파일이 없습니다.");
                return null;
            }

            string json = File.ReadAllText(saveFilePath);

            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("[SaveManager] 저장 파일이 비어있습니다.");
                return null;
            }

            PlayerData data = JsonUtility.FromJson<PlayerData>(json);

            if (data == null)
            {
                Debug.LogError("[SaveManager] 저장 데이터를 불러왔지만 PlayerData가 null입니다.");
                return null;
            }

            Debug.Log("[SaveManager] 게임 불러오기 성공");
            return data;
        }
        catch (IOException e)
        {
            Debug.LogError($"[SaveManager] 파일 불러오기 중 오류 발생: {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] 불러오기 중 예상하지 못한 오류 발생: {e}");
            return null;
        }
    }

    // 저장 파일 삭제 - 테스트용
    public void DeleteSaveFile()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
                Debug.Log("[SaveManager] 저장 파일 삭제 완료");
            }
            else
            {
                Debug.Log("[SaveManager] 삭제할 저장 파일이 없습니다.");
            }
        }
        catch (IOException e)
        {
            Debug.LogError($"[SaveManager] 저장 파일 삭제 중 오류 발생: {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] 삭제 중 예상하지 못한 오류 발생: {e}");
        }
    }

    // 자동 저장
    private IEnumerator AutoSaveCoroutine()
    {
        while (true)
        {
            Save();

            Debug.Log("[AutoSave] 자동 저장 완료");

            yield return new WaitForSeconds(saveInterval);
        }
    }
}