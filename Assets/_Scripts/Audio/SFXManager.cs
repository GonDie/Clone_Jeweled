using System.Collections.Generic;
using UnityEngine;

public class SFXManager : Singleton<SFXManager>
{
    [SerializeField] AudioClip _selectPieceSFX;
    [SerializeField] AudioClip _swapPieceSFX;
    [SerializeField] AudioClip _matchPieceSFX;

    AudioSource _audioSrc;

    Dictionary<SFXType, AudioClip> _audioDictionary;

    protected override void Awake()
    {
        base.Awake();

        _audioSrc = GetComponent<AudioSource>();

        _audioDictionary = new Dictionary<SFXType, AudioClip>();
        _audioDictionary.Add(SFXType.SelectPiece, _selectPieceSFX);
        _audioDictionary.Add(SFXType.SwapPiece, _swapPieceSFX);
        _audioDictionary.Add(SFXType.MatchPiece, _matchPieceSFX);
    }

    public void PlaySFX(SFXType type)
    {
        _audioSrc.PlayOneShot(_audioDictionary[type]);
    }
}