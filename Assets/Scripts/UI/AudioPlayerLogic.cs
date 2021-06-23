using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using TheSanctuary;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

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
public class AudioPlayerLogic : MonoBehaviour
{
    private Dictionary<float, AudioClip> _tracksDict = new Dictionary<float, AudioClip>();
    [SerializeField]
    public AudioSource Source;
    private LEDDisplayLogic _ledScript;
    private List<float> _tracksLenght;
    private List<AudioClip> _tracks = new List<AudioClip>();

    [SerializeField]
    public GameObject MainMenuBall;

    private float[] _clipSampleData;
    [SerializeField]
    public float UpdateStep = 0.005f;
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
    
    private List<Texture2D> _trackCovers = new List<Texture2D>();

    [SerializeField]
    public GameObject NetworkData;

    private TMP_Text _trackNameText;
    private TMP_Text _artistNameText;

    [SerializeField]
    public Material[] NetworksMaterial;

    List<Artist> _beatmakersList = new List<Artist>();

    void Start()
    {
        // List<string> ArtistNames = new List<string>();

        // foreach(var track in Tracks)
        // {
        //     var ArtistName = track.name.Split('-').FirstOrDefault();
        //     if(ArtistNames.Contains(ArtistName)) continue;

        //     Dictionary<float, string> ArtistTracks = new Dictionary<float, string>();

        //     foreach(var trackData in Tracks)
        //     {
        //         if(trackData.name.Split('-').FirstOrDefault() == ArtistName)
        //         {
        //             ArtistTracks.Add(trackData.length, trackData.name.Split('-').Last());
        //             ArtistNames.Add(ArtistName);
        //         }
        //     }

        //     switch(ArtistName)
        //     {
        //         case "drake":
        //             _beatmakersList.Add(new Artist(ArtistName, ArtistTracks, "drake@gmail.com", "@champagnepappi", "/drake", "/drake", "@Drake"));
        //             break;
        //         case "goldman":
        //             _beatmakersList.Add(new Artist(ArtistName, ArtistTracks, "muradgoldman@gmail.com", "@muradgoldman", string.Empty, string.Empty, string.Empty, "@muradgoldman"));
        //             break;
        //         case "clvne":
        //             _beatmakersList.Add(new Artist(ArtistName, ArtistTracks, "divak.design@gmail.com", "@divak.design", string.Empty, string.Empty, "@divak.design", "@divak.design"));
        //             break;
        //     }
        // }

        // if(Directory.Exists("Assets/DownloadBundles"))
        // {
        //     Debug.Log("файлов нет... отправляю запрос");
        //     FindObjectOfType<Network>().SendServerData("ServerGetTracksData");
        // }

        _clipSampleData = new float[SampleRate];

        _ledScript = GameObject.Find("LEDText").GetComponent<LEDDisplayLogic>();
        Source = this.GetComponent<AudioSource>();
    }

    public void OnNextTrack()
    {
        BallVizualization(false);

        Source.Stop();

        if(_tracksLenght.IndexOf(Source.clip.length) != _tracksLenght.Count - 1)
        Source.clip = _tracksDict[_tracksLenght[_tracksLenght.IndexOf(Source.clip.length)+1]];
        else
        Source.clip = _tracksDict[_tracksLenght[0]];

        _ledScript.SetLEDTrackName(Source.clip.name, Source.clip.length);
        Source.Play();

        SetTrackData(Source.clip.name);

        BallVizualization(true);
    }

    public void OnPreviewTrack()
    {
        BallVizualization(false);

        Source.Stop();

        if(_tracksLenght.IndexOf(Source.clip.length) != 0)
        Source.clip = _tracksDict[_tracksLenght[_tracksLenght.IndexOf(Source.clip.length)-1]];
        else
        Source.clip = _tracksDict[_tracksLenght[_tracksLenght.Count - 1]];

        _ledScript.SetLEDTrackName(Source.clip.name, Source.clip.length);
        Source.Play();

        SetTrackData(Source.clip.name);

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
                Source.clip.GetData(_clipSampleData, Source.timeSamples);
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
        Texture2D Cover = _trackCovers.First(x => x.name.Contains(Text[0].ToLower()));
        if(Cover == null) Cover = _trackCovers.LastOrDefault();

        float Yoffset = 0.3f;

        BGMaterial.SetTexture("Texture2D_4d1c136f48194b7eb5ae69894dd308e7", Cover);

        _artistNameText.text = Text[0];
        _trackNameText.text = Text[1];

        foreach(var network in _beatmakersList.First(x => x.GetArtistName() == Text[0]).GetArtistContacts())
        {
            GameObject NetworkDataObject;
            Material DataObjectMaterial;

            if(network.Value != string.Empty)
            {
                NetworkDataObject = Instantiate(NetworkData, Vector3.zero, Quaternion.Euler(new Vector3(90, 0, 0)));
                DataObjectMaterial = NetworkDataObject.GetComponentInChildren<MeshRenderer>().material;
                NetworkDataObject.transform.SetParent(transform);
                NetworkDataObject.transform.localPosition = new Vector3(0.72f, Yoffset, -0.3f);

                Yoffset -= 0.3f;

                NetworkDataObject.GetComponentInChildren<TMP_Text>().text = network.Value;

                DataObjectMaterial = NetworksMaterial.First(x => x.name.Contains(network.Key));

                NetworkDataObject.GetComponentInChildren<MeshRenderer>().material = DataObjectMaterial;
            }
        }
    }

    //  находится в разработке
    public void UnpackTracksData(Track[] tracks)
    {
        Debug.Log(tracks.Length);
        try
        {
            for(int i = 0; i < tracks.Length; i++)
            {
                var bundleData = tracks[i].trackBundleData;
                File.WriteAllBytes($"Assets/DownloadBundles/{tracks[i].trackAuthor}-{tracks[i].trackName}.unity3d", bundleData);
            }

            StartCoroutine(DowloadTracksData());
        }
        catch(Exception ex)
        {
            Debug.LogError(ex);
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

                        _tracks.Add(track.asset as AudioClip);
                        _trackCovers.Add(logo.asset as Texture2D);
                    }
                }
            }

            foreach(var item in _tracks)
            {
                _tracksDict.Add(item.length, item);
            }

            _tracksLenght = new List<float>(_tracksDict.Keys);

            Source.clip = _tracksDict[_tracksLenght[UnityEngine.Random.Range(0, _tracksLenght.Count)]];
            _ledScript.SetLEDTrackName(Source.clip.name, Source.clip.length);
            Source.Play();

            BallVizualization(true);

            _trackNameText = GetComponentsInChildren<TMP_Text>().First(x => x.name.Contains("Track"));
            _artistNameText = GetComponentsInChildren<TMP_Text>().First(x => x.name.Contains("Artist"));

            SetTrackData(Source.clip.name);
        }
        catch(Exception ex)
        {
            Debug.Log(ex);
        }

        yield break;
    }
}