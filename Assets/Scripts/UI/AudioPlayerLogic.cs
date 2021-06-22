using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using TheSanctuary;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayerLogic : MonoBehaviour
{
    private Dictionary<float, AudioClip> TracksDict = new Dictionary<float, AudioClip>();
    [SerializeField]
    public AudioSource Source;
    private LEDDisplayLogic LEDScript;
    private List<float> TracksLenght;
    private List<AudioClip> _tracks = new List<AudioClip>();

    [SerializeField]
    public GameObject _mainMenuBall;

    private float[] _clipSampleData;
    [SerializeField]
    public float _updateStep = 0.005f;
    [SerializeField]
    public int _sampleRate = 1024;
    private float _currentUpdateTime = 0f;
    public float _clipLoudness;
    [SerializeField]
    public float _sizeFactor = 20;
    public float _minSize = 7;
    public float _maxSize = 9;

    private Coroutine C_BallAudioVizualization;

    [SerializeField]
    private GameObject _backgroundPanel;
    
    private List<Texture2D> _trackCovers = new List<Texture2D>();

    [SerializeField]
    public GameObject _networkData;

    private TMP_Text _trackNameText;
    private TMP_Text _artistNameText;

    [SerializeField]
    public Material[] _networksMaterial;

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

        _clipSampleData = new float[_sampleRate];

        LEDScript = GameObject.Find("LEDText").GetComponent<LEDDisplayLogic>();
        Source = this.GetComponent<AudioSource>();
    }

    public void OnNextTrack()
    {
        BallVizualization(false);

        Source.Stop();

        if(TracksLenght.IndexOf(Source.clip.length) != TracksLenght.Count - 1)
        Source.clip = TracksDict[TracksLenght[TracksLenght.IndexOf(Source.clip.length)+1]];
        else
        Source.clip = TracksDict[TracksLenght[0]];

        LEDScript.SetLEDTrackName(Source.clip.name, Source.clip.length);
        Source.Play();

        SetTrackData(Source.clip.name);

        BallVizualization(true);
    }

    public void OnPreviewTrack()
    {
        BallVizualization(false);

        Source.Stop();

        if(TracksLenght.IndexOf(Source.clip.length) != 0)
        Source.clip = TracksDict[TracksLenght[TracksLenght.IndexOf(Source.clip.length)-1]];
        else
        Source.clip = TracksDict[TracksLenght[TracksLenght.Count - 1]];

        LEDScript.SetLEDTrackName(Source.clip.name, Source.clip.length);
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

    private IEnumerator BallVizualization()
    {
        yield return null;
        while(true)
        {
            _currentUpdateTime += Time.deltaTime;
            if(_currentUpdateTime >= _updateStep)
            {
                _currentUpdateTime = 0;
                Source.clip.GetData(_clipSampleData, Source.timeSamples);
                _clipLoudness = 0;

                foreach(var sample in _clipSampleData)
                {
                    _clipLoudness += Mathf.Abs(sample);
                }

                _clipLoudness /= _sampleRate;
                _clipLoudness *= _sizeFactor;
                _clipLoudness = Mathf.Clamp(_clipLoudness, _minSize, _maxSize);

                _mainMenuBall.transform.localScale = Vector3.Lerp(_mainMenuBall.transform.localScale, new Vector3(1, 1, 1) * _clipLoudness, Time.deltaTime);
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
        
        Material BGMaterial = _backgroundPanel.GetComponent<MeshRenderer>().material;
        Texture2D Cover = _trackCovers.Where(x => x.name.Contains(Text[0].ToLower())).ToList().FirstOrDefault();
        if(Cover == null) Cover = _trackCovers.LastOrDefault();

        float Yoffset = 0.3f;

        BGMaterial.SetTexture("Texture2D_4d1c136f48194b7eb5ae69894dd308e7", Cover);

        _artistNameText.text = Text[0];
        _trackNameText.text = Text[1];

        foreach(var network in _beatmakersList.Where(x => x.GetArtistName() == Text[0]).FirstOrDefault().GetArtistContacts())
        {
            GameObject NetworkDataObject;
            Material DataObjectMaterial;

            if(network.Value != string.Empty)
            {
                NetworkDataObject = Instantiate(_networkData, Vector3.zero, Quaternion.Euler(new Vector3(90, 0, 0)));
                DataObjectMaterial = NetworkDataObject.GetComponentInChildren<MeshRenderer>().material;
                NetworkDataObject.transform.SetParent(transform);
                NetworkDataObject.transform.localPosition = new Vector3(0.72f, Yoffset, -0.3f);

                Yoffset -= 0.3f;

                NetworkDataObject.GetComponentInChildren<TMP_Text>().text = network.Value;

                DataObjectMaterial = _networksMaterial.Where(x => x.name.Contains(network.Key)).ToList().FirstOrDefault();

                NetworkDataObject.GetComponentInChildren<MeshRenderer>().material = DataObjectMaterial;
            }
        }
    }

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
                TracksDict.Add(item.length, item);
            }

            TracksLenght = new List<float>(TracksDict.Keys);

            Source.clip = TracksDict[TracksLenght[UnityEngine.Random.Range(0, TracksLenght.Count)]];
            LEDScript.SetLEDTrackName(Source.clip.name, Source.clip.length);
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