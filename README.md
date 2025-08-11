🧐 TheLastOne_GyuminOnly
TheLastOne 게임 제작 중, 한예준이 작업한 스크립트를 모아놨습니다.
한예준은 UI/Dialogue/Sound 및 로컬라이제이션을 담당하였습니다.


📁 폴더 구조 및 담당 영역
1. Manager
   - UIManager : UI를 그룹 단위(로비/인게임/HUD/일시정지)로 등록·표시·숨김·리셋. RegisterStatic/DynamicUI, GetUI<T>(), 컷신/일시정지 게이팅.
   - DialogueManager : DialogueDataSO 캐시 및 키 기반 1회성 재생 게이팅. TriggerDialogue(key)로 DialogueUI 구동.
   - SoundManager : SoundGroupSO 캐시 기반 BGM/SFX(2D/3D) 재생, Master/BGM/SFX 볼륨 저장·복원, 선로딩 지원.
2. UI
   - UIBase : 모든 화면의 수명주기 규약 — Initialize / Show / Hide / ResetUI.
규약: 표시/숨김(Show/Hide)과 상태 초기화(ResetUI) 분리, 코루틴·리스너는 Hide/Reset 시 정리.
   - 개별UI : LobbyUI, FadeUI, InGameUI(HUD), PauseMenuUI, ModificationUI, DialogueUI, Mission/QuestUI, InventoryUI, HackingProgressUI(월드 스페이스) 등.
3. Dialogue
   - DialogueData : DialogueData(자막 LocalizedString, 보이스 LocalizedAsset<AudioClip>), DialogueDataSO(키별 시퀀스).
   - Runtime : DialogueUI(타자기/페이드/보이스 동기화), DialogueTrigger(콜라이더 이벤트), 직접 호출 예: BaseNpcStatController에서 TriggerDialogue(key) 호출.
4. Sound
   - Data : SoundGroupSO — 그룹명과 Addressables AudioClip 리스트(인덱스/랜덤 선택).
   - Runtime: SoundPlayer(풀링된 재생기, 2D/3D 통합, duration 기반 pitch 자동 보정), PlayUISFX / PlaySFX / PlayBGM / StopBGM.  
5. Localization
   - Tables: String Table / Asset Table로 텍스트·아이콘·보이스를 키 기반 관리. SelectedLocale 런타임 전환 + PlayerPrefs 저장/복원.
   - LocalizedString/Asset 이벤트(StringChanged / AssetChanged)로 로딩 완료 시점에 UI 갱신.
   - TMP Font Fallback : 다양한 언어에 대응할 수 있도록 TMP Font Fallback 활용.
