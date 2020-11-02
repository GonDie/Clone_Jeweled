using System.Collections.Generic;
using UnityEngine;

public class SFXManager : Singleton<SFXManager>
{
    [SerializeField] AudioClip _selectPieceSFX;
    [SerializeField] AudioClip _swapPieceSFX;
    [SerializeField] AudioClip _matchPieceSFX;
    [SerializeField] AudioClip _pieceDropSFX;

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
        _audioDictionary.Add(SFXType.DropPiece, _pieceDropSFX);
    }

    public void PlaySFX(SFXType type, bool prioritary = false, float volume = 1f)
    {
        if(!prioritary)
            _audioSrc.PlayOneShot(_audioDictionary[type], volume);
        else if(!_audioSrc.isPlaying)
        {
            _audioSrc.clip = _audioDictionary[type];
            _audioSrc.Play();
        }
    }
}