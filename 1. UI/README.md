# 🖥️ UI

<br>

## 📁 폴더 구조

```
UI/

├─ Common/                 # 씬에 공통으로 사용되는 UI
├─ InGame/                 # 인게임 전용 UI 모듈 모음
│  ├─ Dialogue/            # 대사/자막 UI (DialogueUI 등)
│  ├─ HackingProgress/     # 해킹 진행도 월드 스페이스 UI
│  ├─ HUD/                 # HUD(체력/탄약/무기 슬롯/토스트 등)
│  ├─ Minigame/            # 미니게임 관련 UI(해킹/퍼즐 등 서브 인터랙션)
│  ├─ Mission/             # 미션 패널/목표 안내
│  ├─ Modification/        # 무기 개조/파트 장착 UI
│  ├─ Quest/               # 퀘스트 진행/알림 UI
│  └─ SkillOverlay/        # 스킬 발동/쿨다운 오버레이
├─ Inventory/              # 인벤토리/무기 프리뷰/스탯 비교
├─ Loading/                # 로딩 화면(진행률/키 게이팅)
├─ Lobby/                  # 타이틀/로비/메인 메뉴
├─ Setting/                # 설정(언어/그래픽/사운드 등)
├─ PauseHandler.cs         # 일시정지 상태 전환/입력 게이팅 핸들러
├─ SlotUtility.cs          # 무기명/설명 로컬라이즈, 스탯 조회 유틸
└─ UIBase.cs               # 모든 UI의 공통 베이스(Initialize/Show/Hide/ResetUI)
```
<br>

---

<br>

## 💻 코드 샘플 및 주석

### 1. HUD – 체력/게이지/토스트 (InGameUI)

```csharp

public override void Show()

{
   base.Show();
   if (!playerCondition) return;
   if (playerCondition.CurrentFocusGauge >= 1f)
       focusEffectCoroutine ??= StartCoroutine(FocusEffectCoroutine());
   if (playerCondition.CurrentInstinctGauge >= 1f)
       instinctEffectCoroutine ??= StartCoroutine(InstinctEffectCoroutine());
}

public void UpdateHealthSlider(float current, float max)

{
 healthText.text = $"{current}";
  maxHealthText.text = $"{max}";
  // 세그먼트 기반 체력바 업데이트 (가득/부분/빈칸)
   int full = Mathf.FloorToInt(current / healthSegmentValue);
   float partial = (current % healthSegmentValue) / healthSegmentValue;
   for (int i = 0; i < healthSegments.Count; i++)
   {
       if (i < full) healthSegments[i].fillAmount = 1f;
       else if (i == full) healthSegments[i].fillAmount = partial;
       else healthSegments[i].fillAmount = 0f;
   }
}

public void ShowToast(string key)

{
var localized = new LocalizedString("New Table", key);
StartCoroutine(SetToastAndHide(localized)); // 로컬라이즈 후 1.5초 표시
}

```

* **요점**: HUD는 **세그먼트 체력바\*\*(피격 시 애니메이션), **스테미나 임계치 경고**, **포커스/인스팅트 풀 게이지 루프 연출**, **로컬라이즈 토스트**를 담당합니다.

<br>

---

<br>

### 2. 무기 슬롯 HUD – 선택/탄약/연출 (WeaponUI)

```csharp

private void Update()

{
   // 선택 슬롯만 스케일 업 (부드럽게 Lerp)
   if (targetScales == null) return;
   for (int i = 0; i < slotTransforms.Count; i++)
       slotTransforms[i].localScale = Vector3.Lerp(slotTransforms[i].localScale, targetScales[i], Time.deltaTime, scaleSpeed);
}
public void Refresh(bool playShowAnimation = true)
{
// 보유 무기 수집 → 슬롯 아이콘/이름/탄약 로컬라이즈 → 선택 슬롯 하이라이트/스케일/알파
// 현재 탄 수 변화 시 셰이크/플래시 연출, 패널 Auto-Hide 코루틴 실행
}

```

* **요점**: 장착 무기/보유 무기를 스캔해 **슬롯 아이콘/이름/탄약**을 갱신하고, **선택 슬롯 스케일 업·알파 강조** 및 **재장전·탄감 셰이크/플래시**를 적용합니다.

* **로컬라이제이션**: 무기 이름은 `SlotUtility.GetWeaponName`을 통해 **LocalizedString** 바인딩.

<br>

---

<br>

### 3. 인벤토리 – 프리뷰/스탯 비교 (InventoryUI + PreviewWeaponHandler)

```csharp
public override void Show()
{
   base.Show();
   panelAnimator?.Rebind();
   panelAnimator?.Play("Panel In");
   RefreshInventoryUI();
    입력 게이팅 & 마우스 커서 노출
   playerCondition.OnDisablePlayerMovement();
   Cursor.lockState = CursorLockMode.None;
   Cursor.visible = true;
}



private void ShowWeapon(WeaponType weaponType)

{
   foreach (var go in weaponPreviewMap.Values) go.SetActive(false);
   if (!ownedWeapons.TryGetValue(weaponType, out var weapon)) { *초기화* return; }


   var stat = SlotUtility.GetWeaponStat(weapon);
   UpdateStats(stat.Damage, stat.MaxAmmoCountInMagazine, stat.Rpm, stat.Recoil, stat.Weight);
   SlotUtility.GetWeaponName(weapon, titleText);            // 이름 로컬라이즈
   SlotUtility.GetWeaponDescription(weapon, descriptionText); // 설명 로컬라이즈
   if (weaponPreviewMap.TryGetValue(weaponType, out var previewGo)) previewGo.SetActive(true);
}

```

* **요점**: 보유 무기를 스캔해 **버튼/프리뷰 오브젝트 매핑**, **최대치 대비 스탯 바** 표시, **이름/설명 로컬라이즈**를 수행합니다.

* **프리뷰**: `PreviewWeaponHandler`가 `unscaledDeltaTime`으로 **지속 회전**, **파트별 렌더러** 접근 지원.

* **수명주기**: `Hide()`에서 패널 아웃 애니메이션 → 코루틴으로 실제 비활성, `ResetUI()`에서 텍스트/버튼/프리뷰 **완전 초기화**.


<br>

---

<br>

### 4. 공통 베이스 – 수명주기 규약 (UIBase)

```csharp

public abstract class UIBase : MonoBehaviour

{
   protected UIManager uiManager;
   public virtual void Initialize(UIManager manager, object param = null) { uiManager = manager; }
   public virtual void Show()  { gameObject.SetActive(true);  }
   public virtual void Hide()  { gameObject.SetActive(false); }
   public virtual void ResetUI() {}
}

```

* **요점**: 모든 화면이 **동일한 진입점**(Initialize/Show/Hide/ResetUI)을 가지며, UIManager는 이 규약만으로 UI를 **등록·표시·리셋**합니다.

<br>

---

<br>

### 5. 공통 유틸 – 이름/설명 로컬라이즈 & 스탯 조회 (SlotUtility)

```csharp

public static void GetWeaponName(BaseWeapon weapon, TextMeshProUGUI nameText)

{
   if (!TryGetWeaponType(weapon, out var type)) { nameText.text = ""; return; }
   var localized = new LocalizedString("New Table", $"{type}_Title");
   localized.StringChanged += val => nameText.text = val; // 언어 전환 시 즉시 반영
}

public static WeaponStatView GetWeaponStat(BaseWeapon weapon)
{
   // 무기 타입별 Damage/RPM/Recoil/Weight/MaxAmmo 집계
   // UI 슬라이더 바인딩에 바로 사용 가능
}

```

* **요점**: **String Table 키 규칙**(예: `Rifle_Title`, `Rifle_Description`)을 통해 **이름/설명**을 자동 교체하고, 스탯은 `WeaponStatView`로 추출해 **슬라이더/텍스트**에 바인딩합니다.

<br>

---

<br>

## ✅ 핵심 포인트 요약

* **UIBase 규약**으로 모든 UI의 **표시/숨김/리셋**을 표준화 → 상태 꼬임/누수 방지
* **HUD/슬롯/인벤토리** 각각의 역할 분리: 실시간 표시 ↔ 선택/연출 ↔ 프리뷰/비교
* **Localization 연동**: 이름/설명/토스트를 **이벤트 기반**으로 갱신(언어 전환 즉시 반영)
* **연출 품질**: 게이지 풀 이펙트/탄감 셰이크/플래시/패널 인·아웃 등 **사용자 피드백 강화**

