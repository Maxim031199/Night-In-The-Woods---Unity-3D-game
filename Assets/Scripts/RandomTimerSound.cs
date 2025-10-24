using UnityEngine;

public class RandomTimerSound : MonoBehaviour
{
    [SerializeField] private AudioClip soundEffect; 
    private AudioSource audioSource;
    [SerializeField] private float minInterval = 2f;
    [SerializeField] private float maxInterval = 30f; 
    private float timer;
    private float nextSoundTime;

    private void Start()
    {
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        
        audioSource.clip = soundEffect;

        
        nextSoundTime = Random.Range(minInterval, maxInterval);
    }

    private void Update()
    {
        
        timer += Time.deltaTime;

        
        if (timer >= nextSoundTime)
        {
            PlayRandomSound();

            
            nextSoundTime = Random.Range(minInterval, maxInterval);
        }
    }

    private void PlayRandomSound()
    {
        
        audioSource.Play();
    }
}