﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using TheSanctuary;
using Newtonsoft.Json;

//  да, знаю, что такие слова в названии класса, как
//  "Logic", "Master", "Manager" и т.д. - дурной тон,
//  но скрипт писался около года назад, по этому остался
//  в первоначальном виде во многом
//
//  данный класс создан для управления логикой игрового
//  музыкального плеера
//
//  сейчас работаю над подгрузкой данных с сервера,
//  и с большой вероятностью скрип сильно изменится в ближайшее время

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    private Dictionary<float, AudioClip> _tracksDict = new Dictionary<float, AudioClip>();
    private AudioSource _source;
    private LEDDisplayLogic _ledScript;
    private List<float> _tracksLenght;
    [SerializeField]
    private List<AudioClip> Tracks = new List<AudioClip>();
    [SerializeField]
    public GameObject MainMenuBall;
    private float[] _clipSampleData;
    [SerializeField]
    public float UpdateStep = 0.01f;
    [SerializeField]
    public int SampleRate = 1024;
    private float _currentUpdateTime = 0f;
    public float ClipLoudness;
    [SerializeField]
    public float SizeFactor = 20;
    public float MinSize = 7;
    public float MaxSize = 9;

    private Coroutine C_BallAudioVizualization;

    [SerializeField]
    private GameObject BackgroundPanel;
    [SerializeField]
    private List<Texture2D> TrackCovers = new List<Texture2D>();

    [SerializeField]
    public GameObject NetworkData;

    private TMP_Text _trackNameText;
    private TMP_Text _artistNameText;

    [SerializeField]
    public Material[] NetworksMaterial;

    private List<Artist> _artists = new List<Artist>();

    public AudioSource GetAudioSource()
    {
        return _source;
    }

    void SetArtistData()
    {
        string path = "Assets/DownloadData/ArtistData";
        if(!Directory.Exists(path)) Directory.CreateDirectory(path);

        string[] files = Directory.GetFiles(path);
        if(files.Length != 0)
        {
            foreach(var item in files)
            {
                if(!item.Contains(".meta"))
                {
                    using(FileStream fs = File.OpenRead(item))
                    {
                        byte[] data = new byte[fs.Length];
                        fs.Read(data, 0, data.Length);
                        string jsonData = System.Text.Encoding.UTF8.GetString(data);

                        var artistData = JsonConvert.DeserializeObject<Artist>(jsonData);
                        _artists.Add(artistData);
                    }
                }
            }
        }
    }

    void Start()
    {
        SetArtistData();

        _ledScript = GameObject.Find("LEDText").GetComponent<LEDDisplayLogic>();
        _source = GetComponent<AudioSource>();

        foreach(var item in Tracks)
        {
            _tracksDict.Add(item.length, item);
        }

        _tracksLenght = new List<float>(_tracksDict.Keys);

        _source.clip = _tracksDict[_tracksLenght[UnityEngine.Random.Range(0, _tracksLenght.Count)]];
        _ledScript.SetLEDTrackName(_source.clip.name, _source.clip.length);
        _source.Play();

        BallVizualization(true);

        _trackNameText = GetComponentsInChildren<TMP_Text>().First(x => x.name.Contains("Track"));
        _artistNameText = GetComponentsInChildren<TMP_Text>().First(x => x.name.Contains("Artist"));

        SetTrackData(_source.clip.name);

        _clipSampleData = new float[SampleRate];
    }

    public void OnNextTrack()
    {
        BallVizualization(false);

        _source.Stop();

        if(_tracksLenght.IndexOf(_source.clip.length) != _tracksLenght.Count - 1)
        _source.clip = _tracksDict[_tracksLenght[_tracksLenght.IndexOf(_source.clip.length)+1]];
        else
        _source.clip = _tracksDict[_tracksLenght[0]];

        _ledScript.SetLEDTrackName(_source.clip.name, _source.clip.length);
        _source.Play();

        SetTrackData(_source.clip.name);

        BallVizualization(true);
    }

    public void OnPreviewTrack()
    {
        BallVizualization(false);

        _source.Stop();

        if(_tracksLenght.IndexOf(_source.clip.length) != 0)
        _source.clip = _tracksDict[_tracksLenght[_tracksLenght.IndexOf(_source.clip.length)-1]];
        else
        _source.clip = _tracksDict[_tracksLenght[_tracksLenght.Count - 1]];

        _ledScript.SetLEDTrackName(_source.clip.name, _source.clip.length);
        _source.Play();

        SetTrackData(_source.clip.name);

        BallVizualization(true);
    }

    public void BallVizualization(bool Play)
    {
        if(C_BallAudioVizualization != null)
        {
            StopCoroutine(C_BallAudioVizualization);
            C_BallAudioVizualization = null;
        }   

        if(Play)
        {
            C_BallAudioVizualization = StartCoroutine(BallVizualization());
        }
    }

    //  метод описывает поведения фонового мяча на заднем плане главного меню
    //  в зависимости от преобладания в треке низких частот
    private IEnumerator BallVizualization()
    {
        yield return null;
        while(true)
        {
            _currentUpdateTime += Time.deltaTime;
            if(_currentUpdateTime >= UpdateStep)
            {
                _currentUpdateTime = 0;
                _source.clip.GetData(_clipSampleData, _source.timeSamples);
                ClipLoudness = 0;

                foreach(var sample in _clipSampleData)
                {
                    ClipLoudness += Mathf.Abs(sample);
                }

                ClipLoudness /= SampleRate;
                ClipLoudness *= SizeFactor;
                ClipLoudness = Mathf.Clamp(ClipLoudness, MinSize, MaxSize);

                MainMenuBall.transform.localScale = Vector3.Lerp(MainMenuBall.transform.localScale, new Vector3(1, 1, 1) * ClipLoudness, Time.deltaTime);
            }

            yield return null;
        }
    }

    private void SetTrackData(string TrackName)
    {
        var Text = TrackName.Split('-');

        foreach(var item in GameObject.FindGameObjectsWithTag("Social Network Data"))
        {
            Destroy(item);
        }
        
        Material BGMaterial = BackgroundPanel.GetComponent<MeshRenderer>().material;
        Texture2D Cover = TrackCovers.FirstOrDefault(x => x.name.Contains(Text[0]));

        float Yoffset = 0.6f;

        BGMaterial.SetTexture("Texture2D_4d1c136f48194b7eb5ae69894dd308e7", Cover);

        _artistNameText.text = Text[0];
        _trackNameText.text = Text[1];

        var artistData = _artists.Find(x => _source.clip.name.Contains(x.Name));

        GameObject NetworkDataObject;
        Material DataObjectMaterial;
        Dictionary<string, string> keyWord = new Dictionary<string, string>();

        if(artistData.Mail != null) keyWord.Add("Mail", artistData.Mail);
        if(artistData.Instagram != null) keyWord.Add("Instagram", artistData.Instagram);
        if(artistData.SoundCloud != null) keyWord.Add("SoundCloud", artistData.SoundCloud);
        if(artistData.Facebook != null) keyWord.Add("Facebook", artistData.Facebook);
        if(artistData.Twitter != null) keyWord.Add("Twitter", artistData.Twitter);
        if(artistData.TikTok != null) keyWord.Add("TikTok", artistData.TikTok);

        foreach(var item in keyWord)
        {
            NetworkDataObject = Instantiate(NetworkData, Vector3.zero, Quaternion.Euler(new Vector3(90, 0, 0)));
            DataObjectMaterial = NetworkDataObject.GetComponentInChildren<MeshRenderer>().material;
            NetworkDataObject.transform.SetParent(transform);
            NetworkDataObject.transform.localPosition = new Vector3(0.145f, Yoffset, -0.3f);

            Yoffset -= 0.3f;

            NetworkDataObject.GetComponentInChildren<TMP_Text>().text = item.Value;
            DataObjectMaterial = NetworksMaterial.First(x => x.name.Contains(item.Key));
            NetworkDataObject.GetComponentInChildren<MeshRenderer>().material = DataObjectMaterial;
        }
    }

    //  находится в разработке
    private IEnumerator DowloadTracksData()
    {
        yield return null;
        
        try
        {
            if(!Directory.Exists("Assets/DownloadBundles"))
            {
                BinaryFormatter formater = new BinaryFormatter();

                var files = Directory.GetFiles("Assets/DownloadBundles");

                foreach(var fileName in files)
                {
                    using(FileStream stream = new FileStream($"Assets/DownloadBundles/{fileName}", FileMode.OpenOrCreate))
                    {
                        AssetBundle assetRequest = (AssetBundle)formater.Deserialize(stream);
                        
                        var track = assetRequest.LoadAssetAsync("track.mp3", typeof(AudioClip));
                        var logo = assetRequest.LoadAssetAsync("logo.png", typeof(Texture2D));

                        Tracks.Add(track.asset as AudioClip);
                        TrackCovers.Add(logo.asset as Texture2D);
                    }
                }
            }

        }
        catch(Exception ex)
        {
            Debug.Log(ex);
        }

        yield break;
    }
}