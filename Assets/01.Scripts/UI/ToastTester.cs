using UnityEngine;

public class ToastTester : MonoBehaviour
{
    public void TestShortMessage()
    {
        ToastMessage.instance.Show("저장 완료!");
    }

    public void TestMediumMessage()
    {
        ToastMessage.instance.Show("인벤토리 가방 공간이 얼마 남지 않았습니다.");
    }

    public void TestLongMessage()
    {
        ToastMessage.instance.Show("골드가 부족하여 아이템을 구매할 수 없습니다. 던전에서 몬스터를 처치하고 골드를 모아보세요!");
    }
}
