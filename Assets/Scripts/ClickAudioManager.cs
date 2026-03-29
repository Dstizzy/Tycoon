using UnityEngine;

public class AudioManager : MonoBehaviour
{
   public static AudioManager Instance;

   [Header("Audio Sources")]
   public AudioSource soundSource; 

   [Header("Audio Clips")]
   public AudioClip clickSound;  

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

   public void PlayClick()
   {
      if (soundSource != null && clickSound != null)
      {
         soundSource.PlayOneShot(clickSound);
      }
   }
}
