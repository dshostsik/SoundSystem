using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using SFB;
using UnityEngine.Networking;
using Visualization;

/// <summary>
/// UI Controller — pe³ni rolê ³¹cznika miêdzy interfejsem u¿ytkownika
/// (slidery, dropdowny, guziki) a silnikiem akustycznym.
///
/// Dostarcza:
/// - wybór systemu (5.1 / 7.1)
/// - zmianê parametrów g³oœników (poziom SPL, directivity, rotacja)
/// - przesuwanie s³uchacza
/// - wybór materia³u powierzchni pokoju
/// - manualne odpalanie symulacji.
/// </summary>
public class AcousticUIController : MonoBehaviour
{
    private UIDocument doc;

    [Header("References")] public RoomAcousticsManager acoustics;
    public SurroundSystemFactory systemFactory;

    [Header("UI Elements – system selection")]
    public DropdownField systemDropdown;

    [Header("UI Elements – speaker controls")]
    public DropdownField speakerDropdown;

    public Slider speakerLevelSlider;
    public Slider speakerRotationSlider;

    //[Header("UI Elements – listener controls")]
    //public Slider listenerX;

    //public Slider listenerZ;

    [Header("Audio Mixer (test)")] [Tooltip("Klips testowy odtwarzany przez Play Test")]
    public AudioClip testClip;

    [Tooltip("dB odpowiadaj±ce volume=1.0 (kalibracja). Domy¶lnie 94 dB")]
    public float dbForFullVolume = 94f;

    private Button selectSongButton;
    private Button playTestButton;

    //// UI elements dla mixera (pobrane z UIDocument)
    //private Slider masterVolumeSlider;
    //private Label mixerInfoLabel;

    [Header("UI Elements – material controls")]
    public TMP_Dropdown surfaceDropdown;

    public TMP_Dropdown materialDropdown;

    [Header("UI Elements – debug")] public Label infoText;

    private IReadOnlyDictionary<string, Speaker> currentSpeakers;
    private Speaker currentSpeakerSelection;

    public static event Action ConfigurationChangedEvent;

    /// <summary>
    /// Default path to music folder. Maybe users keep their music there
    /// </summary>
    private string pathToMusicFolder;

#if UNITY_WEBGL && !UNITY_EDITOR
    // web
        DllImport("__Internal")
        // Never mind, actually. we still do basic app, not web
    private void OnSelectSongClicked(ClickEvent evt) {
        throw new System.PlatformNotSupportedException("Web pages are not supported");
    }
#else
    // standalone file explorer (Windows - explorer; Mac - finder; Linux - idk tbh) or editor itself
    private static readonly ExtensionFilter[] Filters = { new(".mp3"), new(".aif"), new(".wav"), new(".ogg") };
    private string actualPath;
    private StringBuilder builder = new StringBuilder();

    private string extension;
    private AudioSource audioSrc;
    private GameObject go;
    private AudioSource src;

    /// <summary>
    /// Select the song from file explorer
    /// </summary>
    /// <param name="evt">clicked trigger</param>
    private void OnSelectSongClicked(ClickEvent evt)
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select a song:", pathToMusicFolder, Filters, false);
        if (paths.Length == 0) return;
        builder.Clear();
        actualPath = builder
            .Append("file://")
            .Append(paths[0])
            .ToString();


        string requestedAudio = string.Format(actualPath, "{0}");

        string[] res = requestedAudio.Split(".");

        // "Use 'from end' expression" - Resharper
        extension = res[^1];

        StartCoroutine(LoadAudio());
    }

    private IEnumerator LoadAudio()
    {
        using UnityWebRequest www =
            UnityWebRequestMultimedia.GetAudioClip(actualPath, GetApplicableAudioExtension(extension));
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            testClip = DownloadHandlerAudioClip.GetContent(www);
        }
    }

    private AudioType GetApplicableAudioExtension(string extensionToProceed)
    {
        switch (extensionToProceed.ToLower())
        {
            case "wav": return AudioType.WAV;
            case "mp3": return AudioType.MPEG;
            case "ogg": return AudioType.OGGVORBIS;
            case "aif":
            case "aiff": return AudioType.AIFF;
            default: return AudioType.UNKNOWN;
        }
    }
#endif

    void Awake()
    {
        doc = GetComponent<UIDocument>();

        var root = doc.rootVisualElement;

        speakerLevelSlider = root.Q<Slider>("sound_level");
        systemDropdown = root.Q<DropdownField>("system_dropdown");
        infoText = root.Q<Label>("info_text");
        speakerDropdown = root.Q<DropdownField>("speaker_dropdown");

        selectSongButton = root.Q<Button>("choose_song");
        playTestButton = root.Q<Button>("play_test");


        speakerLevelSlider.RegisterValueChangedCallback(OnSpeakerLevelChanged);
        systemDropdown.RegisterCallback<ChangeEvent<string>>(OnSystemSelected);
        speakerDropdown.RegisterCallback<ChangeEvent<string>>(OnSpeakerSelected);
        selectSongButton.RegisterCallback<ClickEvent>(OnSelectSongClicked);
        playTestButton.RegisterCallback<ClickEvent>(OnPlayTestClicked);

        pathToMusicFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
    }


    private void Start()
    {
        acoustics = RoomAcousticsManager.Instance;
        systemFactory = acoustics.systemFactory;
        ConfigurationChangedEvent?.Invoke();

        go = new GameObject("AcousticUI_TestSource");
        src = go.AddComponent<AudioSource>();


        RefreshInfo();
    }

    // --- SYSTEM SELECTION ------------------------------------------------------
    private void OnSystemSelected(ChangeEvent<string> selectedConfiguration)
    {
        string newValue = (string)selectedConfiguration.newValue.Clone();

        systemDropdown.schedule.Execute(() =>
        {
            switch (newValue)
            {
                case "5.1":
                    systemFactory.Build51();
                    break;
                case "7.1":
                    systemFactory.Build71();
                    break;
                default:
                    systemFactory.Build91();
                    break;
            }

            currentSpeakers = systemFactory.CreatedSpeakers;
            selectSongButton.SetEnabled(true);
            playTestButton.SetEnabled(true);
            RebuildSpeakerDropdown();
            ConfigurationChangedEvent?.Invoke();
            acoustics.RunSimulation();
            RefreshInfo();
        });
    }

    // --- SPEAKER LIST ---------------------------------------------------------

    private void RebuildSpeakerDropdown()
    {
        speakerDropdown.choices.Clear();
        foreach (var sp in currentSpeakers.Keys)
            speakerDropdown.choices.Add(sp);
    }

    private Speaker GetSelectedSpeaker(string requiredChannel)
    {
        return currentSpeakers[requiredChannel];
    }

    // --- SPEAKER PARAMS -------------------------------------------------------

    private void OnSpeakerSelected(ChangeEvent<string> selectedSpeaker)
    {
        currentSpeakerSelection = GetSelectedSpeaker(selectedSpeaker.newValue);
        if (currentSpeakerSelection == null) return;

        speakerLevelSlider.value = currentSpeakerSelection.baseLevel;
        ConfigurationChangedEvent?.Invoke();
        // TODO: bring back when rotation slider implemented
//        speakerRotationSlider.value = currentSpeakerSelection.transform.eulerAngles.y;
    }

    private void OnSpeakerLevelChanged(ChangeEvent<float> newLevel)
    {
        if (currentSpeakerSelection == null) return;

        ConfigurationChangedEvent?.Invoke();
        currentSpeakerSelection.SetBaseLevel(newLevel.newValue);
        RefreshInfo();
    }

    private void OnSpeakerRotationChanged(float rotY)
    {
        if (currentSpeakerSelection == null) return;

        ConfigurationChangedEvent?.Invoke();
        currentSpeakerSelection.SetRotation(Quaternion.Euler(0, rotY, 0));
        RefreshInfo();
    }

    // --- LISTENER -------------------------------------------------------------

    //private void OnListenerMove(float _)
    //{
    //    acoustics.listener.transform.position = new Vector3(
    //        listenerX.value,
    //        acoustics.listener.transform.position.y,
    //        listenerZ.value
    //    );

    //    ConfigurationChangedEvent?.Invoke();
    //    acoustics.RunSimulation();
    //    RefreshInfo();
    //}

    // --- MATERIALS ------------------------------------------------------------
    private void OnMaterialSelected(int index)
    {
        var room = acoustics.room;
        int surfaceIndex = surfaceDropdown.value;

        if (surfaceIndex < 0 || surfaceIndex >= room.surfaces.Length)
            return;

        var surface = room.surfaces[surfaceIndex];

        // zak³adamy ¿e materialDropdown ma przypisane AcousticMaterial z Resources
        var mats = Resources.LoadAll<AcousticMaterial>("");
        if (index < 0 || index >= mats.Length)
            return;

        surface.material = mats[index];
        ConfigurationChangedEvent?.Invoke();
        acoustics.RunSimulation();
        RefreshInfo();
    }

    // --- AUDIO MIXER CALLBACKS -----------------------------------------------

    private void OnMasterVolumeChanged(ChangeEvent<float> evt)
    {
        float v = Mathf.Clamp01(evt.newValue);
        AudioListener.volume = v;
        RefreshInfo();
    }

    private void OnPlayTestClicked(ClickEvent evt)
    {
        if (testClip.Equals(null))
        {
            Debug.LogWarning("AcousticUIController: testClip is not assigned.");
            return;
        }

        if (testClip.loadState == AudioDataLoadState.Loading || !testClip.loadState.Equals(AudioDataLoadState.Loaded))
        {
            Debug.LogWarning("AcousticUIController: testClip is not loaded yet.");
            return;
        }

        var ram = RoomAcousticsManager.Instance;
        if (ram.Equals(null) || ram.listener.Equals(null))
        {
            Debug.LogWarning("AcousticUIController: RoomAcousticsManager or listener missing.");
            return;
        }

        float db = ram.GetLastOverallDb();
        float linear = DbToLinear(db, dbForFullVolume);
        float finalVolume = Mathf.Clamp01(linear * AudioListener.volume);

        WaveVisualizer visualizer = WaveVisualizerFactory.Visualizer;

        visualizer.Frequency = ram.sampleRate;
        visualizer.Speed = ram.speedOfSound;
        visualizer.Amplitude = 0.0025f;

        go.transform.position = ram.listener.transform.position;
        src.clip = testClip;
        src.spatialBlend = 1f;
        src.volume = finalVolume;


        if (src.isPlaying)
        {
            src.Stop();
            src.clip = testClip;
        }

        src.Play();


        Destroy(go, testClip.length + 0.1f);

        RefreshInfo();
    }

    private static float DbToLinear(float db, float dbRef)
    {
        return Mathf.Pow(10f, (db - dbRef) / 20f);
    }

    // --- INFO ----------------------------------------------------------------

    private void RefreshInfo()
    {
        if (systemFactory == null) return;
        var speakers = systemFactory.CreatedSpeakers;

        string msg = $"Speakers: {speakers.Count}\n";
        msg += $"Listener: {acoustics.listener.transform.position}\n";
        msg += $"Room: {acoustics.room.width} x {acoustics.room.length} x {acoustics.room.height}\n";

        if (speakers.Count > 0)
        {
            msg += "\nSpeaker Levels:\n";
            foreach (var sp in speakers.Values)
                msg += $"{sp.channelName} = {sp.baseLevel:F2}\n";
        }

        infoText.text = msg;
    }

    public void OnDisable()
    {
        speakerLevelSlider.UnregisterValueChangedCallback(OnSpeakerLevelChanged);
        systemDropdown.UnregisterValueChangedCallback(OnSystemSelected);
        selectSongButton.UnregisterCallback<ClickEvent>(OnSelectSongClicked);
        playTestButton.UnregisterCallback<ClickEvent>(OnPlayTestClicked);
        speakerDropdown.UnregisterValueChangedCallback(OnSpeakerSelected);
    }
}