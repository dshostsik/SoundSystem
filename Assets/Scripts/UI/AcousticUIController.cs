using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
//using UnityEngine.UI;
using UnityEngine.UIElements;

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

    [Header("References")] 
    public RoomAcousticsManager acoustics;
    public SurroundSystemFactory systemFactory;

    [Header("UI Elements – system selection")]
    public DropdownField systemDropdown;

    public Button startButton;

    [Header("UI Elements – speaker controls")]
    public DropdownField speakerDropdown;

    public Slider speakerLevelSlider;
    public Slider speakerRotationSlider;

    //[Header("UI Elements – listener controls")]
    //public Slider listenerX;

    //public Slider listenerZ;

    [Header("Audio Mixer (test)")]
    [Tooltip("Klips testowy odtwarzany przez Play Test")]
    public AudioClip testClip;
    [Tooltip("dB odpowiadaj¹ce volume=1.0 (kalibracja). Domyœlnie 94 dB")]
    public float dbForFullVolume = 94f;

    // UI elements dla mixera (pobrane z UIDocument)
    private Slider masterVolumeSlider;
    private Button playTestButton;
    private Label mixerInfoLabel;

    [Header("UI Elements – material controls")]
    public TMP_Dropdown surfaceDropdown;

    public TMP_Dropdown materialDropdown;

    [Header("UI Elements – debug")] public Label infoText;

    private IReadOnlyDictionary<string, Speaker> currentSpeakers;
    private Speaker currentSpeakerSelection;

    public static event Action ConfigurationChangedEvent;
    
    void Awake()
    {
        doc = GetComponent<UIDocument>();

        var root = doc.rootVisualElement;

        startButton = root.Q<Button>("start_simulation");
        speakerLevelSlider = root.Q<Slider>("sound_level");
        systemDropdown = root.Q<DropdownField>("system_dropdown");
        infoText = root.Q<Label>("info_text");
        speakerDropdown = root.Q<DropdownField>("speaker_dropdown");

        startButton.RegisterCallback<ClickEvent>(Run);
        speakerLevelSlider.RegisterValueChangedCallback(OnSpeakerLevelChanged);
        systemDropdown.RegisterCallback<ChangeEvent<string>>(OnSystemSelected);
        speakerDropdown.RegisterCallback<ChangeEvent<string>>(OnSpeakerSelected);
    }

    private void Start()
    {
        //systemDropdown.onValueChanged.AddListener(OnSystemSelected);
        //speakerDropdown.onValueChanged.AddListener(OnSpeakerSelected);

        // speakerLevelSlider.onValueChanged.AddListener(OnSpeakerLevelChanged);
        // speakerRotationSlider.onValueChanged.AddListener(OnSpeakerRotationChanged);
        //
        // listenerX.onValueChanged.AddListener(OnListenerMove);
        // listenerZ.onValueChanged.AddListener(OnListenerMove);

        //surfaceDropdown.onValueChanged.AddListener(OnSurfaceSelected);
        //materialDropdown.onValueChanged.AddListener(OnMaterialSelected);
        acoustics = RoomAcousticsManager.Instance;
        systemFactory = acoustics.systemFactory;
        ConfigurationChangedEvent?.Invoke();
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
                case "5_1":
                    systemFactory.Build51();
                    break;
                case "7_1":
                    systemFactory.Build71();
                    break;
                default:
                    systemFactory.Build91();
                    break;
            }

            currentSpeakers = systemFactory.CreatedSpeakers;
            Debug.Log($"{currentSpeakers}:{currentSpeakers.Count}");
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
        if (testClip == null)
        {
            Debug.LogWarning("AcousticUIController: testClip is not assigned.");
            return;
        }

        var ram = RoomAcousticsManager.Instance;
        if (ram == null || ram.listener == null)
        {
            Debug.LogWarning("AcousticUIController: RoomAcousticsManager or listener missing.");
            return;
        }

        float db = ram.GetLastOverallDb();
        float linear = DbToLinear(db, dbForFullVolume);
        float finalVolume = Mathf.Clamp01(linear * AudioListener.volume);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySoundClip(testClip, ram.listener.transform, finalVolume);
        }
        else
        {
            var go = new GameObject("AcousticUI_TestSource");
            go.transform.position = ram.listener.transform.position;
            var src = go.AddComponent<AudioSource>();
            src.clip = testClip;
            src.spatialBlend = 1f;
            src.volume = finalVolume;
            src.Play();
            Destroy(go, testClip.length + 0.1f);
        }

        RefreshInfo();
    }

    private static float DbToLinear(float db, float dbRef)
    {
        return Mathf.Pow(10f, (db - dbRef) / 20f);
    }

    // --- INFO ----------------------------------------------------------------

    private void RefreshInfo()
    {
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
        startButton.UnregisterCallback<ClickEvent>(Run);
        speakerLevelSlider.UnregisterValueChangedCallback(OnSpeakerLevelChanged);
        systemDropdown.UnregisterValueChangedCallback(OnSystemSelected);
        speakerDropdown.UnregisterValueChangedCallback(OnSpeakerSelected);
    }

    private void Run(ClickEvent evt)
    {
        ConfigurationChangedEvent?.Invoke();
        RefreshInfo();
        acoustics?.RunSimulation();
    }
}