using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
   [SerializeField] private GameObject[] tutorialSections; // Array to hold all tutorial sections for easy management
   [SerializeField] private GameObject turnButton;         // Reference to the button that must be clicked to proceed


   private static int  tutorialIndex   = 0;           // To track the current tutorial section
   private static int  sectionIndex    = 0;           // To track the current section within a tutorial
   public         bool requiredButtonClicked = true;  // Flag to check if the required button has been clicked
   public         bool oreRefineryUpgrade    = false; // Flag to check if the ore refinery upgrade has been completed
   public         bool oreUpgradeButton      = false; // Flag to check if the ore refinery upgrade button has been clicked
   public         bool tutorialGoing         = true;  // Flag to check if the tutorial is still going


   public static TutorialManager Instance { get; private set; }

   private void OnEnable()
   {
      PopUpManager.OnHoverTagChanged += HandleGlobalHover;
      OreRefinery_Manager.HandleTutorial += HandleNextStep;
   }

   private void OnDisable()
   {
      PopUpManager.OnHoverTagChanged -= HandleGlobalHover;
      OreRefinery_Manager.HandleTutorial -= HandleNextStep;
   }

   // Store the reference to the current part so we can toggle arrows from the event
   public GameObject currentActivePart;

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

   private void Start()
   {
      //panel.SetActive(true);
      tutorialIndex = 0;
      sectionIndex = 1;
      tutorialSections[tutorialIndex].SetActive(true);
      //GoThroughSection(tutorialSections[tutorialIndex], sectionIndex);

   }

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
      }
   }

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

   private void DoAllChecks(GameObject myPart)
   {
      if(myPart.transform.Find("Screen") != null)
      {
         requiredButtonClicked = false;
         turnButton.GetComponent<Button>().onClick.AddListener(() => 
         {
            requiredButtonClicked = true;
            if(OreRefinery_Manager.Instance.IsBlocked == true)
            {
               OreRefinery_Manager.Instance.IsBlocked = false;
            }
            GoThroughSection(tutorialSections[tutorialIndex], sectionIndex++);
         });
      }

      if(myPart.transform.Find("OreRefinery") != null)
      {
         requiredButtonClicked = false;
         oreRefineryUpgrade = true;
         currentActivePart = myPart;
      }
   }

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
   }

   public void HandleNextStep()
   {
      GoThroughSection(tutorialSections[tutorialIndex], sectionIndex++);
      requiredButtonClicked = true;
   }
}
