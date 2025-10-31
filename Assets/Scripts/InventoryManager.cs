using System;

using UnityEngine;

public class InventoryManager : MonoBehaviour {

    public static InventoryManager Instance { get; private set; }

    const int MIN_PEARL_COUNT = 0;
    const int MAX_PEARL_COUNT = 1000;
    const int MIN_CRYSTAL_COUNT = MIN_PEARL_COUNT;
    const int MAX_CRYSTAL_COUNT = MAX_PEARL_COUNT;
    const int MIN_ORE_COUNT = MIN_PEARL_COUNT;
    const int MAX_ORE_COUNT = MAX_PEARL_COUNT;


    public int pearlCount;
    public int crystalCount;
    public int oreCount = 1000;

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
    public void TryAddCrystal(int crystalAmount) {
        if (crystalCount > MAX_CRYSTAL_COUNT) {
            Debug.LogError("Crystal count is at maximum!");
            return;
        } else if ((crystalCount + crystalAmount) > MAX_CRYSTAL_COUNT) {
            Debug.LogError("Crystal count is at maximum!");
        } else
            crystalCount += crystalAmount;

        OnCrystalCountChanged?.Invoke(crystalCount);

        return;
    }
    public void TrySpendCrystal(int crystalAmount) {
        if (crystalCount < MIN_CRYSTAL_COUNT) {
            Debug.LogError("Crystal count is at minimum!");
            return;
        } else if (crystalCount < crystalAmount) {
            Debug.LogError("Not enough crystals to spend!");
            return;
        } else
            crystalCount -= crystalAmount;

        OnCrystalCountChanged?.Invoke(crystalCount);

        return;
    }
    public void TryAddOre(int oreAmount)
    {
        if (oreCount > MAX_ORE_COUNT)
        {
            Debug.LogError("Ore count is at maximum!");
            return;
        }
        else if ((oreCount + oreAmount) > MAX_ORE_COUNT)
        {
            Debug.LogError("Ore count is at maximum!");
        }
        else
            oreCount += oreAmount;

        OnOreCountChanged?.Invoke(oreCount);

        return;
    }
    public void TrySpendOre(int oreAmount)
    {
        if (oreCount < MIN_ORE_COUNT)
        {
            Debug.LogError("Ore count is at minimum!");
            return;
        }
        else if (oreCount < oreAmount)
        {
            Debug.LogError("Not enough ore to spend!");
            return;
        }
        else
            oreCount -= oreAmount;

        OnOreCountChanged?.Invoke(oreCount);

        return;
    }

    public Action<int> OnPearlCountChanged;
    public Action<int> OnCrystalCountChanged;
    public Action<int> OnOreCountChanged;
}
