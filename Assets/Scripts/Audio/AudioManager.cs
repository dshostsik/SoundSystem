using UnityEngine;


class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSourcePrefab;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void PlaySoundClip(AudioClip clip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(audioSourcePrefab, spawnTransform.position, Quaternion.identity);

        audioSource.clip = clip;

        audioSource.volume = volume;

        audioSource.Play();

        float clipLength = audioSource.clip.length;

        Destroy(audioSource.gameObject, clipLength);
    }

    /// <summary>
    /// Mapuje dB SPL -> liniowy wolumen i odtwarza klip. 
    /// <paramref name="dbSPL"/> — poziom w dB, <paramref name="dbForFullVolume"/> — dB odpowiadające volume=1.0 (kalibracja).
    /// </summary>
    public void PlaySoundClipAtDb(AudioClip clip, Transform spawnTransform, float dbSPL, float dbForFullVolume = 94f)
    {
        float linear = DbToLinearVolume(dbSPL, dbForFullVolume);
        PlaySoundClip(clip, spawnTransform, Mathf.Clamp01(linear));
    }

    /// <summary>
    /// Konwersja: amplitude (0..∞) = 10^( (db - dbRef) / 20 )
    /// Jeżeli db == dbRef -> 1.0, jeżeli db < dbRef -> <1.0.
    /// </summary>
    private static float DbToLinearVolume(float db, float dbRef)
    {
        return Mathf.Pow(10f, (db - dbRef) / 20f);
    }
}

