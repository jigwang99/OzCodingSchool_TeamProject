using UnityEngine;

public class SaveTest : MonoBehaviour
{
    [ContextMenu("1. 저장 테스트")]
    public void TestSave()
    {
        SaveManager.instance.Save();

        Debug.Log("===== 저장 테스트 완료 =====");
    }

    [ContextMenu("2. 불러오기 테스트")]
    public void TestLoad()
    {
        PlayerData data = SaveManager.instance.Load();

        if (data == null)
        {
            Debug.Log("===== 저장 데이터가 없습니다. =====");
            return;
        }

        Debug.Log("===== 불러오기 테스트 =====");
        Debug.Log($"마지막 저장 시간 : {data.lastSaveTime}");

        // PlayerData에 실제 존재하는 변수로 추가
        // Debug.Log($"Gold : {data.gold}");
        // Debug.Log($"Fish : {data.fish}");
    }

    [ContextMenu("3. 저장 파일 삭제")]
    public void TestDelete()
    {
        SaveManager.instance.DeleteSaveFile();

        Debug.Log("===== 저장 파일 삭제 완료 =====");
    }
}
