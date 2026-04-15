using System.Collections.Generic;
using UnityEngine;

// Manages the roguelike-style NPC encounter system.
// Each turn, a random NPC may appear near a building on the map.
// Players click the NPC to trigger a multi-line dialogue with branching choices,
// each leading to weighted random outcomes that reward or penalize pearls/ore.
// All dialogue and logic is hardcoded (no LLM required).
public class NPCEncounterSystem : MonoBehaviour
{
    public static NPCEncounterSystem Instance { get; private set; }

    // Reward/Penalty constants                                                                     
    private const int SMALL_PEARL_REWARD   = 20;
    private const int MEDIUM_PEARL_REWARD  = 50;
    private const int LARGE_PEARL_REWARD   = 100;
    private const int HUGE_PEARL_REWARD    = 150;

    private const int SMALL_PEARL_COST     = 10;
    private const int MEDIUM_PEARL_COST    = 30;
    private const int LARGE_PEARL_COST     = 50;
    private const int HUGE_PEARL_COST      = 100;

    private const int SMALL_PEARL_PENALTY  = 20;
    private const int MEDIUM_PEARL_PENALTY = 50;
    private const int LARGE_PEARL_PENALTY  = 80;

    private const int SMALL_ORE_COST       = 15;
    private const int MEDIUM_ORE_COST      = 30;
    private const int LARGE_ORE_COST       = 50;

    private const int TINY_BONUS           = 5;
    private const int SMALL_BONUS          = 10;

    // Outcome weight constants (higher = more likely)                                              
    private const int HIGH_SUCCESS_WEIGHT   = 70;
    private const int MEDIUM_SUCCESS_WEIGHT = 50;
    private const int LOW_SUCCESS_WEIGHT    = 35;
    private const int GUARANTEED_WEIGHT     = 100;

    // Spawn settings                                                                               
    private const int DEFAULT_SPAWN_CHANCE  = 25;


    // ─────────────────────────────────────────────────────────────────────
    //  Data classes
    // ─────────────────────────────────────────────────────────────────────

    // Holds all sprite and personality info for a single NPC
    [System.Serializable]
    public class NPCProfile
    {
        public string npcName;
        public Sprite mapSprite;          // Shown on the world map when the NPC spawns
        public Sprite portraitNeutral;    // Default portrait
        public Sprite portraitHappy;
        public Sprite portraitAngry;
        public Sprite portraitSurprised;
        public Sprite portraitThinking;
        public Sprite portraitSpecial;    // Unique expression per character (see enum below)
        public NPCPersonality personality;
    }

    // Each NPC is tied to a building and has a unique special portrait:
    //   HermitCrab  → TradeHut
    //   Turtle      → Lab
    //   Jellyfish   → Refinery
    //   Seahorse    → Exploration
    //   Octopus     → Forge
    //   Dolphin     → (Manager)
    public enum NPCPersonality
    {
        HermitCrab,
        Turtle,
        Jellyfish,
        Seahorse,
        Octopus,
        Dolphin
    }

    // Portrait expression types. "Special" maps to portraitSpecial per character.
    public enum ExpressionType
    {
        Neutral,
        Happy,
        Angry,
        Surprised,
        Thinking,
        Special
    }

    // A single line of dialogue with an associated portrait expression.
    // If isAction is true the UI renders the line in italics (stage direction).
    [System.Serializable]
    public class DialogueLine
    {
        public string text;
        public ExpressionType expression;
        public bool isAction;

        public DialogueLine(string text, ExpressionType expression = ExpressionType.Neutral, bool isAction = false)
        {
            this.text = text;
            this.expression = expression;
            this.isAction = isAction;
        }
    }

   // The result the player receives after choosing an option.
   // pearlChange: positive = gain, negative = loss
   // oreCost:     ore spent (always >= 0)
   [System.Serializable]
   public class EncounterOutcome
   {
      public DialogueLine[] resultDialogues;
      public int pearlChange;
      public int oreCost;
      public bool isPositive;
      public bool keepNpcAfterClose;

      public EncounterOutcome() { }

      // Convenience constructor for single-line error/fallback outcomes
      public EncounterOutcome(string singleLine, ExpressionType expr, int pearl, bool positive)
      {
         resultDialogues = new DialogueLine[] { new DialogueLine(singleLine, expr) };
         pearlChange = pearl;
         isPositive = positive;
         keepNpcAfterClose = false;
      }
   }

   // A complete encounter: opening dialogue lines followed by player choices
   [System.Serializable]
    public class EncounterScenario
    {
        public DialogueLine[] openingDialogues;
        public Choice[] choices;
    }

    // A single selectable option. outcomeWeights controls the probability
    // distribution across the possible outcomes.
    [System.Serializable]
    public class Choice
    {
        public string choiceText;
        public EncounterOutcome[] outcomes;
        public int[] outcomeWeights;
        public int oreCost;
        public int pearlCost;
    }


   // ─────────────────────────────────────────────────────────────────────
   //  Inspector fields
   // ─────────────────────────────────────────────────────────────────────

   [Header("NPC Profiles (6 Characters)")]
   [SerializeField] private NPCProfile[] npcProfiles = new NPCProfile[6];

   [Header("Spawn Settings")]
   [SerializeField] private Transform[] buildingLocations;   // 5 building transforms
   [SerializeField] private GameObject npcPrefab;            // Prefab with SpriteRenderer + Collider2D
   [SerializeField] private Vector3 spawnOffset = new Vector3(2f, 1f, 0f);
   [SerializeField] private Vector3 npcWorldScale = new Vector3(4.5f, 4.5f, 1f);
   [SerializeField, Range(0, 100)] private int spawnChancePerTurn = DEFAULT_SPAWN_CHANCE;

   [Header("UI References")]
   [SerializeField] private NPCDialogueUI dialogueUI;


   // ─────────────────────────────────────────────────────────────────────
   //  Runtime state
   // ─────────────────────────────────────────────────────────────────────

   private GameObject currentNPCObject;
    private NPCProfile currentNPC;
    private EncounterScenario currentScenario;
    private bool hasActiveNPC = false;

    // Maps each personality to its pool of encounter scenarios
    private Dictionary<NPCPersonality, EncounterScenario[]> scenarioDatabase;


    // ─────────────────────────────────────────────────────────────────────
    //  Unity lifecycle
    // ─────────────────────────────────────────────────────────────────────

    // Enforces the Singleton pattern and builds the scenario database
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

    // Subscribes to the turn-end event so NPCs can spawn each turn
    private void Start()
    {
        TurnManager.OnTurnEnded += OnTurnEnded;
    }

    private void OnDestroy()
    {
        TurnManager.OnTurnEnded -= OnTurnEnded;
    }


    // ─────────────────────────────────────────────────────────────────────
    //  Scenario database
    //  Each NPC personality has 3 scenarios with 2-3 choices each.
    //  isAction = true lines are rendered in italics by the UI.
    // ─────────────────────────────────────────────────────────────────────

    private void InitializeScenarioDatabase()
    {
        scenarioDatabase = new Dictionary<NPCPersonality, EncounterScenario[]>
        {
            // ── HERMIT CRAB ─ TradeHut ──
            [NPCPersonality.HermitCrab] = new EncounterScenario[]
            {
                // Scenario 1 — Investment opportunity
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Ah, a visitor.", ExpressionType.Neutral),
                        new DialogueLine("Monocle adjusted. Pearls gleaming.", ExpressionType.Thinking, isAction: true),
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
                                        new DialogueLine("Pearls counted with visible satisfaction.", ExpressionType.Special, isAction: true),
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
                                        new DialogueLine("Eye contact avoided.", ExpressionType.Neutral, isAction: true),
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
                                        new DialogueLine("Shell stroked thoughtfully. Eyes gleaming with greed.", ExpressionType.Special, isAction: true),
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
                                        new DialogueLine("Gaze fixed firmly on the floor.", ExpressionType.Thinking, isAction: true),
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

                // Scenario 2 — Ore appraisal
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Pocket watch examined.", ExpressionType.Neutral, isAction: true),
                        new DialogueLine("Ah, perfect timing.", ExpressionType.Happy),
                        new DialogueLine("I've been looking for quality ore specimens.", ExpressionType.Neutral),
                        new DialogueLine("Premium pieces fetch a handsome price in my circles.", ExpressionType.Thinking),
                        new DialogueLine("Happen to have any for sale?", ExpressionType.Special)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = $"Sell ore ({MEDIUM_ORE_COST} Ore)",
                            oreCost = MEDIUM_ORE_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Excellent specimen!", ExpressionType.Happy),
                                        new DialogueLine("Ore examined through monocle with delight.", ExpressionType.Special, isAction: true),
                                        new DialogueLine("This is worth far more than you know.", ExpressionType.Thinking),
                                        new DialogueLine("Here. A generous payment, as promised.", ExpressionType.Happy)
                                    },
                                    pearlChange = LARGE_PEARL_REWARD,
                                    oreCost = MEDIUM_ORE_COST,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Wait...", ExpressionType.Surprised),
                                        new DialogueLine("Ore inspected closely. Color draining from face.", ExpressionType.Thinking, isAction: true),
                                        new DialogueLine("Good heavens! This grade is rather... common.", ExpressionType.Angry),
                                        new DialogueLine("I can only offer a modest sum. My apologies.", ExpressionType.Neutral)
                                    },
                                    pearlChange = SMALL_PEARL_REWARD,
                                    oreCost = MEDIUM_ORE_COST,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { HIGH_SUCCESS_WEIGHT, 30 }
                        },
                        new Choice
                        {
                            choiceText = "Negotiate the price first",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Ho ho! A negotiator!", ExpressionType.Surprised),
                                        new DialogueLine("I respect a sharp business mind.", ExpressionType.Thinking),
                                        new DialogueLine("Very well. A token of good faith.", ExpressionType.Happy)
                                    },
                                    pearlChange = SMALL_PEARL_REWARD,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Eyebrow raised.", ExpressionType.Neutral, isAction: true),
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

                // Scenario 3 — Gentleman's wager
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("A soft chuckle.", ExpressionType.Happy, isAction: true),
                        new DialogueLine("You know, I find myself in excellent spirits today.", ExpressionType.Neutral),
                        new DialogueLine("Care for a gentleman's wager?", ExpressionType.Thinking),
                        new DialogueLine("A simple coin toss. Nothing crude, I assure you.", ExpressionType.Neutral),
                        new DialogueLine("Pearls practically dancing on the shell.", ExpressionType.Special, isAction: true)
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
                                        new DialogueLine("Coin flipped with practiced elegance.", ExpressionType.Neutral, isAction: true),
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
                                        new DialogueLine("Coin flipped with practiced elegance.", ExpressionType.Neutral, isAction: true),
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
                                        new DialogueLine("Coin flipped with a flourish.", ExpressionType.Neutral, isAction: true),
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
                                        new DialogueLine("Coin flipped with a flourish.", ExpressionType.Neutral, isAction: true),
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
                                        new DialogueLine("Hat tipped graciously.", ExpressionType.Happy, isAction: true),
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

            // ── TURTLE ─ Lab  ──────────
            [NPCPersonality.Turtle] = new EncounterScenario[]
            {
                // Scenario 1 — Dangerous experiment
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Ooh! Perfect timing!", ExpressionType.Surprised),
                        new DialogueLine("Beakers crashing to the floor.", ExpressionType.Surprised, isAction: true),
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
                                        new DialogueLine("Chemicals mixed with reckless abandon.", ExpressionType.Thinking, isAction: true),
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
                                        new DialogueLine("Chemicals mixed with reckless abandon.", ExpressionType.Thinking, isAction: true),
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

                // Scenario 2 — Mystery invention
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Hey hey hey!", ExpressionType.Happy),
                        new DialogueLine("Check this out!", ExpressionType.Surprised),
                        new DialogueLine("My invention!", ExpressionType.Happy),
                        new DialogueLine("I don't know what it does either!", ExpressionType.Special),
                        new DialogueLine("Trade me some ore for it?", ExpressionType.Happy)
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
                                        new DialogueLine("Button pressed.", ExpressionType.Thinking, isAction: true),
                                        new DialogueLine("Oh! It works!", ExpressionType.Surprised),
                                        new DialogueLine("It was a pearl refiner!", ExpressionType.Happy)
                                    },
                                    pearlChange = MEDIUM_PEARL_REWARD,
                                    oreCost = SMALL_ORE_COST,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Button pressed.", ExpressionType.Thinking, isAction: true),
                                        new DialogueLine("...Huh? Smoke?", ExpressionType.Surprised),
                                        new DialogueLine("RUN!", ExpressionType.Special)
                                    },
                                    pearlChange = 0,
                                    oreCost = SMALL_ORE_COST,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { MEDIUM_SUCCESS_WEIGHT, 50 }
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
                                        new DialogueLine("Button pressed.", ExpressionType.Thinking, isAction: true),
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

                // Scenario 3 — Explosive bet
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Ooh! I just thought of something!", ExpressionType.Surprised),
                        new DialogueLine("Eyes sparkling with manic energy.", ExpressionType.Special, isAction: true),
                        new DialogueLine("Wanna bet on my next experiment?!", ExpressionType.Happy),
                        new DialogueLine("Will it explode? Won't it?!", ExpressionType.Special),
                        new DialogueLine("Either way it'll be FUN!", ExpressionType.Happy)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = $"Bet: It explodes ({MEDIUM_PEARL_COST} Pearls)",
                            pearlCost = MEDIUM_PEARL_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Chemicals mixed haphazardly.", ExpressionType.Thinking, isAction: true),
                                        new DialogueLine("3... 2... 1...", ExpressionType.Neutral),
                                        new DialogueLine("KABOOM!", ExpressionType.Surprised),
                                        new DialogueLine("Haha! You were right! Here ya go!", ExpressionType.Special)
                                    },
                                    pearlChange = MEDIUM_PEARL_REWARD + SMALL_PEARL_REWARD,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Chemicals mixed haphazardly.", ExpressionType.Thinking, isAction: true),
                                        new DialogueLine("3... 2... 1...", ExpressionType.Neutral),
                                        new DialogueLine("...Nothing.", ExpressionType.Neutral),
                                        new DialogueLine("Aww! It didn't blow up! Your loss~", ExpressionType.Happy)
                                    },
                                    pearlChange = -MEDIUM_PEARL_COST,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { 55, 45 }
                        },
                        new Choice
                        {
                            choiceText = $"Bet: It doesn't ({MEDIUM_PEARL_COST} Pearls)",
                            pearlCost = MEDIUM_PEARL_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Beaker shaken violently.", ExpressionType.Thinking, isAction: true),
                                        new DialogueLine("3... 2... 1...", ExpressionType.Neutral),
                                        new DialogueLine("...Huh. Stable!", ExpressionType.Surprised),
                                        new DialogueLine("No way! You win! Take it!", ExpressionType.Happy)
                                    },
                                    pearlChange = MEDIUM_PEARL_REWARD + SMALL_PEARL_REWARD,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Beaker shaken violently.", ExpressionType.Thinking, isAction: true),
                                        new DialogueLine("3... 2... 1...", ExpressionType.Neutral),
                                        new DialogueLine("BOOM! Hahaha!", ExpressionType.Special),
                                        new DialogueLine("It exploded! I win~!", ExpressionType.Happy)
                                    },
                                    pearlChange = -MEDIUM_PEARL_COST,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { 45, 55 }
                        },
                        new Choice
                        {
                            choiceText = "Too scary, no thanks",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Boo! Chicken!", ExpressionType.Angry),
                                        new DialogueLine("...Just kidding~", ExpressionType.Happy),
                                        new DialogueLine("Here, a consolation snack!", ExpressionType.Happy)
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

            // ── JELLYFISH ─ Refinery ──────────
            [NPCPersonality.Jellyfish] = new EncounterScenario[]
            {
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("...", ExpressionType.Neutral),
                        new DialogueLine("...Shiny...", ExpressionType.Thinking),
                        new DialogueLine("...Pretty thing...", ExpressionType.Happy),
                        new DialogueLine("A faint glow begins.", ExpressionType.Special, isAction: true)
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
                                        new DialogueLine("Glowing brightly now.", ExpressionType.Special, isAction: true)
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
                                        new DialogueLine("The glow fades.", ExpressionType.Neutral, isAction: true)
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
                                        new DialogueLine("Entire body illuminated with joy.", ExpressionType.Special, isAction: true)
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
                                        new DialogueLine("Slow, methodical mining.", ExpressionType.Neutral, isAction: true),
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
                                        new DialogueLine("Drifting away slowly.", ExpressionType.Neutral, isAction: true)
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
                        new DialogueLine("Pulsing with soft light.", ExpressionType.Special, isAction: true)
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
                                        new DialogueLine("Blindingly bright with happiness.", ExpressionType.Special, isAction: true)
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

            // ── SEAHORSE ─ Exploration ───────────────
            [NPCPersonality.Seahorse] = new EncounterScenario[]
            {
                // Scenario 1 — Mysterious newcomer
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Hmph.", ExpressionType.Neutral),
                        new DialogueLine("Found a new route.", ExpressionType.Thinking),
                        new DialogueLine("Eyes hidden in shadow.", ExpressionType.Special, isAction: true),
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
                                        new DialogueLine("Leading the way confidently.", ExpressionType.Neutral, isAction: true),
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
                                        new DialogueLine("Map studied intently.", ExpressionType.Thinking, isAction: true),
                                        new DialogueLine("...Wait.", ExpressionType.Surprised),
                                        new DialogueLine("Map was upside down.", ExpressionType.Special, isAction: true),
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
                                        new DialogueLine("Shadow deepens.", ExpressionType.Special, isAction: true)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                },

                // Scenario 2 — Treasure map
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Paper held up.", ExpressionType.Neutral, isAction: true),
                        new DialogueLine("Treasure map.", ExpressionType.Neutral),
                        new DialogueLine("Genuine.", ExpressionType.Thinking),
                        new DialogueLine("Dramatic pause. Face half in shadow.", ExpressionType.Special, isAction: true),
                        new DialogueLine("Need ore for supplies.", ExpressionType.Neutral)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = $"Fund the expedition ({MEDIUM_ORE_COST} Ore)",
                            oreCost = MEDIUM_ORE_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Wise.", ExpressionType.Neutral),
                                        new DialogueLine("Map studied. Expedition launched.", ExpressionType.Neutral, isAction: true),
                                        new DialogueLine("...Wait.", ExpressionType.Surprised),
                                        new DialogueLine("Map was upside down. But we found treasure anyway.", ExpressionType.Thinking),
                                        new DialogueLine("Your share.", ExpressionType.Neutral)
                                    },
                                    pearlChange = LARGE_PEARL_REWARD,
                                    oreCost = MEDIUM_ORE_COST,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Map studied. Expedition launched.", ExpressionType.Neutral, isAction: true),
                                        new DialogueLine("...", ExpressionType.Thinking),
                                        new DialogueLine("This is my shopping list.", ExpressionType.Special, isAction: true),
                                        new DialogueLine("Sorry. Small consolation.", ExpressionType.Neutral)
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
                            choiceText = "Don't fund",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Fine.", ExpressionType.Neutral),
                                        new DialogueLine("Opportunity knocks once.", ExpressionType.Thinking),
                                        new DialogueLine("...Actually, I'll ask someone else.", ExpressionType.Neutral)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                },

                // Scenario 3 — Silent auction
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Mysterious pouch placed on the table.", ExpressionType.Neutral, isAction: true),
                        new DialogueLine("Auction.", ExpressionType.Neutral),
                        new DialogueLine("Blind bid.", ExpressionType.Thinking),
                        new DialogueLine("Could be treasure. Could be trash.", ExpressionType.Neutral),
                        new DialogueLine("Shadow deepens across face.", ExpressionType.Special, isAction: true)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = $"High bid ({MEDIUM_PEARL_COST} Pearls)",
                            pearlCost = MEDIUM_PEARL_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Pouch opened.", ExpressionType.Neutral, isAction: true),
                                        new DialogueLine("...", ExpressionType.Thinking),
                                        new DialogueLine("Rare pearls.", ExpressionType.Surprised),
                                        new DialogueLine("Good eye.", ExpressionType.Neutral)
                                    },
                                    pearlChange = MEDIUM_PEARL_REWARD + SMALL_PEARL_REWARD,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Pouch opened.", ExpressionType.Neutral, isAction: true),
                                        new DialogueLine("...", ExpressionType.Thinking),
                                        new DialogueLine("Pebbles.", ExpressionType.Neutral),
                                        new DialogueLine("A resigned shrug.", ExpressionType.Special, isAction: true)
                                    },
                                    pearlChange = -MEDIUM_PEARL_COST,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { MEDIUM_SUCCESS_WEIGHT, 50 }
                        },
                        new Choice
                        {
                            choiceText = $"Low bid ({SMALL_PEARL_COST} Pearls)",
                            pearlCost = SMALL_PEARL_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Pouch opened.", ExpressionType.Neutral, isAction: true),
                                        new DialogueLine("Small find. But decent.", ExpressionType.Thinking),
                                        new DialogueLine("Here.", ExpressionType.Neutral)
                                    },
                                    pearlChange = SMALL_PEARL_REWARD + SMALL_BONUS,
                                    isPositive = true
                                },
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Pouch opened.", ExpressionType.Neutral, isAction: true),
                                        new DialogueLine("Empty.", ExpressionType.Neutral),
                                        new DialogueLine("That's the risk.", ExpressionType.Thinking)
                                    },
                                    pearlChange = -SMALL_PEARL_COST,
                                    isPositive = false
                                }
                            },
                            outcomeWeights = new int[] { 60, 40 }
                        },
                        new Choice
                        {
                            choiceText = "Walk away",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Smart.", ExpressionType.Neutral),
                                        new DialogueLine("Or maybe not.", ExpressionType.Thinking),
                                        new DialogueLine("You'll never know.", ExpressionType.Neutral)
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

            // ── OCTOPUS ─ Forge ───────────────────
            [NPCPersonality.Octopus] = new EncounterScenario[]
            {
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Oh!", ExpressionType.Surprised),
                        new DialogueLine("Perfect timing!", ExpressionType.Happy),
                        new DialogueLine("New crafting method!", ExpressionType.Happy),
                        new DialogueLine("All eight arms posed dramatically.", ExpressionType.Special, isAction: true)
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
                                        new DialogueLine("All eight arms working in perfect harmony.", ExpressionType.Neutral, isAction: true),
                                        new DialogueLine("Perfect!", ExpressionType.Happy),
                                        new DialogueLine("Admiring own handiwork.", ExpressionType.Special, isAction: true),
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
                                        new DialogueLine("All eight arms working furiously.", ExpressionType.Neutral, isAction: true),
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
                        new DialogueLine("Basking in self-satisfaction.", ExpressionType.Special, isAction: true)
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
                                        new DialogueLine("Another masterpiece.", ExpressionType.Special, isAction: true)
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

            // ── 돌고래
            // InitializeScenarioDatabase() 내 Dolphin의 첫 번째 시나리오를 아래로 교체

            [NPCPersonality.Dolphin] = new EncounterScenario[]
            {
                // Scenario 1 — Break time
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Oh, hello!", ExpressionType.Happy),
                        new DialogueLine("Just taking a break...", ExpressionType.Neutral),
                        new DialogueLine("Thousand-yard stare.", ExpressionType.Special, isAction: true),
                        new DialogueLine("So much work...", ExpressionType.Thinking)
                    },
                    choices = new Choice[]
                    {
                        new Choice
                        {
                            choiceText = $"Treat to coffee ({SMALL_ORE_COST} Ore)",
                            oreCost = SMALL_ORE_COST,
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("You brought ore for the vending machine?!", ExpressionType.Surprised),
                                        new DialogueLine("Feeling energized!", ExpressionType.Happy),
                                        new DialogueLine("Here, from my emergency fund...", ExpressionType.Happy)
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
                            choiceText = "Cheer them on",
                            outcomes = new EncounterOutcome[]
                            {
                                new EncounterOutcome
                                {
                                    resultDialogues = new DialogueLine[]
                                    {
                                        new DialogueLine("Thank you...", ExpressionType.Happy),
                                        new DialogueLine("Tears welling up.", ExpressionType.Surprised, isAction: true),
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
                                        new DialogueLine("Soul leaving body.", ExpressionType.Special, isAction: true)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                },

                // Scenario 2 — Document help
                new EncounterScenario
                {
                    openingDialogues = new DialogueLine[]
                    {
                        new DialogueLine("Um...", ExpressionType.Neutral),
                        new DialogueLine("Could you help...", ExpressionType.Thinking),
                        new DialogueLine("With some documents?", ExpressionType.Neutral),
                        new DialogueLine("The light fading from those eyes.", ExpressionType.Special, isAction: true)
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
                                        new DialogueLine("Papers organized with renewed vigor.", ExpressionType.Neutral, isAction: true),
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
                                        new DialogueLine("Color draining from face.", ExpressionType.Special, isAction: true)
                                    },
                                    pearlChange = 0,
                                    isPositive = true
                                }
                            },
                            outcomeWeights = new int[] { GUARANTEED_WEIGHT }
                        }
                    }
                },

                // Scenario 3 — Leftover snacks
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
                                        new DialogueLine("Hollow smile.", ExpressionType.Special, isAction: true)
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


    // ─────────────────────────────────────────────────────────────────────
    //  NPC spawn / despawn
    // ─────────────────────────────────────────────────────────────────────

    // Called every turn end. Removes the old NPC and may spawn a new one.
    private void OnTurnEnded()
    {
        // Close any open dialogue first
        if (NPCDialogueUI.Instance != null && NPCDialogueUI.Instance.IsDialogueActive())
            NPCDialogueUI.Instance.ForceClose();

        if (hasActiveNPC)
            DespawnNPC();

        if (Random.Range(0, 100) < spawnChancePerTurn)
            SpawnRandomNPC();
    }

    // Picks a random NPC + scenario and places the NPC near a random building
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

        // Apply map sprite
        var sr = currentNPCObject.GetComponent<SpriteRenderer>();
        if (sr != null && currentNPC.mapSprite != null)
            sr.sprite = currentNPC.mapSprite;

      // Scale up so the NPC is easily visible on the map
      currentNPCObject.transform.localScale = npcWorldScale;

      // Ensure there is a collider for click detection
      if (currentNPCObject.GetComponent<Collider2D>() == null)
        {
            BoxCollider2D box = currentNPCObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1f, 1f);
        }

        var clickHandler = currentNPCObject.AddComponent<NPCClickHandler>();
        clickHandler.Initialize(this);

        hasActiveNPC = true;
        Debug.Log($"[NPC] {currentNPC.npcName} has appeared!");
    }

    // Destroys the currently active NPC and clears references
    private void DespawnNPC()
    {
        if (currentNPCObject != null)
            Destroy(currentNPCObject);

        currentNPCObject = null;
        currentNPC = null;
        currentScenario = null;
        hasActiveNPC = false;
    }


    // ─────────────────────────────────────────────────────────────────────
    //  Interaction processing
    // ─────────────────────────────────────────────────────────────────────

    // Opens the dialogue UI when the player clicks an NPC on the map
    public void OnNPCClicked()
    {
        if (!hasActiveNPC || currentNPC == null || currentScenario == null) return;

      // Block NPC interaction while other popup-style UI is open
      if (PopUpManager.Instance != null && PopUpManager.Instance.IsWindowOpen)
         return;

      // Block NPC interaction while narrative overlay/tutorial UI is active
      if (NarrativeOverlayUI.Instance != null && NarrativeOverlayUI.Instance.IsBusy())
         return;

      // Prevent opening dialogue if one is already active
      if (NPCDialogueUI.Instance != null && NPCDialogueUI.Instance.IsDialogueActive()) return;

        if (dialogueUI != null)
            dialogueUI.ShowDialogue(currentNPC, currentScenario);
    }

   // Resolves the player's choice: checks costs, picks a weighted random
   // outcome, deducts ENTRY costs (pearlCost/oreCost on the Choice), and
   // returns the outcome. The outcome's pearlChange is NOT applied here —
   // it is applied later by the UI when the result text is displayed.
   public EncounterOutcome ProcessChoice(int choiceIndex)
   {
      if (currentScenario == null || choiceIndex >= currentScenario.choices.Length)
         return null;

      Choice selectedChoice = currentScenario.choices[choiceIndex];
      var inv = InventoryManager.Instance;

      // ── Cost check (before anything is deducted) ─────────────────────
      if (inv != null)
      {
         if (selectedChoice.pearlCost > 0 && inv.pearlCount < selectedChoice.pearlCost)
         {
            return new EncounterOutcome
            {
               resultDialogues = new DialogueLine[]
                {
                        new DialogueLine("You don't have enough pearls...", ExpressionType.Neutral)
                },
               pearlChange = 0,
               isPositive = false,
               keepNpcAfterClose = true
            };
         }

         if (selectedChoice.oreCost > 0 && inv.oreCount < selectedChoice.oreCost)
         {
            return new EncounterOutcome
            {
               resultDialogues = new DialogueLine[]
                {
                        new DialogueLine("You don't have enough ore...", ExpressionType.Neutral)
                },
               pearlChange = 0,
               isPositive = false,
               keepNpcAfterClose = true
            };
         }
      }

      // ── Deduct entry costs (the bet / trade cost on the Choice) ──────
      if (inv != null)
      {
         if (selectedChoice.pearlCost > 0)
         {
            bool spent = inv.TrySpendPearl(selectedChoice.pearlCost);
            if (!spent) return null;
         }

         if (selectedChoice.oreCost > 0)
         {
            bool spent = inv.TrySpendOre(selectedChoice.oreCost);
            if (!spent)
            {
               if (selectedChoice.pearlCost > 0)
                  inv.TryAddPearl(selectedChoice.pearlCost);
               return null;
            }
         }
      }

      // ── Roll a weighted random outcome ───────────────────────────────
      EncounterOutcome result = GetWeightedRandomOutcome(
          selectedChoice.outcomes,
          selectedChoice.outcomeWeights
      );

      // NOTE: result.pearlChange is NOT applied here.
      // The UI calls ApplyOutcomeReward() when the result text finishes.

      Debug.Log($"[NPC] Choice {choiceIndex}: Spent {selectedChoice.pearlCost}P {selectedChoice.oreCost}O → Pending {result.pearlChange:+#;-#;0}P");

      return result;
   }

   // Selects one outcome from the array using the corresponding weights
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

   // Called by the dialogue UI AFTER the result text has been displayed.
   // Applies the pearl reward or penalty from the outcome.
   public void ApplyOutcomeReward(EncounterOutcome outcome)
   {
      if (outcome == null) return;

      var inv = InventoryManager.Instance;
      if (inv == null) return;

      if (outcome.pearlChange > 0)
      {
         bool success = inv.TryAddPearl(outcome.pearlChange);
         if (success)
            Debug.Log($"[NPC] Gained {outcome.pearlChange} pearls.");
         else
            Debug.LogWarning($"[NPC] Could not add {outcome.pearlChange} pearls (at max?).");
      }
      else if (outcome.pearlChange < 0)
      {
         int amount = -outcome.pearlChange;
         bool success = inv.TrySpendPearl(amount);
         if (success)
            Debug.Log($"[NPC] Lost {amount} pearls.");
         else
            Debug.LogWarning($"[NPC] Could not spend {amount} pearls (insufficient?).");
      }
   }

   // Called by the dialogue UI when the conversation ends
   public void OnDialogueEnded(bool shouldDespawnNpc = true)
    {
      if (shouldDespawnNpc)
         DespawnNPC();
    }

    // Returns the correct portrait sprite for a given expression type.
    // Falls back to portraitNeutral if the specific sprite is not assigned.
    public Sprite GetPortraitForExpression(NPCProfile npc, ExpressionType expression)
    {
        return expression switch
        {
            ExpressionType.Happy     => npc.portraitHappy     ?? npc.portraitNeutral,
            ExpressionType.Angry     => npc.portraitAngry     ?? npc.portraitNeutral,
            ExpressionType.Surprised => npc.portraitSurprised ?? npc.portraitNeutral,
            ExpressionType.Thinking  => npc.portraitThinking  ?? npc.portraitNeutral,
            ExpressionType.Special   => npc.portraitSpecial   ?? npc.portraitNeutral,
            _                        => npc.portraitNeutral
        };
    }

   // Returns the first NPC profile matching the requested personality.
   public NPCProfile GetProfileByPersonality(NPCPersonality personality)
   {
      if (npcProfiles == null)
         return null;

      foreach (NPCProfile currentProfile in npcProfiles)
      {
         if (currentProfile != null && currentProfile.personality == personality)
            return currentProfile;
      }

      return null;
   }

   // ─────────────────────────────────────────────────────────────────────
   //  Public getters
   // ─────────────────────────────────────────────────────────────────────

    public NPCProfile GetCurrentNPC()                  => currentNPC;
    public EncounterScenario GetCurrentScenario()      => currentScenario;
    public bool HasActiveNPC()                         => hasActiveNPC;
}


// Attached at runtime to the NPC prefab instance to detect mouse clicks
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