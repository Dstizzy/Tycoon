using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
   [SerializeField] private GameObject[] tutorialSections; // Array to hold all tutorial sections for easy management
   [SerializeField] private GameObject   turnButton;       // Reference to the button that must be clicked to proceed


   private static int  tutorialIndex   = 0;           // To track the current tutorial section
   private static int  sectionIndex    = 0;           // To track the current section within a tutorial
   public         bool requiredButtonClicked = true;  // Flag to check if the required button has been clicked
   public         bool oreRefineryUpgrade    = false; // Flag to check if the ore refinery upgrade has been completed
   public         bool forgeFunction         = false; // Checks if the forge function has been explained
   public         bool tradeHutFunction      = false; // Checks if the trade hut function has been explained
   public         bool explorationFunction   = false; // Checks if the exploration function has been explained
   public         bool labFunction           = false; // Checks if the lab function has been explained
   public         bool enemyFunction         = false; // Checks if the enemy function has been explained
   public         bool victoryFunction       = false; // Checks if the victory function has been explained
   public         bool oreUpgradeButton      = false; // Flag to check if the ore refinery upgrade button has been clicked
   public         bool tutorialGoing         = true;  // Flag to check if the tutorial is still going


   public static TutorialManager Instance { get; private set; }

   private void OnEnable()
   {
      PopUpManager.OnHoverTagChanged += HandleGlobalHover;
      OreRefinery_Manager.HandleTutorial += HandleNextStep;
      ForgeManager.HandleTutorial += HandleNextStep;
      InventoryManager.HandleTutorial += HandleNextStep;
   }

   private void OnDisable()
   {
      PopUpManager.OnHoverTagChanged -= HandleGlobalHover;
      OreRefinery_Manager.HandleTutorial -= HandleNextStep;
      ForgeManager.HandleTutorial -= HandleNextStep;
      InventoryManager.HandleTutorial -= HandleNextStep;
   }

   // Store the reference to the current part so we can toggle arrows from the event
   public GameObject currentActivePart;

   // Awake is called when the script instance is being loaded
   private void Awake()
   {
      while (tutorialIndex < tutorialSections.Length)
      {
         if (tutorialSections[tutorialIndex] == null)
         {
            Debug.LogError($"Tutorial section at index {tutorialIndex} is not assigned in the inspector.");
            return;
         }
         else
         {
            tutorialSections[tutorialIndex].SetActive(false);
         }
         tutorialIndex++;
      }
   }

   // Start is called before the first frame update
   private void Start()
   {
      tutorialIndex = 0;
      sectionIndex = 1;
      tutorialSections[tutorialIndex].SetActive(true);
      GoThroughSection(tutorialSections[tutorialIndex], sectionIndex);

   }

   // Update is called once per frame
   private void Update()
   {
      if (requiredButtonClicked)
      {
         if(Mouse.current.leftButton.wasPressedThisFrame)
         {
            GoThroughSection(tutorialSections[tutorialIndex], sectionIndex);
            sectionIndex++;
         }
      }
   }

   // Method to manage the flow of the tutorial sections and their subsections
   private void GoThroughSection(GameObject tutorialSection, int sectionIndex)
   {
      switch(sectionIndex)
      {
         case 1:
            tutorialSection.transform.Find("FirstPart").gameObject.SetActive(true);
            DoAllChecks(tutorialSection.transform.Find("FirstPart").gameObject);
            break;
         case 2:
            if (tutorialSection.transform.Find("SecondPart") != null)
            {
               tutorialSection.transform.Find("FirstPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("SecondPart").gameObject.SetActive(true);
               DoAllChecks(tutorialSection.transform.Find("SecondPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 3:
            if (tutorialSection.transform.Find("ThirdPart") != null)
            {
               tutorialSection.transform.Find("SecondPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("ThirdPart").gameObject.SetActive(true);
               DoAllChecks(tutorialSection.transform.Find("ThirdPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 4:
            if (tutorialSection.transform.Find("FourthPart") != null)
            {
               tutorialSection.transform.Find("ThirdPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("FourthPart").gameObject.SetActive(true);
               DoAllChecks(tutorialSection.transform.Find("FourthPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 5:
            if (tutorialSection.transform.Find("FifthPart") != null)
            {
               tutorialSection.transform.Find("FourthPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("FifthPart").gameObject.SetActive(true);
               DoAllChecks(tutorialSection.transform.Find("FifthPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 6:
            if (tutorialSection.transform.Find("SixthPart") != null)
            {
               tutorialSection.transform.Find("FifthPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("SixthPart").gameObject.SetActive(true);
               DoAllChecks(tutorialSection.transform.Find("SixthPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 7:
            if (tutorialSection.transform.Find("SeventhPart") != null)
            {
               tutorialSection.transform.Find("SixthPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("SeventhPart").gameObject.SetActive(true);
               DoAllChecks(tutorialSection.transform.Find("SeventhPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
      }
   }

   // Method to transition to the next tutorial section
   public void GoToNext()
   {
      if (tutorialIndex < tutorialSections.Length - 1)
      {
         tutorialSections[tutorialIndex].SetActive(false);
         tutorialIndex++;
         tutorialSections[tutorialIndex].SetActive(true);
         sectionIndex = 1;
         GoThroughSection(tutorialSections[tutorialIndex], sectionIndex);
      }
      else
      {
         tutorialSections[tutorialIndex].SetActive(false);
      }
   }

   // Method to perform all necessary checks for the current tutorial part
   private void DoAllChecks(GameObject myPart)
   {
      if(myPart.transform.Find("Turn") != null)
      {
         requiredButtonClicked = false;
         turnButton.GetComponent<Button>().onClick.AddListener(HandleTurn);
      }

      if(myPart.transform.Find("OreRefinery") != null)
      {
         requiredButtonClicked = false;
         oreRefineryUpgrade = true;
         currentActivePart = myPart;
      }

      if(myPart.transform.Find("Forge") != null)
      {
         requiredButtonClicked = false;
         forgeFunction = true;
         currentActivePart = myPart;
      }

      if(myPart.transform.Find("Inventory") != null)
      {
         requiredButtonClicked = false;
         InventoryManager.Instance.tutorialFunction = true;
         currentActivePart = myPart;
      }

      if(myPart.transform.Find("TradeHut") != null)
      {
         requiredButtonClicked = false;
         tradeHutFunction = true;
         currentActivePart = myPart;
      }
   }

   public void HandleTurn()
   {
      requiredButtonClicked = true;
      if (OreRefinery_Manager.Instance.IsBlocked == true)
      {
         OreRefinery_Manager.Instance.IsBlocked = false;
      }
      turnButton.GetComponent<Button>().onClick.RemoveListener(HandleTurn);
      GoThroughSection(tutorialSections[tutorialIndex], sectionIndex++);
   }

   // Method to handle global hover events and toggle arrows based on the current tutorial part
   public void HandleGlobalHover(string tag)
   {
      if(oreRefineryUpgrade == true)
      {
         if(tag == "Ore Refinery")
         {
            currentActivePart.transform.Find("Arrow").gameObject.SetActive(true);
            OreRefinery_Manager.Instance.tutorialUpgrade = true;
         }
         else
         {
            currentActivePart.transform.Find("Arrow").gameObject.SetActive(false);
         }
      }

      if(forgeFunction == true)
      {
         if(tag == "Forge")
         {
            currentActivePart.transform.Find("Arrow").gameObject.SetActive(true);
            ForgeManager.Instance.tutorialFunction = true;
         }
         else
         {
            currentActivePart.transform.Find("Arrow").gameObject.SetActive(false);
         }
      }

      if(tradeHutFunction == true)
      {
         if(tag == "Trade Hut")
         {
            currentActivePart.transform.Find("Arrow").gameObject.SetActive(true);
            TradeHutManager.Instance.tutorialFunction = true;
         }
         else
         {
            currentActivePart.transform.Find("Arrow").gameObject.SetActive(false);
         }
      }

      if(explorationFunction == true)
      {
         if(tag == "Exploration")
         {
            currentActivePart.transform.Find("Arrow").gameObject.SetActive(true);
            ExplorationUnitManager.Instance.tutorialFunction = true;
         }
         else
         {
            currentActivePart.transform.Find("Arrow").gameObject.SetActive(false);
         }
      }

      if(labFunction == true)
      {
         if(tag == "Lab")
         {
            currentActivePart.transform.Find("Arrow").gameObject.SetActive(true);
            LabManager.Instance.tutorialFunction = true;
         }
         else
         {
            currentActivePart.transform.Find("Arrow").gameObject.SetActive(false);
         }
      }

      if(enemyFunction == true)
      {
         if(tag == "Enemy")
         {
            currentActivePart.transform.Find("Arrow").gameObject.SetActive(true);
            TurnManager.Instance.tutorialFunction = true;
         }
         else
         {
            currentActivePart.transform.Find("Arrow").gameObject.SetActive(false);
         }
      }

      if(victoryFunction == true)
      {
         if(tag == "Victory")
         {
            currentActivePart.transform.Find("Arrow").gameObject.SetActive(true);
         }
         else
         {
            currentActivePart.transform.Find("Arrow").gameObject.SetActive(false);
         }
      }


   }

   // Method to handle the next step in the tutorial when the required button is clicked
   public void HandleNextStep()
   {
      requiredButtonClicked = true;
      if(oreRefineryUpgrade == true)
      {
         oreRefineryUpgrade = false;
      }

      GoThroughSection(tutorialSections[tutorialIndex], sectionIndex++);
   }
}
