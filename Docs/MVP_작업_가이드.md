# MVP 작업 가이드

목표는 아래 흐름이 실제 플레이에서 한 번 완성되는 것

`전투 -> 커먼 물고기 획득 -> 식당 자동 판매 -> 골드 획득 -> 업그레이드 -> 다음 전투`

첫 MVP에서는 물고기/적/요리/무기를 각각 1종만 사용한다. 뽑기 로직은 구현하되 결과 확인은 UI 대신 `Debug.Log`로 한다. 가구, 도감, 오프라인 보상, 등급 확장은 이후에 추가한다.


공용 매니저는 `Singleton<T>`를 상속하고 `클래스명.instance`로 접근


## 담당별 첫 작업

### 경영

1. `RestaurantManager`를 만들고 일정 주기마다 주문 1건을 처리
2. 커먼 물고기 1개를 소비하고 기본 요리 1종을 판매
3. 판매 성공 시 골드를 지급.
4. 식당 레벨업 버튼 1개를 만들고 비용과 효과를 임시 수치로 적용.

완료 기준: 물고기가 있을 때 자동 판매가 반복되고, 골드가 증가

### 전투

1. 고양이 1마리, 기본 무기 1개, 적 1종으로 전투 씬을 구성.
2. 이동, 적 탐지, 자동 공격, HP, 사망을 구현.
3. 적 처치 시 커먼 물고기를 지급.
4. 적을 모두 처치하면 다음 스테이지로, 패배하면 같은 스테이지 재도전으로 처리.

완료 기준: 자동 전투가 끝나고 적 처치 보상이 식당에서 확인된다.

### 뽑기

1. `GachaManager`를 만들고 골드를 소비하는 단차 뽑기를 구현
2. 무기 후보 2종 이상 중 하나를 확률적으로 선택하도록 구현한
3. 뽑기 성공 시 획득한 무기 이름을 `Debug.Log`로 출력
4. 골드가 부족하면 뽑기하지 않고 부족 메시지를 `Debug.Log`로 출력

뽑기 UI, 연출, 10연차, 등급별 확률 표시는 이후에 추가

완료 기준: 골드를 지급한 뒤 뽑기를 호출하면 무기 이름 또는 골드 부족 메시지가 Console에 출력

### 재화 · 업그레이드 · 저장

1. `CurrencyManager`를 만들고 골드와 커먼 물고기의 획득/소비 구현
2. `SaveManager`를 만들고 `PlayerData`를 JSON으로 저장/불러오기
3. 무기 또는 식당 중 하나의 업그레이드 비용 증가식을 구현
4. 앱 종료·재실행 후 재화와 레벨이 복원되는지 확인

완료 기준: 전투와 식당이 재화를 매니저를 통해 변경하며, 저장 후 다시 실행해도 값이 유지

### QA · UI

1. 전투/경영 화면 전환 버튼과 골드·커먼 물고기 표시 UI 구현
2. 재화 변경 이벤트를 받아 UI 텍스트를 갱신

완료 기준: 플레이 중 골드와 물고기 변화가 UI에 즉시 반영되고, 전투와 경영 화면을 오갈 수 있다

## 공통 인터페이스 규칙

```csharp
// CurrencyManager
CurrencyManager.instance.AddGold(int amount);
bool success = CurrencyManager.instance.SpendGold(int amount);
CurrencyManager.instance.AddCommonFish(int amount);
bool success = CurrencyManager.instance.SpendCommonFish(int amount);

// SaveManager
SaveManager.instance.Save();
PlayerData data = SaveManager.instance.Load();

// GameManager
GameManager.instance.PlayerData;
GameManager.instance.SetPlayerData(data);
GameManager.instance.CreateNewPlayerData();
```

- 골드와 물고기는 `PlayerData`에 직접 더하거나 빼지 않고 `CurrencyManager`를 사용.
- 파일 저장과 불러오기는 `SaveManager`만 담당
- 시스템 간 참조는 매니저 API, 이벤트, ID 기반 데이터 연결로 구현
- 새 매니저는 `Assets/01.Scripts/Managers`, 전투 코드는 `Combat`, 경영 코드는 `Restaurant`, 데이터 클래스는 `Data` 폴더.
- 각 담당자는 본인 담당 폴더와 파일을 우선 수정한다. `Core`, `PlayerData`, 다른 담당자의 파일 수정은 먼저 공유
- 프리팹과 ScriptableObject 등 데이터는 각 시스템 담당자가 준비한다. 다른 담당자는 해당 프리팹이나 데이터의 내부 구조를 임의로 변경하지 않는다.
- 기능을 연결하기 전에는 임시 수치와 임시 UI를 사용

## 첫 통합 테스트

1. 새 게임을 시작.
2. 전투에서 적 1종을 처치해 커먼 물고기를 획득
3. 경영 화면에서 물고기가 소비되고 골드가 증가하는지 확인.
4. 골드를 사용해 업그레이드 1회를 진행.
5. 저장 후 재실행하여 골드, 물고기, 업그레이드 레벨, 스테이지가 유지되는지 확인.
6. 골드를 사용해 뽑기를 1회 실행하고, 획득한 무기 이름이 Console의 `Debug.Log`에 출력되는지 확인.
