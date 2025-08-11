# 🗣️ Dialogue

<br>

## 📁 폴더 구조

```text
DialogueManager.cs  # 캐싱/중복 방지/트리거 진입점

InGame/Dialogue/          # 대화 시스템 전반 (UI 포함)
├─ Data/                  # 데이터 정의
│  ├─ DialogueData.cs     # 1줄(화자/본문/보이스/타입) 구조체
│  └─ DialogueDataSO.cs   # 키 기반 시퀀스 ScriptableObject
├─ Runtime/               # 런타임 로직
│  └─ DialogueTrigger.cs  # 콜라이더 기반 재생 트리거(1회성 옵션)
└─ UI/                    # 표현(타자기/보이스/페이드)
   └─ DialogueUI.cs       # 시퀀스 재생 코루틴, 연출 담당
```

<br>

---

<br>

## 💻 코드 샘플 및 주석

### 1) 데이터 모델 (DialogueData / DialogueDataSO)

```csharp
[Serializable]
public struct DialogueData
{
    public string Speaker;                         // 화자명(표시용)
    public LocalizedString Message;                // 자막(다국어)
    public SpeakerType SpeakerType;                // Player/Ally/Enemy 등 스킨 구분
    public LocalizedAsset<AudioClip> voiceClip;    // 언어별 보이스
}

[CreateAssetMenu(menuName = "ScriptableObjects/DialogueDataSO")]
public class DialogueDataSO : ScriptableObject
{
    public int dialogueKey;                        // 재생 키(세이브와 매칭)
    public List<DialogueData> sequence;            // 한 번에 재생할 줄 묶음
}
```

* **핵심**: 텍스트·보이스를 **Localization 테이블**로 관리, 언어 추가 시 데이터만 증분.

<br>

---

<br>

### 2) 매니저 — 캐싱/중복 방지/트리거 진입점 (DialogueManager)

```csharp
[Serializable]
public class DialogueManager
{
    [SerializeField] private List<DialogueDataSO> dialogueDataList = new();
    private Dictionary<int, DialogueDataSO> dialogueDataDict = new();
    private CoreManager coreManager;

    public void Start() { coreManager = CoreManager.Instance; }

    public bool HasPlayed(int key)
    {
        int saveKey = BaseEventIndex.BaseDialogueIndex + key;
        var scene = coreManager.sceneLoadManager.CurrentScene;
        return coreManager.gameManager.SaveData.stageInfos[scene].completionDict.TryGetValue(saveKey, out var done) && done;
    }

    public void MarkAsPlayed(int key)
    {
        int saveKey = BaseEventIndex.BaseDialogueIndex + key;
        var scene = coreManager.sceneLoadManager.CurrentScene;
        coreManager.gameManager.SaveData.stageInfos[scene].completionDict[saveKey] = true;
    }

    public void CacheDialogueData()
    {
        dialogueDataDict.Clear(); dialogueDataList.Clear();
        foreach (var so in coreManager.resourceManager.GetAllAssetsOfType<DialogueDataSO>())
            if (so && !dialogueDataDict.ContainsKey(so.dialogueKey))
            { dialogueDataList.Add(so); dialogueDataDict.Add(so.dialogueKey, so); }
    }

    public void TriggerDialogue(int key)
    {
        if (coreManager.gameManager.IsGamePaused) return;                 // 일시정지 가드
        if (!dialogueDataDict.TryGetValue(key, out var data)) return;     // 미등록 키 방어
        var ui = coreManager.uiManager.GetUI<DialogueUI>();
        if (ui) ui.ShowSequence(data.sequence);                           // UI로 위임
    }
}
```

* **핵심**: 씬별 세이브 딕셔너리와 키를 조합해 **1회성 대사 보장**. Addressables에서 SO를 **캐싱**하여 빠른 접근.

<br>

---

<br>

### 3) 트리거 — 콜라이더 기반, 1회성 옵션 (DialogueTrigger)

```csharp
public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private int dialogueKey;
    [SerializeField] private bool triggerOnce = true;
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;           // 1회성 가드
        if (!other.CompareTag("Player")) return;
        if (CoreManager.Instance.dialogueManager.HasPlayed(dialogueKey)) return;

        hasTriggered = true;
        CoreManager.Instance.dialogueManager.TriggerDialogue(dialogueKey);
        CoreManager.Instance.dialogueManager.MarkAsPlayed(dialogueKey);
    }
}
```

* **핵심**: 플레이어 진입 시 **HasPlayed → Trigger → MarkAsPlayed** 순으로 안전 재생.

<br>

---

<br>

### 4) UI — 타자기/보이스/페이드(개념)

* `DialogueUI.ShowSequence(List<DialogueData>)`가 진입점.
* 각 줄에 대해 **LocalizedString/LocalizedAsset** 로딩 완료 시점을 이벤트로 받아 자막/보이스를 갱신.
* 보이스 길이(없으면 기본값)를 활용해 \*\*타자기 → 대기 → 역타자(삭제)\*\*를 코루틴에서 원자적으로 처리.
* 화자 타입(SpeakerType)에 따라 **프레임/텍스트 컬러** 스킨 변경.
* 숨김/종료 시 코루틴·보이스를 **즉시 정리**하고 캔버스를 비활성화.

<br>

---

<br>

## ▶ 사용 예시

```csharp
// 1) 트리거 배치형: Collider 안에서 자동 재생(1회성)
//    DialogueTrigger.dialogueKey = 13;

// 2) 스크립트 직접 호출형: 예) 첫 처치 이벤트에서 1회 재생
CoreManager.Instance.dialogueManager.TriggerDialogue(1);
CoreManager.Instance.dialogueManager.MarkAsPlayed(1);

// 3) 초기화: 씬 로드 직후 한 번만 캐싱
CoreManager.Instance.dialogueManager.CacheDialogueData();
```

<br>

---

<br>

## ✅ 핵심 포인트 요약

* **데이터/런타임/UI 분리**: SO(데이터) ↔ 매니저(게이팅/캐싱) ↔ UI(연출) 3계층
* **한 번만 재생 보장**: 키 + 씬 기반 세이브 딕셔너리로 중복 방지
* **Localization 연동**: 텍스트/보이스를 테이블로 분리, 이벤트 기반 갱신
* **UX 일관성**: 보이스 길이와 동기화된 타자기/페이드, 숨김 시 자원 정리로 누수 방지
