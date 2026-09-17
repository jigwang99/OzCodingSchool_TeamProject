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
                return;

            PlayerData data = GameManager.instance.PlayerData;

            if (data == null)
                return;

            // 마지막 저장 시간 갱신
            data.lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //data.lastSaveTime = System.DateTime.Now.ToBinary().ToString();

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFilePath, json);
        }
        catch (IOException)
        {
            return;
        }
        catch (Exception)
        {
            return;
        }
    }

    //데이터 불러오기
    public PlayerData Load()
    {
        try
        {
            if (!File.Exists(saveFilePath))
                return null;

            string json = File.ReadAllText(saveFilePath);

            if (string.IsNullOrEmpty(json))
                return null;

            PlayerData data = JsonUtility.FromJson<PlayerData>(json);

            if (data == null)
                return null;

            return data;
        }
        catch (IOException e)
        {
            return null;
        }
        catch (Exception e)
        {
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
            }
            else
            {
                return;
            }
        }
        catch (IOException e)
        {
            return;
        }
        catch (Exception e)
        {
            return;
        }
    }

    // 자동 저장
    private IEnumerator AutoSaveCoroutine()
    {
        while (true)
        {
            Save();

            yield return new WaitForSeconds(saveInterval);
        }
    }
}
