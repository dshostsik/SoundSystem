using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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

    [Header("Audio Mixer (test)")] [Tooltip("Klips testowy odtwarzany przez Play Test")]
    public AudioClip testClip;

    [Tooltip("dB odpowiadaj±ce volume=1.0 (kalibracja). Domy¶lnie 94 dB")]
    public float dbForFullVolume = 94f;

    private Button selectSongButton;
    private Button playTestButton;


    [Header("UI Elements – material controls")]
    public DropdownField surfaceDropdown;

    public DropdownField materialDropdown;

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
    private int index = -1;
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

        surfaceDropdown = root.Q<DropdownField>("surface_dropdown");
        materialDropdown = root.Q<DropdownField>("material_dropdown");

        selectSongButton = root.Q<Button>("choose_song");
        playTestButton = root.Q<Button>("play_test");


        speakerLevelSlider.RegisterValueChangedCallback(OnSpeakerLevelChanged);
        systemDropdown.RegisterCallback<ChangeEvent<string>>(OnSystemSelected);
        speakerDropdown.RegisterCallback<ChangeEvent<string>>(OnSpeakerSelected);
        selectSongButton.RegisterCallback<ClickEvent>(OnSelectSongClicked);
        playTestButton.RegisterCallback<ClickEvent>(OnPlayTestClicked);

        surfaceDropdown.RegisterCallback<ChangeEvent<string>>(OnWallSelected);
        materialDropdown.RegisterCallback<ChangeEvent<string>>(OnMaterialSelected);
        materialDropdown.SetEnabled(false);

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
    }

    private void OnSpeakerLevelChanged(ChangeEvent<float> newLevel)
    {
        if (currentSpeakerSelection == null) return;

        ConfigurationChangedEvent?.Invoke();
        currentSpeakerSelection.SetBaseLevel(newLevel.newValue);
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

    // --- SURFACES & MATERIALS ------------------------------------------------------------

    private void OnWallSelected(ChangeEvent<string> selectedSurface)
    {
        string newValue = (string)selectedSurface.newValue.Clone();

        surfaceDropdown.schedule.Execute(() =>
        {
            switch (newValue)
            {
                case "Ceiling":
                    index = 0;
                    break;
                case "Floor":
                    index = 1;
                    break;
                case "Right wall":
                    index = 2;
                    break;
                case "Left wall":
                    index = 3;
                    break;
                case "Front wall":
                    index = 4;
                    break;
                case "Back wall":
                    index = 5;
                    break;
                default:
                    index = -1;
                    break;
            }
            
            acoustics.RunSimulation();
            RefreshInfo();
            materialDropdown.SetEnabled(true);
        });
    }

    private void OnMaterialSelected(ChangeEvent<string> selectedMaterial)
    {
        string newValue = (string)selectedMaterial.newValue.Clone();
        
        materialDropdown.schedule.Execute(() =>
        {
            RoomSurface surface = RoomAcousticsManager.Instance.room.surfaces[index];
            AcousticMaterial newMaterial = newValue switch
            {
                "Ceiling Tiles" => Resources.Load<AcousticMaterial>("Ceiling Tiles"),
                "Concrete" => Resources.Load<AcousticMaterial>("Concrete"),
                "Curtains" => Resources.Load<AcousticMaterial>("Curtains"),
                "Drywall" => Resources.Load<AcousticMaterial>("Drywall"),
                "Rug" => Resources.Load<AcousticMaterial>("Rug"),
                _ => Resources.Load<AcousticMaterial>("Wood")
            };

            surface.material = newMaterial;
            surface.GetComponent<MeshRenderer>().material = newMaterial.assignedMaterial;
        });
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
        if (!testClip)
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
        if (!ram || !ram.listener)
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
        surfaceDropdown.UnregisterValueChangedCallback(OnWallSelected);
        materialDropdown.UnregisterValueChangedCallback(OnMaterialSelected);
    }
}