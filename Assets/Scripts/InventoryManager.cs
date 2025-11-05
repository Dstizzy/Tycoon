using System;

using UnityEngine;

public class InventoryManager : MonoBehaviour {

    public static InventoryManager Instance { get; private set; }

    const int MIN_PEARL_COUNT = 0;
    const int MAX_PEARL_COUNT = 1000;
    const int MIN_CRYSTAL_COUNT = MIN_PEARL_COUNT;
    const int MAX_CRYSTAL_COUNT = MAX_PEARL_COUNT;

    public int pearlCount;
    public int crystalCount;
    public int oreCount;


   void Update()
   {
      if (Input.GetKeyDown(KeyCode.P))
      {
         TryAddPearl(800); // Press P to give yourself 1000 pearls
         Debug.Log("Added 800 pearls for testing!");
      }
   }

   private void Awake() {

        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        pearlCount = MIN_PEARL_COUNT;
        crystalCount = MIN_CRYSTAL_COUNT;
    }

    public void TryAddPearl(int pearlAmount) {
        if (pearlCount > MAX_PEARL_COUNT) {
            Debug.LogError("Pearl count is at maximum!");
            return;
        } else if ((pearlCount + pearlAmount) > MAX_PEARL_COUNT) {
            Debug.LogError("Pearl count is at maximum!");
        } else
            pearlCount += pearlAmount;

        OnPearlCountChanged?.Invoke(pearlCount);

        return;
    }
    public void TrySpendPearl(int pearlAmount) {
        if (pearlCount < MIN_PEARL_COUNT) {
            Debug.LogError("Pearl count is at minimum!");
            return;
        } else if (pearlCount < pearlAmount) {
            Debug.LogError("Not enough pearls to spend!");
            return;
        } else
            pearlCount -= pearlAmount;

        OnPearlCountChanged?.Invoke(pearlCount);

        return;
    }
    public void TryAddCrystal(int pearlAmount) {
        if (pearlCount > MAX_PEARL_COUNT) {
            Debug.LogError("Pearl count is at maximum!");
            return;
        } else if ((pearlCount + pearlAmount) > MAX_PEARL_COUNT) {
            Debug.LogError("Pearl count is at maximum!");
        } else
            pearlCount += pearlAmount;

        OnCrystalCountChanged?.Invoke(crystalCount);

        return;
    }
    public void TrySpendCrystal(int pearlAmount) {
        if (pearlCount < MIN_PEARL_COUNT) {
            Debug.LogError("Pearl count is at minimum!");
            return;
        } else if (pearlCount < pearlAmount) {
            Debug.LogError("Not enough pearls to spend!");
            return;
        } else
            pearlCount -= pearlAmount;

        OnCrystalCountChanged?.Invoke(crystalCount);

        return;
    }

    public Action<int> OnPearlCountChanged;
    public Action<int> OnCrystalCountChanged;
    public Action<int> OnOreCountChanged;
}
