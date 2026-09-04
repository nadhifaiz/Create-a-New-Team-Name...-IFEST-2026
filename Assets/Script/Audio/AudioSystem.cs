using System;
using Ami.BroAudio;
using UnityEngine;

public class AudioSystem : SingletonMonoBehaviour<AudioSystem>
{
    [Header("BGM")]
    public SoundID bgm_MainMenu;
    public SoundID bgm_BattleBGM1;
    public SoundID bgm_BattleBGM2;
    public SoundID bgm_BattleBGM3;

    [Header("SFX")]
    public SoundID sfx_HitPlay;
    public SoundID sfx_AwanTransisi;
    public SoundID sfx_UnlockACharacter;
    public SoundID sfx_CharacterSelected;
    public SoundID sfx_BattonSwingGoblin;
    public SoundID sfx_Goblin;
    public SoundID sfx_SuaraTanahKebelah;
    public SoundID sfx_TancapPedangKeTanah;
    public SoundID sfx_PedangPlayerHilang;
    public SoundID sfx_Api;
    public SoundID sfx_NagaNyemburinApi;
    public SoundID sfx_NagaGrowl;
    public SoundID sfx_PenyihirBuatPerisai;
    public SoundID sfx_Kebakar;
    public SoundID sfx_Hantu;
    public SoundID sfx_HantuMakanPlayer;
    public SoundID sfx_IceShield;

    [Range(0f, 1f)] public float defaultBGM = 0.5f;
    [Range(0f, 1f)] public float defaultSFX = 0.5f;

    private SoundID currentBgm;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }

        DontDestroyOnLoad(gameObject);
        ApplyDefaultVolume();
    }

    private void OnEnable()
    {
        GameEvents.onValueChangeBGM += SetBGMVolume;
        GameEvents.onValueChangeSFX += SetSFXVolume;
    }

    private void OnDisable()
    {
        GameEvents.onValueChangeBGM -= SetBGMVolume;
        GameEvents.onValueChangeSFX -= SetSFXVolume;
    }

    private void ApplyDefaultVolume()
    {
        BroAudio.SetVolume(BroAudioType.Music, defaultBGM);
        BroAudio.SetVolume(BroAudioType.SFX, defaultSFX);
    }

    public void PlayAudio(SoundID audio)
    {
        BroAudio.Play(audio);
    }

    public static void PlaySfx(SoundID sound)
    {
        if (Instance == null || sound.Equals(SoundID.Invalid))
        {
            return;
        }

        Instance.PlayAudio(sound);
    }

    public void PlayBGM(SoundID bgm)
    {
        if (bgm.Equals(SoundID.Invalid))
        {
            return;
        }

        if (bgm.Equals(currentBgm))
        {
            return;
        }

        BroAudio.Play(bgm);
        currentBgm = bgm;
    }

    public void PlayBattleBGM(int levelIndex)
    {
        SoundID battleBgm = levelIndex switch
        {
            1 => bgm_BattleBGM1,
            2 => bgm_BattleBGM2,
            3 => bgm_BattleBGM3,
            _ => bgm_BattleBGM1
        };

        PlayBGM(battleBgm);
    }

    public void StopBGM()
    {
        BroAudio.Stop(BroAudioType.Music);
        currentBgm = default;
    }

    public void SetBGMVolume(float value)
    {
        BroAudio.SetVolume(BroAudioType.Music, value);
    }

    public void SetSFXVolume(float value)
    {
        BroAudio.SetVolume(BroAudioType.SFX, value);
    }
}

public static partial class GameEvents
{
    public static Action<float> onValueChangeBGM;
    public static Action<float> onValueChangeSFX;
}
