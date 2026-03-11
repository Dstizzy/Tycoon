using UnityEngine;

public class IdleAnimator : MonoBehaviour
{
   [Header("Animation Settings")]
   public Sprite[] frames;     
   public float speed = 0.5f;   

   private SpriteRenderer spriteRenderer;
   private int currentFrame = 0;
   private float timer = 0f;

   void Start()
   {
      spriteRenderer = GetComponent<SpriteRenderer>();
   }

   void Update()
   {
      if (frames.Length == 0 || spriteRenderer == null) return;

      timer += Time.deltaTime;

      if (timer >= speed)
      {
         timer = 0f;
         currentFrame++;

         if (currentFrame >= frames.Length)
         {
            currentFrame = 0;
         }

         spriteRenderer.sprite = frames[currentFrame];
      }
   }
}