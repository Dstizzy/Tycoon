using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
   [SerializeField] private GameObject[] tutorialSections;  // Array to hold all tutorial sections for easy management
   [SerializeField] private GameObject   oreRefineryCanvas; // building canvas of the ore refinery
   [SerializeField] private GameObject   forgeCanvas;       // building canvas of the forge
   [SerializeField] private GameObject   tradeHutCanvas;    // building canvas of the trade hut
   [SerializeField] private GameObject   explorationCanvas; // building canvas of the exploration
   [SerializeField] private GameObject   labCanvas;         // building canvas of the lab
   [SerializeField] private GameObject   turnButton;        // Reference to the button that must be clicked to proceed


   public int  tutorialIndex   = 0;           // To track the current tutorial section
   private static int  sectionIndex    = 0;           // To track the current section within a tutorial
   public         bool requiredButtonClicked = true;  // Flag to check if the required button has been clicked
   public         bool oreRefineryUpgrade    = false; // Flag to check if the ore refinery upgrade has been completed
   public         bool forgeFunction         = false; // Checks if the forge function has been explained
   public         bool tradeHutFunction      = false; // Checks if the trade hut function has been explained
   public         bool tradeHutFunctionTwo   = false; // Checks if the second trade hut function has been explained
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
      ForgeManager.HandleTutorial += HandleNextStep;
      TradeHutManager.HandleTutorial += HandleNextStep;
   }

   private void OnDisable()
   {
      PopUpManager.OnHoverTagChanged -= HandleGlobalHover;
      ForgeManager.HandleTutorial -= HandleNextStep;
      TradeHutManager.HandleTutorial -= HandleNextStep;
   }

   // Store the reference to the current part so we can toggle arrows from the event
   public GameObject currentActivePart = null;
   public GameObject currentActiveSection = null;

   // Event to start tutorial parts that can only be handled in other managers
   public static event Action HandleForgeTutorial;
   public static event Action<int> HandleTradeHutTutorial;

   // Start is called before the first frame update
   private void Start()
   {
      // =====================================================================
      // [ADDED] Disable legacy tutorial by default
      // =====================================================================
      if (TutorialFlowSettings.UseLegacyTutorial == false)
      {
         gameObject.SetActive(false);
         return;
      }
      // =====================================================================
      // [END ADDED]
      // =====================================================================

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
            if(currentActivePart != null)
            {
               currentActivePart.transform.Find("Arrow").gameObject.SetActive(false);
               currentActivePart = null;
            }
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
         case 8:
            if (tutorialSection.transform.Find("EighthPart") != null)
            {
               tutorialSection.transform.Find("SeventhPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("EighthPart").gameObject.SetActive(true);
               DoAllChecks(tutorialSection.transform.Find("EighthPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 9:
            if (tutorialSection.transform.Find("NinthPart") != null)
            {
               tutorialSection.transform.Find("EighthPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("NinthPart").gameObject.SetActive(true);
               DoAllChecks(tutorialSection.transform.Find("NinthPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 10:
            if (tutorialSection.transform.Find("TenthPart") != null)
            {
               tutorialSection.transform.Find("NinthPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("TenthPart").gameObject.SetActive(true);
               DoAllChecks(tutorialSection.transform.Find("TenthPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 11:
            if (tutorialSection.transform.Find("EleventhPart") != null)
            {
               tutorialSection.transform.Find("TenthPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("EleventhPart").gameObject.SetActive(true);
               DoAllChecks(tutorialSection.transform.Find("EleventhPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 12:
            if (tutorialSection.transform.Find("TwelvthPart") != null)
            {
               tutorialSection.transform.Find("EleventhPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("TwelvthPart").gameObject.SetActive(true);
               DoAllChecks(tutorialSection.transform.Find("TwelvthPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 13:
            if (tutorialSection.transform.Find("ThirteenthPart") != null)
            {
               tutorialSection.transform.Find("TwelvthPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("ThirteenthPart").gameObject.SetActive(true);
               DoAllChecks(tutorialSection.transform.Find("ThirteenthPart").gameObject);
            }
            else
            {
               GoToNext();
            }
            break;
         case 14:
            if (tutorialSection.transform.Find("FourteenthPart") != null)
            {
               tutorialSection.transform.Find("ThirteenthPart").gameObject.SetActive(false);
               tutorialSection.transform.Find("FourteenthPart").gameObject.SetActive(true);
               DoAllChecks(tutorialSection.transform.Find("FourteenthPart").gameObject);
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
      requiredButtonClicked = false;
      currentActiveSection = myPart;
      if (myPart.transform.Find("OreRefinery") != null)
      {
         oreRefineryCanvas.transform.Find("Arrow").gameObject.SetActive(true);
         currentActivePart = oreRefineryCanvas;
         requiredButtonClicked = true;
      }
      if(myPart.transform.Find("Forge") != null)
      {
         forgeCanvas.transform.Find("Arrow").gameObject.SetActive(true);
         currentActivePart = forgeCanvas;
         requiredButtonClicked = true;
      }
      if(myPart.transform.Find("TradeHut") != null)
      {
         tradeHutCanvas.transform.Find("Arrow").gameObject.SetActive(true);
         currentActivePart = tradeHutCanvas;
         requiredButtonClicked = true;
      }
      if(myPart.transform.Find("Exploration") != null)
      {
         explorationCanvas.transform.Find("Arrow").gameObject.SetActive(true);
         currentActivePart = explorationCanvas;
         requiredButtonClicked = true;
      }
      if(myPart.transform.Find("Lab") != null)
      {
         labCanvas.transform.Find("Arrow").gameObject.SetActive(true);
         currentActivePart = labCanvas;
         requiredButtonClicked = true;
      }
      if(myPart.transform.Find("Turn") != null)
      {
         turnButton.GetComponent<Button>().onClick.AddListener(HandleTurn);
      }
      if(myPart.transform.Find("ForgeExample") != null)
      {
         forgeCanvas.transform.Find("Arrow2").gameObject.SetActive(true);
         forgeCanvas.transform.Find("HoverHere").gameObject.SetActive(true);
         forgeFunction = true;
         HandleForgeTutorial?.Invoke();
      }
      if(myPart.transform.Find("TradeHutExample") != null)
      {
         tradeHutCanvas.transform.Find("Arrow").gameObject.SetActive(true);
         tradeHutCanvas.transform.Find("HoverHere").gameObject.SetActive(true);
         tradeHutFunction = true;
         HandleTradeHutTutorial?.Invoke(1);
      }
      if(myPart.transform.Find("Turn") == null && myPart.transform.Find("ForgeExample") == null 
         && myPart.transform.Find("TradeHutExample") == null && requiredButtonClicked == false)
      {
         currentActivePart = null;
         requiredButtonClicked = true;
      }
   }   

   public void HandleTurn()
   {
      Debug.Log("hello");
      requiredButtonClicked = true;
      currentActivePart = null;
      turnButton.GetComponent<Button>().onClick.RemoveListener(HandleTurn);
      GoThroughSection(tutorialSections[tutorialIndex], sectionIndex++);
   }

   // Method to handle global hover events and toggle arrows based on the current tutorial part
   public void HandleGlobalHover(string tag)
   {
      if(forgeFunction == true)
      {
         if(tag == "Forge")
         {
            forgeCanvas.transform.Find("Arrow3").gameObject.SetActive(true);
         }
         else
         {
            forgeCanvas.transform.Find("Arrow3").gameObject.SetActive(false);
         }
      }

      if(tradeHutFunction == true)
      {
         if(tag == "Trade Hut")
         {
            tradeHutCanvas.transform.Find("Arrow2").gameObject.SetActive(true);
         }
         else
         {
            tradeHutCanvas.transform.Find("Arrow2").gameObject.SetActive(false);
         }
      }
   }

   // Method to handle the next step in the tutorial when the required button is clicked
   public void HandleNextStep()
   {
      requiredButtonClicked = true;
      if(forgeFunction == true)
      {
         forgeFunction = false;
         forgeCanvas.transform.Find("Arrow2").gameObject.SetActive(false);
         forgeCanvas.transform.Find("HoverHere").gameObject.SetActive(false);
         forgeCanvas.transform.Find("Arrow3").gameObject.SetActive(false);
      }
      if(tradeHutFunction == true)
      {
         tradeHutFunction = false;
         tradeHutCanvas.transform.Find("Arrow").gameObject.SetActive(false);
         tradeHutCanvas.transform.Find("HoverHere").gameObject.SetActive(false);
         tradeHutCanvas.transform.Find("Arrow2").gameObject.SetActive(false);

      }

      GoThroughSection(tutorialSections[tutorialIndex], sectionIndex++);
   }
}