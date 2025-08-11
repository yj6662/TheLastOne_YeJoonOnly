# 🔊 Sound

<br>

## 📁 폴더 구조

```
Sound/
├─ SoundGroupSO.cs   # Addressables AudioClip 레퍼런스 모음(인덱스/랜덤 선택 API)
└─ SoundPlayer.cs    # 풀링된 오디오 재생기(2D/3D, duration 기반 pitch 보정)

# Manager/SoundManager.cs 는 매니저 모듈에 위치합니다.
```

<br>

---

<br>

## 💻 코드 샘플 및 주석

### 1) 사운드 오케스트레이션 (SoundManager)

```csharp
public enum BgmType { Lobby, Stage1, Stage2, Ending }
public enum SfxType { ButtonClick, RifleShoot, GrenadeExplode, Drone, Dog, /* ... */ }

[Serializable]
public class SoundManager
{
    [SerializeField] private SerializedDictionary<string, SoundGroupSO> soundGroups = new();
    private AudioSource bgmSource; private ResourceManager resourceManager; private ObjectPoolManager poolManager;
    private float masterVolume = 1f, bgmVolume = 1f, sfxVolume = 1f;

    public void Start(AudioSource audioSource)
    {
        resourceManager = CoreManager.Instance.resourceManager;
        poolManager     = CoreManager.Instance.objectPoolManager;
        bgmSource       = audioSource; bgmSource.playOnAwake = true; bgmSource.loop = true;
        if (bgmSource.clip != null) bgmSource.Play();
        LoadVolumeSettings();
    }

    public void CacheSoundGroup()
    {
        soundGroups.Clear();
        foreach (var group in resourceManager.GetAllAssetsOfType<SoundGroupSO>())
            if (!soundGroups.ContainsKey(group.groupName)) soundGroups.Add(group.groupName, group);
    }

    public void PlayBGM(BgmType type, int index = -1)
    {
        if (!soundGroups.TryGetValue(type.ToString(), out var group)) return;
        var clipRef = (index < 0) ? group.GetRandomClip() : group.GetClip(index);
        if (clipRef?.Asset is not AudioClip clip) return;          // 미로딩 방어
        if (bgmSource.clip == clip && bgmSource.isPlaying) return; // 중복 방지
        bgmSource.clip = clip; bgmSource.Play();
    }

    public SoundPlayer PlayUISFX(SfxType type, float duration = -1, int index = -1)
    {
        if (!soundGroups.TryGetValue(type.ToString(), out var group)) return null;
        var clipRef = (index < 0) ? group.GetRandomClip() : group.GetClip(index);
        if (clipRef?.Asset is not AudioClip clip) return null;
        var obj = poolManager.Get("SoundPlayer");
        if (!obj || !obj.TryGetComponent(out SoundPlayer sp)) return null;
        sp.Play2D(clip, duration, masterVolume * sfxVolume); // 2D UI 재생
        return sp;
    }

    public SoundPlayer PlaySFX(SfxType type, Vector3 pos, float duration = -1, int index = -1)
    {
        if (!soundGroups.TryGetValue(type.ToString(), out var group)) return null;
        var clipRef = (index < 0) ? group.GetRandomClip() : group.GetClip(index);
        if (clipRef?.Asset is not AudioClip clip) return null;
        var obj = poolManager.Get("SoundPlayer");
        if (!obj.TryGetComponent(out SoundPlayer sp)) return null;
        sp.Play3D(clip, duration, masterVolume * sfxVolume, pos);  // 3D 월드 재생
        return sp;
    }

    public void SetMasterVolume(float v) { masterVolume = Mathf.Clamp01(v); AudioListener.volume = v; SetBGMVolume(bgmVolume); SetSFXVolume(sfxVolume); PlayerPrefs.SetFloat("MasterVolume", v); }
    public void SetBGMVolume(float v)    { bgmVolume    = Mathf.Clamp01(v); bgmSource.volume = masterVolume * v; PlayerPrefs.SetFloat("BGMVolume", v); }
    public void SetSFXVolume(float v)    { sfxVolume    = Mathf.Clamp01(v); PlayerPrefs.SetFloat("SFXVolume", v); }

    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        bgmVolume    = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxVolume    = PlayerPrefs.GetFloat("SFXVolume", 1f);
        AudioListener.volume = masterVolume; bgmSource.volume = masterVolume * bgmVolume;
    }
}
```

* **역할**: BGM 전용 소스, **SFX(2D/3D)** 재생을 **그룹 기반**으로 지휘. 볼륨 3채널(Master/BGM/SFX) 저장·복원.
* **포인트**: `CacheSoundGroup()`으로 `SoundGroupSO`를 딕셔너리에 캐싱 → `enum.ToString()` **그룹명 매칭** 규칙.

<br>

---

<br>

### 2) 재생기 – 풀링, 2D/3D, 길이 보정 (SoundPlayer)

```csharp
public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource source;

    public void Play2D(AudioClip clip, float duration = -1f, float volume = 1f)
    {
        Setup2D(volume); ApplyDurationPitch(clip, duration); source.clip = clip; source.Play();
        StartCoroutine(ReturnWhenFinished());
    }

    public void Play3D(AudioClip clip, float duration, float volume, Vector3 pos)
    {
        transform.position = pos; Setup3D(volume); ApplyDurationPitch(clip, duration); source.clip = clip; source.Play();
        StartCoroutine(ReturnWhenFinished());
    }

    private void ApplyDurationPitch(AudioClip clip, float duration)
    {
        if (duration <= 0f || !clip) { source.pitch = 1f; return; }
        var targetPitch = Mathf.Clamp(clip.length / duration, 0.5f, 2f); // 길이 맞춤
        source.pitch = targetPitch;
    }

    private IEnumerator ReturnWhenFinished()
    {
        while (source && source.isPlaying) yield return null;      // 재생 완료 대기
        CoreManager.Instance.objectPoolManager.Release(gameObject); // 풀 반환
    }
}
```

* **역할**: 오디오 재생을 담당하는 **풀링 오브젝트**. 2D/3D, **duration 기반 pitch 보정** 지원.
* **효과**: 생성/파괴 비용과 GC 감축, **첫 재생 렉 최소화**.

<br>

---

<br>

### 3) 데이터 – Addressables 그룹 (SoundGroupSO)

```csharp
[CreateAssetMenu(menuName:"Audio/SoundGroup")]
public class SoundGroupSO : ScriptableObject
{
    public string groupName;
    public List<AssetReferenceT<AudioClip>> audioClips;

    public AssetReferenceT<AudioClip> GetClip(int index) =>
        (index >= 0 && index < audioClips.Count) ? audioClips[index] : null;

    public AssetReferenceT<AudioClip> GetRandomClip() =>
        (audioClips == null || audioClips.Count == 0) ? null : audioClips[Random.Range(0, audioClips.Count)];
}
```

* **역할**: SFX/BGM 트랙을 **그룹명**으로 묶는 SO. 인덱스/랜덤 선택 API 제공.
* **매칭 규칙**: `BgmType.Stage1 → "Stage1"`, `SfxType.ButtonClick → "ButtonClick"` 처럼 **enum.ToString() == groupName**.

<br>

---

<br>

### 4) 사용 예시

```csharp
// UI 버튼 클릭음 (2D)
CoreManager.Instance.soundManager.PlayUISFX(SfxType.ButtonClick);

// 폭발/적 경고(3D)
CoreManager.Instance.soundManager.PlaySFX(SfxType.GrenadeExplode, hitPos);
CoreManager.Instance.soundManager.PlaySFX(SfxType.Drone, enemyCenter);

// 씬 진입 BGM
CoreManager.Instance.soundManager.PlayBGM(BgmType.Stage1);

// 볼륨 조정(설정 UI)
CoreManager.Instance.soundManager.SetMasterVolume(0.8f);
CoreManager.Instance.soundManager.SetBGMVolume(0.7f);
CoreManager.Instance.soundManager.SetSFXVolume(0.9f);
```

<br>

---

<br>

## ✅ 핵심 포인트 요약

* **그룹 캐싱** + **enum 매칭 규칙**으로 호출부 단순화: `PlaySFX(SfxType.Drone, pos)`
* **SoundPlayer 풀링**과 **duration 기반 pitch 보정**으로 연출 품질↑·지연↓
* **3채널 볼륨 정책**(Master/BGM/SFX)과 PlayerPrefs 저장/복원으로 일관 믹스 유지
* 필요 시 `LoadClips()`(선로딩)로 첫 재생 스파이크 제거
