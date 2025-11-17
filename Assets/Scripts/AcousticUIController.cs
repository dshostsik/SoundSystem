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
    
    [Header("References")]
    public RoomAcousticsManager acoustics;
    public SurroundSystemFactory systemFactory;

    [Header("UI Elements – system selection")]
    public TMP_Dropdown systemDropdown;

    public Button startButton;
    
    [Header("UI Elements – speaker controls")]
    public TMP_Dropdown speakerDropdown;
    public Slider speakerLevelSlider;
    public Slider speakerRotationSlider;

    [Header("UI Elements – listener controls")]
    public Slider listenerX;
    public Slider listenerZ;

    [Header("UI Elements – material controls")]
    public TMP_Dropdown surfaceDropdown;
    public TMP_Dropdown materialDropdown;

    [Header("UI Elements – debug")]
    public TMP_Text infoText;

    private Speaker[] currentSpeakers;

    void Awake()
    {
        doc = GetComponent<UIDocument>();

        startButton = doc.rootVisualElement.Q<Button>("start_simulation");
        speakerLevelSlider = doc.rootVisualElement.Q<Slider>("sound_level");
        //slider
        //startButton.RegisterCallback<ClickEvent>(RunSimulation);
        speakerLevelSlider.RegisterValueChangedCallback(OnSpeakerLevelChanged);
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

        RefreshInfo();
    }

    // --- SYSTEM SELECTION ------------------------------------------------------

    private void OnSystemSelected(int index)
    {
        if (index == 0)
            systemFactory.Build51();
        else
            systemFactory.Build71();

        currentSpeakers = systemFactory.CreatedSpeakers.ToArray();
        RebuildSpeakerDropdown();
        acoustics.RunSimulation();
        RefreshInfo();
    }

    // --- SPEAKER LIST ---------------------------------------------------------

    private void RebuildSpeakerDropdown()
    {
        speakerDropdown.ClearOptions();
        foreach (var sp in currentSpeakers)
            speakerDropdown.options.Add(new TMP_Dropdown.OptionData(sp.channelName));

        speakerDropdown.RefreshShownValue();
    }

    private Speaker GetSelectedSpeaker()
    {
        int idx = speakerDropdown.value;
        if (idx < 0 || idx >= currentSpeakers.Length) return null;
        return currentSpeakers[idx];
    }

    // --- SPEAKER PARAMS -------------------------------------------------------

    private void OnSpeakerSelected(int index)
    {
        var sp = GetSelectedSpeaker();
        if (sp == null) return;

        speakerLevelSlider.value = sp.baseLevel;
        speakerRotationSlider.value = sp.transform.eulerAngles.y;
    }

    private void OnSpeakerLevelChanged(ChangeEvent<float> newLevel)
    {
        var sp = GetSelectedSpeaker();
        if (sp == null) return;

        sp.SetBaseLevel(newLevel.newValue);
        RefreshInfo();
    }

    private void OnSpeakerRotationChanged(float rotY)
    {
        var sp = GetSelectedSpeaker();
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
        var speakers = acoustics.systemFactory.CreatedSpeakers;

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
        //startButton.UnregisterCallback<ClickEvent>(RunSimulation);
    }
}
