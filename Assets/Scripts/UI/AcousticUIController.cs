using System;
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

    [Header("References")] public RoomAcousticsManager acoustics;
    public SurroundSystemFactory systemFactory;

    [Header("UI Elements – system selection")]
    public DropdownField systemDropdown;

    public Button startButton;

    [Header("UI Elements – speaker controls")]
    public DropdownField speakerDropdown;

    public Slider speakerLevelSlider;
    public Slider speakerRotationSlider;

    [Header("UI Elements – listener controls")]
    public Slider listenerX;

    public Slider listenerZ;

    [Header("UI Elements – material controls")]
    public TMP_Dropdown surfaceDropdown;

    public TMP_Dropdown materialDropdown;

    [Header("UI Elements – debug")] public Label infoText;

    private Speaker[] currentSpeakers;

    private int currentSpekaerSelection = -1;
    
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
        RefreshInfo();
    }

    // --- SYSTEM SELECTION ------------------------------------------------------

    private void OnSystemSelected(ChangeEvent<string> selectedConfiguration)
    {
        string newValue = (string) selectedConfiguration.newValue.Clone();
        
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

            currentSpeakers = systemFactory.CreatedSpeakers.ToArray();
            Debug.Log($"{currentSpeakers}:{currentSpeakers.Length}");
            RebuildSpeakerDropdown();
            acoustics.RunSimulation();
            RefreshInfo();
        });
    }

    // --- SPEAKER LIST ---------------------------------------------------------

    private void RebuildSpeakerDropdown()
    {
        speakerDropdown.choices.Clear();
        foreach (var sp in currentSpeakers)
            speakerDropdown.choices.Add(sp.channelName);
    }

    private Speaker GetSelectedSpeaker(int idx)
    {
        if (idx < 0 || idx >= currentSpeakers.Length) return null;
        return currentSpeakers[idx];
    }

    // --- SPEAKER PARAMS -------------------------------------------------------

    private void OnSpeakerSelected(ChangeEvent<string> selectedSpeaker)
    {
        //int.TryParse(selectedSpeaker.newValue, out currentSpekaerSelection);
        
        var speakers = systemFactory.CreatedSpeakers;

        for (int i = 0; i < speakers.Count; i++)
        {
            if (speakers[i].channelName == selectedSpeaker.newValue)
            {
                currentSpekaerSelection = i;
                break;
            }
        }
        
        var sp = GetSelectedSpeaker(currentSpekaerSelection);
        if (sp == null) return;

        speakerLevelSlider.value = sp.baseLevel;
        speakerRotationSlider.value = sp.transform.eulerAngles.y;
    }

    private void OnSpeakerLevelChanged(ChangeEvent<float> newLevel)
    {
        var sp = GetSelectedSpeaker(currentSpekaerSelection);
        if (sp == null) return;

        sp.SetBaseLevel(newLevel.newValue);
        RefreshInfo();
    }

    private void OnSpeakerRotationChanged(float rotY)
    {
        var sp = GetSelectedSpeaker(currentSpekaerSelection);
        if (sp == null) return;

        sp.SetRotation(Quaternion.Euler(0, rotY, 0));
        RefreshInfo();
    }

    // --- LISTENER -------------------------------------------------------------

    private void OnListenerMove(float _)
    {
        acoustics.listener.transform.position = new Vector3(
            listenerX.value,
            acoustics.listener.transform.position.y,
            listenerZ.value
        );

        acoustics.RunSimulation();
        RefreshInfo();
    }

    // --- MATERIALS ------------------------------------------------------------

    private void OnSurfaceSelected(int index)
    {
        // nic nie robimy — czekamy na wybór materia³u
    }

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

        acoustics.RunSimulation();
        RefreshInfo();
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
            foreach (var sp in speakers)
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
        RefreshInfo();
        acoustics?.RunSimulation();
    }
}