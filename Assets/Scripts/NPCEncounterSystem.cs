using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Roguelike-style NPC encounter system without LLM.
/// Uses weighted random outcomes for unpredictable rewards/penalties.
/// </summary>
public class NPCEncounterSystem : MonoBehaviour
{
   public static NPCEncounterSystem Instance { get; private set; }

   #region Constants - Reward/Penalty Values
   private const int SMALL_PEARL_REWARD = 20;
   private const int MEDIUM_PEARL_REWARD = 50;
   private const int LARGE_PEARL_REWARD = 100;
   private const int HUGE_PEARL_REWARD = 150;

   private const int SMALL_PEARL_COST = 10;
   private const int MEDIUM_PEARL_COST = 30;
   private const int LARGE_PEARL_COST = 50;
   private const int HUGE_PEARL_COST = 100;

   private const int SMALL_PEARL_PENALTY = 20;
   private const int MEDIUM_PEARL_PENALTY = 50;
   private const int LARGE_PEARL_PENALTY = 80;

   private const int SMALL_ORE_COST = 15;
   private const int MEDIUM_ORE_COST = 30;
   private const int LARGE_ORE_COST = 50;

   private const int TINY_BONUS = 5;
   private const int SMALL_BONUS = 10;

   private const int HIGH_SUCCESS_WEIGHT = 70;
   private const int MEDIUM_SUCCESS_WEIGHT = 50;
   private const int LOW_SUCCESS_WEIGHT = 35;
   private const int GUARANTEED_WEIGHT = 100;

   private const int DEFAULT_SPAWN_CHANCE = 25;
   #endregion

   #region NPC Data Definitions
   [System.Serializable]
   public class NPCProfile
   {
      public string npcName;
      public Sprite mapSprite;
      public Sprite portraitNeutral;
      public Sprite portraitHappy;
      public Sprite portraitAngry;
      public Sprite portraitSurprised;
      public Sprite portraitThinking;
      public Sprite portraitSpecial;    // Unique expression per character
      public NPCPersonality personality;
   }

   public enum NPCPersonality
   {
      HermitCrab,  // TradeHut - Pearl-adorned money lover
      Turtle,      // Lab - Flask-throwing mad scientist
      Jellyfish,   // Refinery - Bioluminescent when happy
      Seahorse,    // Exploration - Shadowed serious face
      Octopus,     // Forge - Self-absorbed expression
      Dolphin      // Manager - Bleached exhaustion
   }

   public enum ExpressionType
   {
      Neutral,
      Happy,
      Angry,
      Surprised,
      Thinking,
      Special      // Unique per character
   }

   [System.Serializable]
   public class DialogueLine
   {
      public string text;
      public ExpressionType expression;

      public DialogueLine(string text, ExpressionType expression = ExpressionType.Neutral)
      {
         this.text = text;
         this.expression = expression;
      }
   }

   [System.Serializable]
   public class EncounterOutcome
   {
      public DialogueLine[] resultDialogues;
      public int pearlChange;
      public int oreCost;
      public bool isPositive;

      // =====================================================================
      // [ADDED] Default constructor for object initializer syntax
      // =====================================================================
      public EncounterOutcome()
      {
      }
      // =====================================================================
      // [END ADDED]
      // =====================================================================

      // Convenience constructor for single-line outcomes
      public EncounterOutcome(string singleLine, ExpressionType expr, int pearl, bool positive)
      {
         resultDialogues = new DialogueLine[] { new DialogueLine(singleLine, expr) };
         pearlChange = pearl;
         isPositive = positive;
      }
   }

   [System.Serializable]
   public class EncounterScenario
   {
      public DialogueLine[] openingDialogues;
      public Choice[] choices;
   }

   [System.Serializable]
   public class Choice
   {
      public string choiceText;
      public EncounterOutcome[] outcomes;
      public int[] outcomeWeights;
      public int oreCost;
      public int pearlCost;
   }
   #endregion

   #region Inspector Settings
   [Header("NPC Profiles (6 Characters)")]
   [SerializeField] private NPCProfile[] npcProfiles = new NPCProfile[6];

   [Header("Spawn Settings")]
   [SerializeField] private Transform[] buildingLocations;
   [SerializeField] private GameObject npcPrefab;
   [SerializeField] private Vector3 spawnOffset = new Vector3(2f, 1f, 0f);
   [SerializeField, Range(0, 100)] private int spawnChancePerTurn = DEFAULT_SPAWN_CHANCE;

   [Header("UI References")]
   [SerializeField] private NPCDialogueUI dialogueUI;
   #endregion

   #region Runtime Variables
   private GameObject currentNPCObject;
   private NPCProfile currentNPC;
   private EncounterScenario currentScenario;
   private bool hasActiveNPC = false;

   private Dictionary<NPCPersonality, EncounterScenario[]> scenarioDatabase;
   #endregion

   #region Unity Lifecycle
   private void Awake()
   {
      if (Instance != null && Instance != this)
      {
         Destroy(gameObject);
         return;
      }
      Instance = this;

      InitializeScenarioDatabase();
   }

   private void Start()
   {
      TurnManager.OnTurnEnded += OnTurnEnded;
   }

   private void OnDestroy()
   {
      TurnManager.OnTurnEnded -= OnTurnEnded;
   }
   #endregion

   #region Scenario Database Initialization
   private void InitializeScenarioDatabase()
   {
      scenarioDatabase = new Dictionary<NPCPersonality, EncounterScenario[]>
      {
         // ==================== HERMIT CRAB - TradeHut ====================
         // Special Expression: Pearl-adorned money-obsessed face
         [NPCPersonality.HermitCrab] = new EncounterScenario[]
          {
                // Scenario 1: Investment Opportunity
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Ah, a visitor.", ExpressionType.Neutral),
                        new DialogueLine("Monocle adjusted. Pearls gleaming.", ExpressionType.Thinking),
                        new DialogueLine("You carry yourself well. A person of taste, I presume?", ExpressionType.Neutral),
                        new DialogueLine("I have a rather... lucrative proposition for you.", ExpressionType.Happy),
                        new DialogueLine("Care to invest in a gentleman's venture?", ExpressionType.Special)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = $"Invest ({MEDIUM_PEARL_COST} Pearls)",
                            pearlCost = MEDIUM_PEARL_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Splendid decision, old chap!", ExpressionType.Happy),
                                        new DialogueLine("Pearls counted with visible satisfaction.", ExpressionType.Special),
                                        new DialogueLine("The returns have exceeded expectations.", ExpressionType.Neutral),
                                        new DialogueLine("Here is your share. Well earned, I must say.", ExpressionType.Happy)
                                    },
                                    pearlChange = MEDIUM_PEARL_REWARD + SMALL_BONUS,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Hmm...", ExpressionType.Thinking),
                                        new DialogueLine("Eye contact avoided.", ExpressionType.Neutral),
                                        new DialogueLine("I regret to inform you... the market was not kind.", ExpressionType.Angry),
                                        new DialogueLine("My sincerest apologies. These things happen.", ExpressionType.Neutral)
                                    },
                                    pearlChange = -MEDIUM_PEARL_COST,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { 55, 45 }
                        },
                        new Choice
                        {
                            choiceText = "Politely decline",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Quite understandable.", ExpressionType.Neutral),
                                        new DialogueLine("Prudence is the mark of a refined individual.", ExpressionType.Thinking),
                                        new DialogueLine("Do accept this small token of goodwill.", ExpressionType.Happy)
                                    },
                                    pearlChange = SMALL_BONUS,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        },
                        new Choice
                        {
                            choiceText = $"Large investment ({HUGE_PEARL_COST} Pearls)",
                            pearlCost = HUGE_PEARL_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Oh ho!", ExpressionType.Surprised),
                                        new DialogueLine("A bold move! I admire your courage!", ExpressionType.Happy),
                                        new DialogueLine("Shell stroked thoughtfully. Eyes gleaming with greed.", ExpressionType.Special),
                                        new DialogueLine("Fortune favors the bold indeed!", ExpressionType.Happy),
                                        new DialogueLine("Your substantial returns, good sir.", ExpressionType.Neutral)
                                    },
                                    pearlChange = HUGE_PEARL_REWARD + MEDIUM_PEARL_REWARD,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("...", ExpressionType.Neutral),
                                        new DialogueLine("Gaze fixed firmly on the floor.", ExpressionType.Thinking),
                                        new DialogueLine("This is... most unfortunate.", ExpressionType.Angry),
                                        new DialogueLine("A miscalculation on my part. Terribly sorry.", ExpressionType.Neutral)
                                    },
                                    pearlChange = -HUGE_PEARL_COST,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { LOW_SUCCESS_WEIGHT, 65 }
                        }
                    }
                },

                // Scenario 2: Ore Trading
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Pocket watch examined.", ExpressionType.Neutral),
                        new DialogueLine("Ah, perfect timing.", ExpressionType.Happy),
                        new DialogueLine("I've recently acquired some rather fine ore.", ExpressionType.Neutral),
                        new DialogueLine("Premium quality. Only the best for my collection.", ExpressionType.Thinking),
                        new DialogueLine("Interested in a trade, perhaps?", ExpressionType.Special)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = $"Buy ore ({MEDIUM_PEARL_COST} Pearls)",
                            pearlCost = MEDIUM_PEARL_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Excellent taste!", ExpressionType.Happy),
                                        new DialogueLine("Ore wrapped with meticulous care.", ExpressionType.Neutral),
                                        new DialogueLine("This piece brings good fortune, they say.", ExpressionType.Thinking),
                                        new DialogueLine("A pleasure doing business with you.", ExpressionType.Happy)
                                    },
                                    pearlChange = -MEDIUM_PEARL_COST,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Wait...", ExpressionType.Surprised),
                                        new DialogueLine("Ore inspected closely. Color draining from face.", ExpressionType.Thinking),
                                        new DialogueLine("Good heavens! This appears to be counterfeit!", ExpressionType.Angry),
                                        new DialogueLine("My deepest apologies. Full refund, of course.", ExpressionType.Neutral)
                                    },
                                    pearlChange = 0,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { HIGH_SUCCESS_WEIGHT, 30 }
                        },
                        new Choice
                        {
                            choiceText = "Negotiate the price",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Ho ho! A negotiator!", ExpressionType.Surprised),
                                        new DialogueLine("I respect a sharp business mind.", ExpressionType.Thinking),
                                        new DialogueLine("Very well. A special price for you.", ExpressionType.Happy)
                                    },
                                    pearlChange = SMALL_PEARL_REWARD,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Eyebrow raised.", ExpressionType.Neutral),
                                        new DialogueLine("A gentleman does not haggle excessively.", ExpressionType.Thinking),
                                        new DialogueLine("I'm afraid we cannot reach an agreement.", ExpressionType.Angry)
                                    },
                                    pearlChange = 0,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { MEDIUM_SUCCESS_WEIGHT, 50 }
                        },
                        new Choice
                        {
                            choiceText = "Not interested",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Very well.", ExpressionType.Neutral),
                                        new DialogueLine("I respect a person who knows their own mind.", ExpressionType.Thinking),
                                        new DialogueLine("Until we meet again.", ExpressionType.Neutral)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                },

                // Scenario 3: Gentleman's Wager
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("A soft chuckle.", ExpressionType.Happy),
                        new DialogueLine("You know, I find myself in excellent spirits today.", ExpressionType.Neutral),
                        new DialogueLine("Care for a gentleman's wager?", ExpressionType.Thinking),
                        new DialogueLine("A simple coin toss. Nothing crude, I assure you.", ExpressionType.Neutral),
                        new DialogueLine("Pearls practically dancing on the shell.", ExpressionType.Special)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = $"Heads ({MEDIUM_PEARL_COST} Pearls)",
                            pearlCost = MEDIUM_PEARL_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Coin flipped with practiced elegance.", ExpressionType.Neutral),
                                        new DialogueLine("...", ExpressionType.Thinking),
                                        new DialogueLine("Heads! Fortune smiles upon you!", ExpressionType.Happy),
                                        new DialogueLine("A worthy opponent. Here are your winnings.", ExpressionType.Neutral)
                                    },
                                    pearlChange = MEDIUM_PEARL_REWARD + SMALL_PEARL_REWARD,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Coin flipped with practiced elegance.", ExpressionType.Neutral),
                                        new DialogueLine("...", ExpressionType.Thinking),
                                        new DialogueLine("Tails, I'm afraid.", ExpressionType.Neutral),
                                        new DialogueLine("Better luck next time, old sport.", ExpressionType.Happy)
                                    },
                                    pearlChange = -MEDIUM_PEARL_COST,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { MEDIUM_SUCCESS_WEIGHT, 50 }
                        },
                        new Choice
                        {
                            choiceText = $"Tails ({MEDIUM_PEARL_COST} Pearls)",
                            pearlCost = MEDIUM_PEARL_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Coin flipped with a flourish.", ExpressionType.Neutral),
                                        new DialogueLine("And the result is...", ExpressionType.Thinking),
                                        new DialogueLine("Tails! Magnificent intuition!", ExpressionType.Surprised),
                                        new DialogueLine("Your winnings, as promised.", ExpressionType.Happy)
                                    },
                                    pearlChange = MEDIUM_PEARL_REWARD + SMALL_PEARL_REWARD,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Coin flipped with a flourish.", ExpressionType.Neutral),
                                        new DialogueLine("And the result is...", ExpressionType.Thinking),
                                        new DialogueLine("Heads. Not your day, it seems.", ExpressionType.Neutral),
                                        new DialogueLine("Perhaps fortune will favor you next time.", ExpressionType.Thinking)
                                    },
                                    pearlChange = -MEDIUM_PEARL_COST,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { MEDIUM_SUCCESS_WEIGHT, 50 }
                        },
                        new Choice
                        {
                            choiceText = "Decline the wager",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Ah, a cautious soul.", ExpressionType.Neutral),
                                        new DialogueLine("Wisdom is its own reward.", ExpressionType.Thinking),
                                        new DialogueLine("Hat tipped graciously.", ExpressionType.Happy),
                                        new DialogueLine("I admire your restraint.", ExpressionType.Neutral)
                                    },
                                    pearlChange = TINY_BONUS,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                }
          },

         // ==================== TURTLE - Lab ====================
         // Special Expression: Flask-throwing maniacal laughter
         [NPCPersonality.Turtle] = new EncounterScenario[]
          {
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Ooh! Perfect timing!", ExpressionType.Surprised),
                        new DialogueLine("Beakers crashing to the floor.", ExpressionType.Surprised),
                        new DialogueLine("Wanna help with this experiment?", ExpressionType.Happy),
                        new DialogueLine("It's probably... maybe... not dangerous!", ExpressionType.Special)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = "Participate",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Here we go!", ExpressionType.Happy),
                                        new DialogueLine("Chemicals mixed with reckless abandon.", ExpressionType.Thinking),
                                        new DialogueLine("WOOHOO! Success!", ExpressionType.Special),
                                        new DialogueLine("Here's your share!", ExpressionType.Happy)
                                    },
                                    pearlChange = LARGE_PEARL_REWARD,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Here we go!", ExpressionType.Happy),
                                        new DialogueLine("Chemicals mixed with reckless abandon.", ExpressionType.Thinking),
                                        new DialogueLine("BOOM!", ExpressionType.Surprised),
                                        new DialogueLine("...You okay? Hehe, miscalculated!", ExpressionType.Special)
                                    },
                                    pearlChange = -SMALL_PEARL_PENALTY,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { 45, 55 }
                        },
                        new Choice
                        {
                            choiceText = "Decline",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Aww~", ExpressionType.Neutral),
                                        new DialogueLine("No fun!", ExpressionType.Angry),
                                        new DialogueLine("Next time for sure!", ExpressionType.Happy)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        },
                        new Choice
                        {
                            choiceText = "Ask what the experiment is",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Uhh...", ExpressionType.Thinking),
                                        new DialogueLine("What was it again?", ExpressionType.Surprised),
                                        new DialogueLine("Oh! I remember!", ExpressionType.Happy),
                                        new DialogueLine("But explaining ruins the fun~", ExpressionType.Special)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                },
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Hey hey hey!", ExpressionType.Happy),
                        new DialogueLine("Check this out!", ExpressionType.Surprised),
                        new DialogueLine("My invention!", ExpressionType.Happy),
                        new DialogueLine("I don't know what it does either!", ExpressionType.Special),
                        new DialogueLine("Wanna buy it?", ExpressionType.Happy)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = $"Buy it ({SMALL_PEARL_COST} Pearls)",
                            pearlCost = SMALL_PEARL_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Button pressed.", ExpressionType.Thinking),
                                        new DialogueLine("Oh! It works!", ExpressionType.Surprised),
                                        new DialogueLine("It was an ore detector!", ExpressionType.Happy)
                                    },
                                    pearlChange = MEDIUM_PEARL_REWARD,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Button pressed.", ExpressionType.Thinking),
                                        new DialogueLine("...Huh? Smoke?", ExpressionType.Surprised),
                                        new DialogueLine("RUN!", ExpressionType.Special)
                                    },
                                    pearlChange = -SMALL_PEARL_COST,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { MEDIUM_SUCCESS_WEIGHT, 50 }
                        },
                        new Choice
                        {
                            choiceText = "Don't buy",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Hmph!", ExpressionType.Angry),
                                        new DialogueLine("Nobody appreciates my genius!", ExpressionType.Angry)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        },
                        new Choice
                        {
                            choiceText = "Tell them to test it first",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Good idea!", ExpressionType.Happy),
                                        new DialogueLine("Button pressed.", ExpressionType.Thinking),
                                        new DialogueLine("BOOM!", ExpressionType.Surprised),
                                        new DialogueLine("...Ow.", ExpressionType.Neutral),
                                        new DialogueLine("But this is great data! Thanks!", ExpressionType.Special)
                                    },
                                    pearlChange = SMALL_PEARL_REWARD,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                },
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Oh!", ExpressionType.Surprised),
                        new DialogueLine("I needed ore samples!", ExpressionType.Happy),
                        new DialogueLine("Can you spare some?", ExpressionType.Neutral)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = $"Give ore ({MEDIUM_ORE_COST} Ore)",
                            oreCost = MEDIUM_ORE_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Thanks!", ExpressionType.Happy),
                                        new DialogueLine("Here, take this!", ExpressionType.Happy),
                                        new DialogueLine("Don't know what it is...", ExpressionType.Thinking),
                                        new DialogueLine("But it looks expensive!", ExpressionType.Special)
                                    },
                                    pearlChange = LARGE_PEARL_REWARD,
                                    oreCost = MEDIUM_ORE_COST,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Thanks!", ExpressionType.Happy),
                                        new DialogueLine("Uhh...", ExpressionType.Thinking),
                                        new DialogueLine("Nothing to give back.", ExpressionType.Neutral),
                                        new DialogueLine("Next time for sure!", ExpressionType.Happy)
                                    },
                                    pearlChange = SMALL_BONUS,
                                    oreCost = MEDIUM_ORE_COST,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { 60, 40 }
                        },
                        new Choice
                        {
                            choiceText = "Refuse",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Aww~", ExpressionType.Neutral),
                                        new DialogueLine("Stingy!", ExpressionType.Angry),
                                        new DialogueLine("Just kidding!", ExpressionType.Happy)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                }
          },

         // ==================== JELLYFISH - Refinery ====================
         // Special Expression: Bioluminescent glow (happy/excited)
         [NPCPersonality.Jellyfish] = new EncounterScenario[]
          {
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("...", ExpressionType.Neutral),
                        new DialogueLine("...Shiny...", ExpressionType.Thinking),
                        new DialogueLine("...Pretty thing...", ExpressionType.Happy),
                        new DialogueLine("A faint glow begins.", ExpressionType.Special)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = "Ask to see it",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("...This...", ExpressionType.Neutral),
                                        new DialogueLine("...For you...", ExpressionType.Happy),
                                        new DialogueLine("Glowing brightly now.", ExpressionType.Special)
                                    },
                                    pearlChange = MEDIUM_PEARL_REWARD,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("...Huh...?", ExpressionType.Surprised),
                                        new DialogueLine("...Where did it go...", ExpressionType.Thinking),
                                        new DialogueLine("The glow fades.", ExpressionType.Neutral)
                                    },
                                    pearlChange = 0,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { 65, 35 }
                        },
                        new Choice
                        {
                            choiceText = "Search together",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("...Together...", ExpressionType.Happy),
                                        new DialogueLine("...", ExpressionType.Thinking),
                                        new DialogueLine("...Found it...", ExpressionType.Surprised),
                                        new DialogueLine("Entire body illuminated with joy.", ExpressionType.Special)
                                    },
                                    pearlChange = MEDIUM_PEARL_REWARD + SMALL_BONUS,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        },
                        new Choice
                        {
                            choiceText = "Say you're busy",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("...Okay...", ExpressionType.Neutral),
                                        new DialogueLine("...Me too...", ExpressionType.Thinking),
                                        new DialogueLine("...Sleepy...", ExpressionType.Neutral)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                },
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("...Yawn...", ExpressionType.Neutral),
                        new DialogueLine("...Lots of ore here...", ExpressionType.Thinking),
                        new DialogueLine("...Help me mine...?", ExpressionType.Neutral)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = "Help",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("...Thank you...", ExpressionType.Happy),
                                        new DialogueLine("Slow, methodical mining.", ExpressionType.Neutral),
                                        new DialogueLine("...This is... yours...", ExpressionType.Special)
                                    },
                                    pearlChange = MEDIUM_PEARL_REWARD + SMALL_PEARL_REWARD,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        },
                        new Choice
                        {
                            choiceText = "Too lazy",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("...Me too...", ExpressionType.Neutral),
                                        new DialogueLine("Drifting away slowly.", ExpressionType.Neutral)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                },
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("...This ore...", ExpressionType.Thinking),
                        new DialogueLine("...So shiny...", ExpressionType.Happy),
                        new DialogueLine("Pulsing with soft light.", ExpressionType.Special)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = $"Trade ({SMALL_ORE_COST} Ore)",
                            oreCost = SMALL_ORE_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("...Good...", ExpressionType.Happy),
                                        new DialogueLine("...Sparkle sparkle...", ExpressionType.Happy),
                                        new DialogueLine("Blindingly bright with happiness.", ExpressionType.Special)
                                    },
                                    pearlChange = MEDIUM_PEARL_REWARD,
                                    oreCost = SMALL_ORE_COST,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        },
                        new Choice
                        {
                            choiceText = "Don't trade",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("...Mm...", ExpressionType.Neutral),
                                        new DialogueLine("...Okay...", ExpressionType.Neutral)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        },
                        new Choice
                        {
                            choiceText = "Just look",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("...Looking is...", ExpressionType.Thinking),
                                        new DialogueLine("...Also nice...", ExpressionType.Happy)
                                    },
                                    pearlChange = TINY_BONUS,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                }
          },

         // ==================== SEAHORSE - Exploration ====================
         // Special Expression: Deep shadow across face, intensely serious/bitter
         [NPCPersonality.Seahorse] = new EncounterScenario[]
          {
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Hmph.", ExpressionType.Neutral),
                        new DialogueLine("Found a new route.", ExpressionType.Thinking),
                        new DialogueLine("Eyes hidden in shadow.", ExpressionType.Special),
                        new DialogueLine("Coming with me?", ExpressionType.Neutral)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = "Go",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Good.", ExpressionType.Neutral),
                                        new DialogueLine("Leading the way confidently.", ExpressionType.Neutral),
                                        new DialogueLine("Here. Your share.", ExpressionType.Neutral),
                                        new DialogueLine("You have an eye.", ExpressionType.Happy)
                                    },
                                    pearlChange = LARGE_PEARL_REWARD + SMALL_PEARL_REWARD,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("This way.", ExpressionType.Neutral),
                                        new DialogueLine("Map studied intently.", ExpressionType.Thinking),
                                        new DialogueLine("...Wait.", ExpressionType.Surprised),
                                        new DialogueLine("Map was upside down.", ExpressionType.Special),
                                        new DialogueLine("Hmm.", ExpressionType.Thinking)
                                    },
                                    pearlChange = SMALL_BONUS,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { 55, 45 }
                        },
                        new Choice
                        {
                            choiceText = "Don't go",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Fine.", ExpressionType.Neutral),
                                        new DialogueLine("Your choice.", ExpressionType.Neutral),
                                        new DialogueLine("Respect that.", ExpressionType.Thinking)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        },
                        new Choice
                        {
                            choiceText = "Demand details",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Hmm...", ExpressionType.Thinking),
                                        new DialogueLine("So it's this way...", ExpressionType.Neutral),
                                        new DialogueLine("No, that way...", ExpressionType.Thinking),
                                        new DialogueLine("Shadow deepens.", ExpressionType.Special)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                },
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Paper held up.", ExpressionType.Neutral),
                        new DialogueLine("Treasure map.", ExpressionType.Neutral),
                        new DialogueLine("Genuine.", ExpressionType.Thinking),
                        new DialogueLine("Dramatic pause. Face half in shadow.", ExpressionType.Special)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = $"Buy it ({MEDIUM_PEARL_COST} Pearls)",
                            pearlCost = MEDIUM_PEARL_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Wise.", ExpressionType.Neutral),
                                        new DialogueLine("Map handed over.", ExpressionType.Neutral),
                                        new DialogueLine("...Wait.", ExpressionType.Surprised),
                                        new DialogueLine("It was upside down.", ExpressionType.Thinking),
                                        new DialogueLine("Close enough.", ExpressionType.Neutral)
                                    },
                                    pearlChange = LARGE_PEARL_REWARD,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Map handed over.", ExpressionType.Neutral),
                                        new DialogueLine("...", ExpressionType.Thinking),
                                        new DialogueLine("This is my shopping list.", ExpressionType.Special),
                                        new DialogueLine("Sorry. Refund.", ExpressionType.Neutral)
                                    },
                                    pearlChange = 0,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { 60, 40 }
                        },
                        new Choice
                        {
                            choiceText = "Don't buy",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Fine.", ExpressionType.Neutral),
                                        new DialogueLine("Opportunity knocks once.", ExpressionType.Thinking),
                                        new DialogueLine("...Actually, I'll sell it again later.", ExpressionType.Neutral)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                },
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Recruiting.", ExpressionType.Neutral),
                        new DialogueLine("Expedition members.", ExpressionType.Thinking),
                        new DialogueLine("Intense stare from the shadows.", ExpressionType.Special),
                        new DialogueLine("You look capable.", ExpressionType.Neutral)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = $"Join (Invest {MEDIUM_ORE_COST} Ore)",
                            oreCost = MEDIUM_ORE_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Good.", ExpressionType.Neutral),
                                        new DialogueLine("Leading the expedition.", ExpressionType.Neutral),
                                        new DialogueLine("Great success.", ExpressionType.Happy),
                                        new DialogueLine("Your dividend.", ExpressionType.Neutral)
                                    },
                                    pearlChange = LARGE_PEARL_REWARD,
                                    oreCost = MEDIUM_ORE_COST,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Leading the expedition.", ExpressionType.Neutral),
                                        new DialogueLine("...", ExpressionType.Thinking),
                                        new DialogueLine("We got lost.", ExpressionType.Surprised),
                                        new DialogueLine("The bitter taste of failure.", ExpressionType.Special)
                                    },
                                    pearlChange = SMALL_BONUS,
                                    oreCost = MEDIUM_ORE_COST,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { MEDIUM_SUCCESS_WEIGHT, 50 }
                        },
                        new Choice
                        {
                            choiceText = "Decline",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Shame.", ExpressionType.Neutral),
                                        new DialogueLine("Next time.", ExpressionType.Neutral)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                }
          },

         // ==================== OCTOPUS - Forge ====================
         // Special Expression: Self-absorbed, narcissistic pride
         [NPCPersonality.Octopus] = new EncounterScenario[]
          {
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Oh!", ExpressionType.Surprised),
                        new DialogueLine("Perfect timing!", ExpressionType.Happy),
                        new DialogueLine("New crafting method!", ExpressionType.Happy),
                        new DialogueLine("All eight arms posed dramatically.", ExpressionType.Special)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = "Test it",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Here we go!", ExpressionType.Happy),
                                        new DialogueLine("All eight arms working in perfect harmony.", ExpressionType.Neutral),
                                        new DialogueLine("Perfect!", ExpressionType.Happy),
                                        new DialogueLine("Admiring own handiwork.", ExpressionType.Special),
                                        new DialogueLine("Take the prototype!", ExpressionType.Neutral)
                                    },
                                    pearlChange = MEDIUM_PEARL_REWARD + SMALL_PEARL_REWARD,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Here we go!", ExpressionType.Happy),
                                        new DialogueLine("All eight arms working furiously.", ExpressionType.Neutral),
                                        new DialogueLine("Hmm...", ExpressionType.Thinking),
                                        new DialogueLine("Failed.", ExpressionType.Neutral),
                                        new DialogueLine("But found improvements!", ExpressionType.Happy)
                                    },
                                    pearlChange = SMALL_BONUS,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { 65, 35 }
                        },
                        new Choice
                        {
                            choiceText = "Don't test",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Aww~", ExpressionType.Neutral),
                                        new DialogueLine("Too bad!", ExpressionType.Neutral),
                                        new DialogueLine("Next time!", ExpressionType.Happy)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                },
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Hey!", ExpressionType.Happy),
                        new DialogueLine("Check out this tool!", ExpressionType.Surprised),
                        new DialogueLine("Made with all 8 arms!", ExpressionType.Happy),
                        new DialogueLine("Simultaneously!", ExpressionType.Happy),
                        new DialogueLine("Basking in self-satisfaction.", ExpressionType.Special)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = $"Buy ({MEDIUM_ORE_COST} Ore)",
                            oreCost = MEDIUM_ORE_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Great choice!", ExpressionType.Happy),
                                        new DialogueLine("200% efficiency!", ExpressionType.Happy),
                                        new DialogueLine("Obviously. I made it.", ExpressionType.Special)
                                    },
                                    pearlChange = LARGE_PEARL_REWARD,
                                    oreCost = MEDIUM_ORE_COST,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        },
                        new Choice
                        {
                            choiceText = "Ask for discount",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Haha!", ExpressionType.Happy),
                                        new DialogueLine("Good negotiator!", ExpressionType.Surprised),
                                        new DialogueLine("Fine, special price!", ExpressionType.Happy)
                                    },
                                    pearlChange = MEDIUM_PEARL_REWARD,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Nope~", ExpressionType.Neutral),
                                        new DialogueLine("Already at cost!", ExpressionType.Thinking),
                                        new DialogueLine("My genius doesn't come cheap!", ExpressionType.Special)
                                    },
                                    pearlChange = 0,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { MEDIUM_SUCCESS_WEIGHT, 50 }
                        },
                        new Choice
                        {
                            choiceText = "Don't buy",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Alright!", ExpressionType.Neutral),
                                        new DialogueLine("Let me know if needed!", ExpressionType.Happy)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                },
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Running low!", ExpressionType.Surprised),
                        new DialogueLine("Ore shortage!", ExpressionType.Neutral),
                        new DialogueLine("Even my brilliant work needs materials.", ExpressionType.Special),
                        new DialogueLine("Can you lend some?", ExpressionType.Neutral)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = $"Lend ({MEDIUM_ORE_COST} Ore)",
                            oreCost = MEDIUM_ORE_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Thanks!", ExpressionType.Happy),
                                        new DialogueLine("Take this!", ExpressionType.Happy),
                                        new DialogueLine("Another masterpiece.", ExpressionType.Special)
                                    },
                                    pearlChange = LARGE_PEARL_REWARD,
                                    oreCost = MEDIUM_ORE_COST,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        },
                        new Choice
                        {
                            choiceText = "Demand interest",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Sharp!", ExpressionType.Surprised),
                                        new DialogueLine("Alright!", ExpressionType.Happy),
                                        new DialogueLine("Here's interest too!", ExpressionType.Neutral)
                                    },
                                    pearlChange = MEDIUM_PEARL_REWARD,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        },
                        new Choice
                        {
                            choiceText = "Don't lend",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Got it!", ExpressionType.Neutral),
                                        new DialogueLine("I'll find elsewhere!", ExpressionType.Happy)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                }
          },

         // ==================== DOLPHIN - Manager ====================
         // Special Expression: Bleached, hollow-eyed exhaustion
         [NPCPersonality.Dolphin] = new EncounterScenario[]
          {
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Oh, hello!", ExpressionType.Happy),
                        new DialogueLine("Just taking a break...", ExpressionType.Neutral),
                        new DialogueLine("Thousand-yard stare.", ExpressionType.Special),
                        new DialogueLine("So much work...", ExpressionType.Thinking)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = $"Treat to coffee ({SMALL_PEARL_COST} Pearls)",
                            pearlCost = SMALL_PEARL_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Thank you!", ExpressionType.Surprised),
                                        new DialogueLine("Feeling energized!", ExpressionType.Happy),
                                        new DialogueLine("Here, from my emergency fund...", ExpressionType.Happy)
                                    },
                                    pearlChange = MEDIUM_PEARL_REWARD,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        },
                        new Choice
                        {
                            choiceText = "Cheer them on",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Thank you...", ExpressionType.Happy),
                                        new DialogueLine("Tears welling up.", ExpressionType.Surprised),
                                        new DialogueLine("It's been so long...", ExpressionType.Happy),
                                        new DialogueLine("Since anyone said that...", ExpressionType.Happy)
                                    },
                                    pearlChange = SMALL_PEARL_REWARD,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        },
                        new Choice
                        {
                            choiceText = "Say you're busy",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Oh, yes!", ExpressionType.Neutral),
                                        new DialogueLine("Everyone's busy!", ExpressionType.Neutral),
                                        new DialogueLine("Back to work...", ExpressionType.Thinking),
                                        new DialogueLine("Soul leaving body.", ExpressionType.Special)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                },
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Um...", ExpressionType.Neutral),
                        new DialogueLine("Could you help...", ExpressionType.Thinking),
                        new DialogueLine("With some documents?", ExpressionType.Neutral),
                        new DialogueLine("The light fading from those eyes.", ExpressionType.Special)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = "Help",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Thank you!", ExpressionType.Happy),
                                        new DialogueLine("Papers organized with renewed vigor.", ExpressionType.Neutral),
                                        new DialogueLine("Processing as expenses!", ExpressionType.Happy)
                                    },
                                    pearlChange = MEDIUM_PEARL_REWARD + SMALL_BONUS,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        },
                        new Choice
                        {
                            choiceText = "Demand compensation",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Of course!", ExpressionType.Happy),
                                        new DialogueLine("Fair compensation!", ExpressionType.Neutral),
                                        new DialogueLine("Here you go!", ExpressionType.Happy)
                                    },
                                    pearlChange = MEDIUM_PEARL_REWARD,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        },
                        new Choice
                        {
                            choiceText = "Refuse",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Oh...", ExpressionType.Neutral),
                                        new DialogueLine("Yes...", ExpressionType.Thinking),
                                        new DialogueLine("Understandable...", ExpressionType.Neutral),
                                        new DialogueLine("Color draining from face.", ExpressionType.Special)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                },
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Leftover snacks!", ExpressionType.Happy),
                        new DialogueLine("From a meeting!", ExpressionType.Neutral),
                        new DialogueLine("Want to share?", ExpressionType.Happy)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = "Eat together",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Hehe!", ExpressionType.Happy),
                                        new DialogueLine("Tastes better together!", ExpressionType.Happy),
                                        new DialogueLine("Oh, here's a gift!", ExpressionType.Surprised)
                                    },
                                    pearlChange = MEDIUM_PEARL_REWARD,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        },
                        new Choice
                        {
                            choiceText = "Decline",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Oh, okay!", ExpressionType.Neutral),
                                        new DialogueLine("Eating alone then...", ExpressionType.Thinking),
                                        new DialogueLine("Hollow smile.", ExpressionType.Special)
                                    },
                                    pearlChange = TINY_BONUS,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        },
                        new Choice
                        {
                            choiceText = "Take everything",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Huh...?", ExpressionType.Surprised),
                                        new DialogueLine("Oh, yes...", ExpressionType.Neutral),
                                        new DialogueLine("Take it all...", ExpressionType.Thinking),
                                        new DialogueLine("I'm on a diet anyway...", ExpressionType.Special)
                                    },
                                    pearlChange = SMALL_PEARL_REWARD,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                }
          }
      };
   }
   #endregion

   #region NPC Spawn/Despawn
   private void OnTurnEnded()
   {
      if (hasActiveNPC)
         DespawnNPC();

      if (Random.Range(0, 100) < spawnChancePerTurn)
         SpawnRandomNPC();
   }

   private void SpawnRandomNPC()
   {
      if (buildingLocations == null || buildingLocations.Length == 0) return;
      if (npcProfiles == null || npcProfiles.Length == 0) return;

      int buildingIndex = Random.Range(0, buildingLocations.Length);
      int npcIndex = Random.Range(0, npcProfiles.Length);

      currentNPC = npcProfiles[npcIndex];

      if (scenarioDatabase.TryGetValue(currentNPC.personality, out var scenarios))
         currentScenario = scenarios[Random.Range(0, scenarios.Length)];

      Vector3 spawnPos = buildingLocations[buildingIndex].position + spawnOffset;
      currentNPCObject = Instantiate(npcPrefab, spawnPos, Quaternion.identity);

      var sr = currentNPCObject.GetComponent<SpriteRenderer>();
      if (sr != null && currentNPC.mapSprite != null)
         sr.sprite = currentNPC.mapSprite;

      var collider = currentNPCObject.GetComponent<Collider2D>();
      if (collider == null)
         currentNPCObject.AddComponent<BoxCollider2D>();

      var clickHandler = currentNPCObject.AddComponent<NPCClickHandler>();
      clickHandler.Initialize(this);

      hasActiveNPC = true;
      Debug.Log($"[NPC] {currentNPC.npcName} has appeared!");
   }

   private void DespawnNPC()
   {
      if (currentNPCObject != null)
         Destroy(currentNPCObject);

      currentNPCObject = null;
      currentNPC = null;
      currentScenario = null;
      hasActiveNPC = false;
   }
   #endregion

   #region Interaction Processing
   public void OnNPCClicked()
   {
      if (!hasActiveNPC || currentNPC == null || currentScenario == null) return;

      if (dialogueUI != null)
         dialogueUI.ShowDialogue(currentNPC, currentScenario);
   }

   public EncounterOutcome ProcessChoice(int choiceIndex)
   {
      if (currentScenario == null || choiceIndex >= currentScenario.choices.Length)
         return null;

      Choice selectedChoice = currentScenario.choices[choiceIndex];

      var inv = InventoryManager.Instance;
      if (inv != null)
      {
         if (selectedChoice.pearlCost > 0 && inv.pearlCount < selectedChoice.pearlCost)
         {
            return new EncounterOutcome("You don't have enough pearls...", ExpressionType.Neutral, 0, false);
         }

         if (selectedChoice.oreCost > 0 && inv.oreCount < selectedChoice.oreCost)
         {
            return new EncounterOutcome("You don't have enough ore...", ExpressionType.Neutral, 0, false);
         }
      }

      EncounterOutcome result = GetWeightedRandomOutcome(
          selectedChoice.outcomes,
          selectedChoice.outcomeWeights
      );

      if (selectedChoice.pearlCost > 0)
         inv?.TrySpendPearl(selectedChoice.pearlCost);
      if (selectedChoice.oreCost > 0)
         inv?.TrySpendOre(selectedChoice.oreCost);

      ApplyOutcome(result);

      return result;
   }

   private EncounterOutcome GetWeightedRandomOutcome(EncounterOutcome[] outcomes, int[] weights)
   {
      int totalWeight = 0;
      foreach (int w in weights)
         totalWeight += w;

      int roll = Random.Range(0, totalWeight);
      int cumulative = 0;

      for (int i = 0; i < outcomes.Length; i++)
      {
         cumulative += weights[i];
         if (roll < cumulative)
            return outcomes[i];
      }

      return outcomes[outcomes.Length - 1];
   }

   private void ApplyOutcome(EncounterOutcome outcome)
   {
      if (outcome == null) return;

      var inv = InventoryManager.Instance;
      if (inv == null) return;

      if (outcome.pearlChange > 0)
         inv.TryAddPearl(outcome.pearlChange);
      else if (outcome.pearlChange < 0)
         inv.TrySpendPearl(-outcome.pearlChange);

      Debug.Log($"[NPC] Result: Pearl {outcome.pearlChange:+#;-#;0}");
   }

   public void OnDialogueEnded()
   {
      DespawnNPC();
   }

   /// <summary>
   /// Gets the appropriate portrait sprite for the given expression
   /// </summary>
   public Sprite GetPortraitForExpression(NPCProfile npc, ExpressionType expression)
   {
      return expression switch
      {
         ExpressionType.Happy => npc.portraitHappy ?? npc.portraitNeutral,
         ExpressionType.Angry => npc.portraitAngry ?? npc.portraitNeutral,
         ExpressionType.Surprised => npc.portraitSurprised ?? npc.portraitNeutral,
         ExpressionType.Thinking => npc.portraitThinking ?? npc.portraitNeutral,
         ExpressionType.Special => npc.portraitSpecial ?? npc.portraitNeutral,
         _ => npc.portraitNeutral
      };
   }
   #endregion

   #region Public Getters
   public NPCProfile GetCurrentNPC() => currentNPC;
   public EncounterScenario GetCurrentScenario() => currentScenario;
   public bool HasActiveNPC() => hasActiveNPC;
   #endregion
}

/// <summary>
/// Handles NPC click detection
/// </summary>
public class NPCClickHandler : MonoBehaviour
{
   private NPCEncounterSystem system;

   public void Initialize(NPCEncounterSystem encounterSystem)
   {
      system = encounterSystem;
   }

   private void OnMouseDown()
   {
      system?.OnNPCClicked();
   }
}