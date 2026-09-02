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



    [Range(0f, 1f)] public float defaultMaster = 0.5f;
    [Range(0f, 1f)] public float defaultBGM = 0.5f;
    [Range(0f, 1f)] public float defaultSFX = 0.5f;

    protected override void Awake()
    {
        base.Awake();
        ApplyDefaultVolume();
    }

    private void OnEnable()
    {
        GameEvents.onValueChangeMaster += SetMasterVolume;
        GameEvents.onValueChangeBGM += SetBGMVolume;
        GameEvents.onValueChangeSFX += SetSFXVolume;
    }

    private void OnDisable()
    {
        GameEvents.onValueChangeMaster -= SetMasterVolume;
        GameEvents.onValueChangeBGM -= SetBGMVolume;
        GameEvents.onValueChangeSFX -= SetSFXVolume;
    }

    private void ApplyDefaultVolume()
    {
        BroAudio.SetVolume(BroAudioType.All, defaultMaster);
        BroAudio.SetVolume(BroAudioType.Music, defaultBGM);
        BroAudio.SetVolume(BroAudioType.SFX, defaultSFX);
    }

    public void PlayAudio(SoundID audio)
    {
        BroAudio.Play(audio);
    }

    public void StopBGM()
    {
        BroAudio.Stop(BroAudioType.Music);
    }

    public void SetMasterVolume(float value)
    {
        BroAudio.SetVolume(BroAudioType.All, value);
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
    public static Action<float> onValueChangeMaster;
    public static Action<float> onValueChangeBGM;
    public static Action<float> onValueChangeSFX;
}