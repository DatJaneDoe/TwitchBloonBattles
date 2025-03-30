using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Xml.Schema;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.Helpers;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using TwitchBloonBattles;
using Harmony;
using HarmonyLib;
using Il2CppAssets.Scripts;
using Il2CppAssets.Scripts.Data;
using Il2CppAssets.Scripts.Data.Boss;
using Il2CppAssets.Scripts.GameEditor.UI;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Models.Bloons.Behaviors;
using Il2CppAssets.Scripts.Models.Rounds;
using Il2CppAssets.Scripts.Models.Towers.Mods;
using Il2CppAssets.Scripts.Simulation;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.Bridge;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using Il2CppGeom;
using Il2CppNinjaKiwi.GUTS;
using Il2CppSystem.Runtime.Serialization.Formatters.Binary;
using MelonLoader.Utils;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using UnityEngine;
using UnityEngine.UI;
using static TwitchBloonBattles.Bosses;
using HarmonyPatch = HarmonyLib.HarmonyPatch;
using Random = System.Random;
using Il2CppInterop.Common;
using Il2CppInterop.Runtime;
using MelonLoader.NativeUtils;
using System.Runtime.InteropServices;
using AccessTools = HarmonyLib.AccessTools;
using Il2CppAssets.Scripts.Simulation.Towers;
using static Il2CppAssets.Scripts.Simulation.Bloons.Bloon;
using Il2CppAssets.Scripts.Models.TowerSets;
using HarmonyPrefix = HarmonyLib.HarmonyPrefix;
using Il2CppAssets.Scripts.Utils;
using static MelonLoader.MelonLogger;
using Il2CppAssets.Scripts.Unity.UI_New;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Simulation.Objects;
using Octokit;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;

[assembly: MelonInfo(typeof(TwitchBloonBattles.UIFunctions), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace TwitchBloonBattles;

[HarmonyPatch]
public class UIFunctions : BloonsTD6Mod
{
    public static ModHelperPanel panel;




    public const string YELLOW = "<color=#ECE63D>";
    public const string RED = "<color=#D9544D>";
    public const string GREEN = "<color=#00BD45>";
    public const string WHITE = "<color=#FFFFFF>";
    public const string ORANGE = "<color=#FFA500>";
    public const string BLUE = "<color=#0000FF>";
    public const string PURPLE = "<color=#800080>";
    public const string NAVY = "<color=#FFFF80>";
    public const string GOLD = "<color=#FFD700>";

    public const string CYAN = "<color=#00FFFF>";

    public static Dictionary<int, Dictionary<int, BloonSend>> sends = new Dictionary<int, Dictionary<int, BloonSend>>();

    public static Dictionary<int, BloonSend> activeSends = new Dictionary<int, BloonSend>();
    public static ModSettingBool EasierStart = new ModSettingBool(true)
    {
        description = "Gives some extra starting cash and hearts at the start. The early game is brutal if chat is sending Bloons non-stop"

    };

    public static ModSettingString Username = new ModSettingString("Username")
    {
        description = "Insert your Twitch username"


    };
    public static ModSettingString BotToken = new ModSettingString("BotToken")
    {
        description = "Get an access token from " + RED + "https://twitchtokengenerator.com" + WHITE + " then copy and paste it here. It will automatically try to connect and put a message in the console"


    };
    public static ModSettingString TokenGeneratorLinkYouCanCopy = new ModSettingString("https://twitchtokengenerator.com")
    {
        description = "This setting does nothing. Its just so you can easily copy and paste the token generator link"


    };
  
    public static ModHelperPanel mainBossPanel;
    public static ModHelperPanel infoPanel;


    public static bool activeBossVote = false;
    public static bool eliteBossVote = false;
    public static List<string> votedForBoss = new List<string> { };
    public static double[] pointPools = new double[] { 0, 0, 0, 0, 0, 0 , 0, 0 , 0, 0, 0, 0, 0,0,0,0};
    public static int[] bossVotes = [0, 0, 0, 0, 0, 0, 0,0,0,0,0,0,0,0,0];
    public static BloonSend[] bloonSends = Array.Empty<BloonSend>();
    public static double timeDelay = 0;
    public static int ecoTicks = 0;
    public static int time = 0;
    public static bool fixedUI = false;

    public static string inputToken = "";

    public static TwitchClient? client;

    public static Dictionary<string, Chatter> chatters = new Dictionary<string, Chatter>();

    public static Dictionary<int, SlotInfo> slotInfos = new Dictionary<int, SlotInfo>();

    public static Dictionary<int, SlotInfo> bossSlotInfos = new Dictionary<int, SlotInfo>();

    public static Dictionary<string, int> baseBloonCosts = new Dictionary<string, int>();


    public static ModSettingInt EcoMultiplier = new ModSettingInt(100)
    {
        description = "Percent multiplier for eco of non-subscribers. A higher value will give your chat higher eco thus more Bloon sends and vice versa. A value of 0 disables non-subscribers from interacting with this mod"


    };



public static ModSettingInt SubscriberEcoMultiplier = new ModSettingInt(200)

    {
        description = "Same as Eco multiplier but for subscribers, incase you want to give them special treatment. Example: Setting Eco multiplier to 100 and this to 200 will result in non-subscribers having normal eco while subscribers get double eco"


    };

public readonly static ModSettingInt PointsPerBit = new ModSettingInt(10)
    {
        description = "If someone gives you bits, award them this many points per bit"

    };

    public static ModSettingBool ScalePointsPerBit = new ModSettingBool(true)
    {
        description = "When someone awards bits, multiply the points they get by the number of chatters"

    };

    public static ModSettingBool OnlySubsCanVoteBoss = new ModSettingBool(false)
     {
        description = "When enabled, only subs can vote for bosses. If disabled, everyone can vote"

    };
public static ModSettingBool SimulateBots = new ModSettingBool(false)
     {
        description = "Makes the mod pretend bots are saying commands in the chat. Works even while not streaming."

    };
    public static ModSettingInt NumberOfBots = new ModSettingInt(100)
    {
        min = 1,
        description = "How many bots?",
        max = 200
    };
    
    public static ModSettingInt NumberOfSubBots = new ModSettingInt(10)
    {
        min = 1,
     
        max = 200,
        description = "How many bots are subscribers?"

    };


    public static ModSettingInt CostMultiplier = new ModSettingInt(100)
    {
        description = "Percent multiplier for bloon send costs. A higher value will make sends more expensive, thus fewer sends"

};

public static ModHelperPanel upperUI;
    public static ModHelperImage upperInfoUI;
    public static ModHelperPanel upperBossUI;

    public static ModHelperText bossVoteText;
    public static ModHelperText bossInfoText;
    public static ModHelperImage uiImage;
    public ModHelperPanel retryButton;

    public static ModSettingEnum<BossAllowMode> AllowedBosses = new ModSettingEnum<BossAllowMode>(BossAllowMode.NormalAndElite)
    {
        description = "Should bosses be allowed to be sent by chat?"

    };
   
    public static string[] validInputs = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

    public static List<ModHelperText> recentEco = new List<ModHelperText> { };

    

    public static int relativeRound = 1;

    public static float[] ecoMult = new float[] { 1,1, 0.8f, 0.8f, 0.5f, 0.2f, 0.2f, 0,0,0};

    public static Color[] multColors = new Color[] {Color.green, Color.green, Color.yellow, Color.yellow, new Color(1,0.5f,0), new Color(0.6f, 0.4f, 0), new Color(0.6f, 0.4f, 0) ,  Color.red, new Color(1, 0f, 1), new Color(0.7f, 0f, 0.7f) };

    public static List<BloonGroupModel> quene = new List<BloonGroupModel>();

    public static Random random = new Random();

    public static int recentIndex = 0;

    public static Dictionary<string, int> counters = new Dictionary<string, int>();

    public static string[] bosses = new string[] { "Bloonarius", "Lych","Vortex", "Phayze", "Dreadbloon","Blastapopoulos" };

    public static int validTime = 0;

    public static ModHelperImage ecoTimer;

    public static List<string> toOverlay = new List<string>() { };


    public enum Modifier
    {
       None,
       Boss,
       Speed,
        Shield,
        Lock,
    }

    public enum BossAllowMode
    {
        None,
        OnlyNormal,
        NormalAndElite,
    }

    public override void OnApplicationStart()
    {
       
        base.OnApplicationStart();
     //     AppDomain.CurrentDomain.Load(Path.Combine(MelonEnvironment.GameRootDirectory, @"UserLibs/TwitchLib.Communication.dll"));
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Bloon_DamageDelegate(
        nint @this,
        float totalAmount,
        nint projectile,
        byte distributeToChildren,
        byte overrideDistributeBlocker,
        byte createEffect,
        nint tower,
        int immuneBloonProperties,
        int originalImmuneBloonProperties,
        byte canDestroyProjectile,
        byte ignoreNonTargetable,
        byte blockSpawnChildren,
        byte ignoreInvunerable,
        NullableInt powerActivatedByPlayerId,
        nint methodInfo
    );

    private static NativeHook<Bloon_DamageDelegate> BloonDamageHook;
    private static Bloon_DamageDelegate DamageDelegate;

    public override unsafe void OnLateInitializeMelon()
    {
        nint originalMethod = *(nint*)
            (nint)
                Il2CppInteropUtils
                    .GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                        AccessTools.Method(typeof(Bloon), nameof(Bloon.Damage))
                    )
                    .GetValue(null);

        DamageDelegate = Damage;

        nint delegatePointer = Marshal.GetFunctionPointerForDelegate(DamageDelegate);

        NativeHook<Bloon_DamageDelegate> hook = new NativeHook<Bloon_DamageDelegate>(
            originalMethod,
            delegatePointer
        );

        hook.Attach();

        BloonDamageHook = hook;
    }

    private void Damage(
        nint @this,
        float totalAmount,
        nint projectile,
        byte distributeToChildren,
        byte overrideDistributeBlocker,
        byte createEffect,
        nint tower,
        int immuneBloonProperties,
        int originalImmuneBloonProperties,
        byte canDestroyProjectile,
        byte ignoreNonTargetable,
        byte blockSpawnChildren,
        byte ignoreInvunerable,
        NullableInt powerActivatedByPlayerId,
        nint methodInfo
    )
    {
        var thisValue = IL2CPP.PointerToValueGeneric<Bloon>(@this, false, false); //example of getting a non-valuetype parameter value
       
        if (counters["Shield"] > 0)
        {

          
          if(totalAmount >= 5)
            {
                totalAmount *= 0.8f;
            }
            else
            {
                totalAmount--;
            }
        }
        BloonDamageHook.Trampoline(
            @this,
            totalAmount,
            projectile,
            distributeToChildren,
            overrideDistributeBlocker,
            createEffect,
            tower,
            immuneBloonProperties,
            originalImmuneBloonProperties,
            canDestroyProjectile,
            ignoreNonTargetable,
            blockSpawnChildren,
            ignoreInvunerable,
            powerActivatedByPlayerId,
            methodInfo
        );

       
    }

    private struct NullableInt
    {
        public byte HasValue;
        public int Value;
    }
 

    public class Chatter
    {
        public string name;
        public int lastSent = 0;
        public double points = 0;
        public int eco = 50;
      
        public Chatter(string name)
        {
            this.name = name;
        }


    }

   

        public void UpdateUpperUI()
        {

            if (upperUI == null)
            {



                upperUI = InGame.instance.mapRect.gameObject.AddModHelperPanel(new Info("UIPanel",
                 0, 3, 0, 0, new UnityEngine.Vector2(0.38f, .95f), new UnityEngine.Vector2(.5f, .5f)), "");

                uiImage = upperUI.AddImage(new Info("ImageSlot", 0, 3, 3000, 250), ModContent.GetSpriteReference<UIFunctions>( "descriptionBox").guidRef);

                for (int i = 0; i < 10; i++)
                {
                    int y = 0;
                    int x = i;
                    if(i > 4)
                    {
                        x -= 5;
                        y++;
                    }
                    CreateSlot(i, x-1, y );

                }

                upperBossUI = InGame.instance.mapRect.gameObject.AddModHelperPanel(new Info("BossVotePanel",
                 0, 3, 0, 0, new UnityEngine.Vector2(0.38f, .95f), new UnityEngine.Vector2(.5f, .5f)),"");
           
              upperBossUI.AddImage(new Info("ImageSlot", 0, 0, 3000, 250), ModContent.GetSpriteReference<UIFunctions>("descriptionBox").guidRef);

                for (int i = 0; i < 6; i++)
                {
                
                    CreateBossSlot(i);

                }
               bossVoteText = upperBossUI.AddText(new BTD_Mod_Helper.Api.Components.Info("BossVoteText", -1200, 0, 600, 600), "Boss Vote \n\n Time Left:", 50, Il2CppTMPro.TextAlignmentOptions.Midline);

                upperBossUI.Hide();
            recentEco.Clear();
                UpdateRecentEco();

                infoPanel = InGame.instance.mapRect.gameObject.AddModHelperPanel(new Info("InfoUIPanel",
                 0, 0, 350, 150, new UnityEngine.Vector2(0.5f, .5f), new UnityEngine.Vector2(.5f, .5f)), ModContent.GetSpriteReference<UIFunctions>("descriptionBox").guidRef);
                infoPanel.Background.raycastTarget = false;
           
                bossInfoText = infoPanel.AddText(new BTD_Mod_Helper.Api.Components.Info("BossInfoText",0,0,350,150), "This text box", 45, Il2CppTMPro.TextAlignmentOptions.Midline);
            
            
        
                infoPanel.Hide();

            upperUI.Hide();


          var u = InGame.instance.mapRect.gameObject.AddModHelperPanel(new Info("SendInfoPanel",
                 0, 3, 0, -100, new UnityEngine.Vector2(0.38f, .95f), new UnityEngine.Vector2(.5f, .5f)), "");

            upperInfoUI = u.AddImage(new Info("ImageSlot", 0, 0, 3000, 350), ModContent.GetSpriteReference<UIFunctions>("descriptionBox").guidRef);
            upperInfoUI.AddText(new Info("ImageSlot", 0, 0, 2800, 320), "Type a number from 0-9 to allocate your points to one of the Bloon Sends on the UI. When the required points are reached the Bloon Send is quened.\n\n You gain points periodically, and can increase your income rate by allocating points to weaker sends. Stronger sends such as bosses wont increase your income but can potentially defeat the streamer \n\n Type ? and your points and eco will show next to the Bloon Sends shortly.", 40);

        }

        
            int highest = 0;

        if (slotInfos.Count == 10 && activeSends.Count == 10)
        {
            for (int i = 0; i < 10; i++)
            {

                if (activeBossVote)
                {
                    try
                    {
                        upperBossUI.Show();
                    }
                    catch
                    {

                    }

                    if (i <= 5)
                    {

                        if (counters["BossVote"] > 300)
                        {
                            bossSlotInfos[i].text.SetText("?????");
                        }
                        else
                        {
                            bossSlotInfos[i].text.SetText(bossVotes[i] + "");

                        }
                    }


                }
                else
                {
                    try
                    {
                        upperBossUI.Hide();
                    }
                    catch
                    {

                    }
                    var slot = slotInfos[i];
                    if (activeSends[i].modifier == Modifier.Lock)
                    {
                        slot.text.Text.text = "N/A";
                    }
                    else
                    {

                        double cost = activeSends[i].baseCost * (chatters.Count + 1) * (CostMultiplier / 100f);
                        if (activeSends[i].modifier == Modifier.Speed || activeSends[i].modifier == Modifier.Shield)
                        {
                            cost *= Math.Pow(1.04f, relativeRound);
                        }

                        activeSends[i].cost = cost;
                        slot.text.Text.text = FormatNumber((long)pointPools[i]) + "/" + FormatNumber((long)cost);

                    }
                }






            }
        }
        
    }
 
    
    public static void MessageFunctions(string name, bool isSubscriber, string text)
    {
        bool isSub = false;

        if (!chatters.ContainsKey(name))
        {
            if (validInputs.Contains(text) || text == "?")
            {
                chatters[name] = new Chatter(name);

                recentEco[recentIndex % 6].Text.text = chatters[name].name + " Joined!";
                chatters[name].points += relativeRound * 100;
                chatters[name].eco += relativeRound;
                recentIndex++;
            }
            
        }
        else
        {
            Chatter chatter = chatters[name];



            int difference = ecoTicks - chatter.lastSent;

            chatter.lastSent = ecoTicks;

            double eco = chatter.eco * difference;

            // Eco given is multiplied by the eco multiplier in the settings
            if (isSubscriber || name == Username)
            {
                isSub = true;
                eco *= SubscriberEcoMultiplier * 0.01f;

            }
            else
            {
                eco *= EcoMultiplier * 0.01f;
            }



            chatter.points += eco;

            if (validInputs.Contains(text))
            {
                // Sends the chatters points to the respective pool
                DivertPoints(chatter, int.Parse(text), isSub);

            }
            else
            {
                if (((chatter.points > 0) || (name == Username)) && text == "?")
                {
                    // If the message is ? and  the chatter has more points than 0, hasnt sent a message recently, or is the user, display their eco.
                    AddRecentText(EcoText(chatter.name, (int)chatter.points, chatter.eco, isSubscriber));
                }
            }
        }
    }

    
    public static void DivertPoints(Chatter chatter, int index, bool isSub)
    {
        if (activeBossVote)
        { 

            // If theres an active boss vote, chat inputs are changed to boss votes instead and do not deduct the chatters points.
            if (!OnlySubsCanVoteBoss || isSub)
            {
                if (!votedForBoss.Contains(chatter.name) && index <= 5 && bossSlotInfos.Count == 6)
                {
                    votedForBoss.Add(chatter.name);
                    bossVotes[index]++;
                    try
                    {
                        float percVoted = (float)votedForBoss.Count() / (float)(chatters.Count+1);

                        if (counters["BossVote"] > 300)
                        {




                            float fixedCounter = (1 - percVoted) * 1000f;

                            counters["BossVote"] = (int)Math.Min(counters["BossVote"], fixedCounter);



                        }

                    }
                    catch
                    {

                    }
                }
            }
            
        }
        else
        {
            if (activeSends.ContainsKey(index))
            {
                if (activeSends[index].modifier != Modifier.Lock)
                {
                    // If the active send is locked, ignore

                    double deduction = Math.Min( chatter.points, activeSends[index].cost);
                 


                    pointPools[index] += deduction;


                    // Give the player eco based on points spent
                    chatter.eco += (int)(ecoMult[index] * 0.01f * deduction);
                    if (chatter.eco < 50)
                    {
                        chatter.eco = 50;
                    }
                    chatter.points -= deduction;
                }
            }
        }

    }

    public class BloonSend
    {
        public double baseCost = 100;
       
        public BloonGroupModel groupToSend = new BloonGroupModel("Red", "Green", 1, 1, 1);
        public bool isBossVote = false;
        public int slot = 1;
        public int roundAvailable = 1;
        public double cost = 500;
        public bool isClump = false;
        public bool isSpeed = false;
        public bool isElite = false;
        public int tier = 0;
        public Modifier modifier = Modifier.None;
        public BloonSend(string bloon, int slot, int roundAvailable) { 
        
            if(bloon.Contains("Boss"))
            {
                modifier = Modifier.Boss;
                if (bloon.Contains("Elite"))
                {
                    isElite = true;
                    tier = int.Parse(bloon.Replace("BossElite", ""));
                }
                else
                {
                    tier = int.Parse(bloon.Replace("Boss", ""));
                }
                
            }
            if (bloon.Contains("Clump"))
            {
                bloon = bloon.Replace("Clump", "");
                isClump = true;
                groupToSend = new BloonGroupModel(bloon, bloon, 1, 10, 10);
            }
            else if (bloon.Contains("Speed"))
            {
                modifier = Modifier.Speed;
              
            }
            else if (bloon.Contains("Shield"))
            {
                modifier = Modifier.Shield;
               
            }
            else if (bloon.Contains("Lock"))
            {
                modifier = Modifier.Lock;

            }
            else
            {
                groupToSend = new BloonGroupModel(bloon, bloon, 1, 1, 1);
            }

            float mult = 1;
            string name = bloon;

            if (modifier == Modifier.None)
            {
               
                if (bloon.Contains("Regrow"))
                {
                    mult *= 1.2f;
                    name = name.Replace("Regrow", "");
                
                }
                if (bloon.Contains("Camo"))
                {
                    mult *= 1.4f;
                    name = name.Replace("Camo", "");
                }
                if (bloon.Contains("Fortified"))
                {
                    mult *= 1.6f;
                    name = name.Replace("Fortified", "");
                }
                if (isClump)
                {
                    mult *= 8;
                }

            }
           
                this.baseCost = baseBloonCosts[name] * mult * 2;
            

            this.slot = slot;
            this.roundAvailable = roundAvailable;

            sends[roundAvailable][slot] = this;
        }

    }



    public static T StartMonobehavior<T>() where T : MonoBehaviour
    {
        var obj = InGame.instance.GetInGameUI().AddComponent<T>();

        return obj as T;
    }
    
    
    

    
         

    public class SlotInfo
    {
        public ModHelperText number;
        public ModHelperText text;
        public ModHelperButton image;
        public ModHelperText modifier;
        public ModHelperImage bg;
        public SlotInfo(ModHelperText text, ModHelperButton image, ModHelperText modifier, ModHelperImage bg)
        {

            this.text = text;
            this.image = image;
            this.modifier = modifier;
            this.bg = bg;
        }

    }

    public static ModHelperPanel CreateSlot(int index, int x, int y)
    {

      
        int baseX = -755;
        int baseY = 65;
        int incrementX = 475;
        int incrementY = -120;

        ModHelperPanel blankSlot = upperUI.Background.gameObject.AddModHelperPanel(new BTD_Mod_Helper.Api.Components.Info("PoolSlot" + index, baseX + (incrementX * x),baseY + (incrementY * y), incrementX - 25, 100));

        Il2CppSystem.Action sent = (Il2CppSystem.Action)delegate ()
        {

            
                pointPools[index] += bloonSends[index].cost;
            
        };
        

        var bg = blankSlot.AddImage(new BTD_Mod_Helper.Api.Components.Info("PoolSlotImage" + index, incrementX - 25, 100), "bgBarBlank");
       
        bg.Image.sprite = ModContent.GetSprite<UIFunctions>("bgBarBlank");
        bg.Image.color = multColors[index];

        var bg2 = bg.AddImage(new BTD_Mod_Helper.Api.Components.Info("PoolSlotImage2" + index, incrementX - 25, 100), ModContent.GetSprite<UIFunctions>("bgBarGold"));

        bg2.Image.raycastTarget = false;
        bg2.Image.rectTransform.localScale = new Vector3(1f, 0, 1);

        bg2.Image.rectTransform.localPosition = new Vector3(0f, -50f, 0f);
     //   bg2.Image.transform.lo = new Vector3(0,-50,0);

        var icon = blankSlot.AddButton(new BTD_Mod_Helper.Api.Components.Info("Icon" + index,-110, 0, 140, 140), Game.instance.model.GetBloon("Red").icon.GetGUID(), sent);

        var modifier = blankSlot.AddText(new BTD_Mod_Helper.Api.Components.Info("Modifier" + index, -75, -20, 200, 200), "10x", 40, Il2CppTMPro.TextAlignmentOptions.Center);
        
        modifier.enabled = false;
        modifier.Text.outlineWidth *= 1.5f;
        var send = blankSlot.AddText(new BTD_Mod_Helper.Api.Components.Info("Text", -190, 0, 600, 100), index + "", 60);

       
        var text = blankSlot.AddText(new BTD_Mod_Helper.Api.Components.Info("Text", 80, 0, 600, 100), "100K/100K", 50, Il2CppTMPro.TextAlignmentOptions.Midline);

     //   var b = text.AddButton(new BTD_Mod_Helper.Api.Components.Info("Button", -80, 0, incrementX - 25, 100), "b", action);
     //   b.Image.sprite = ModContent.GetSprite<BossHandler>("blank");


        slotInfos[index] = new SlotInfo(text, icon, modifier, bg2);
        
        return blankSlot;
    }
    public static ModHelperPanel CreateBossSlot(int index)
    {

        int x = index;
        int baseX = -755;
        int baseY = 65;
        int incrementX = 400;
        
        ModHelperPanel blankSlot = upperBossUI.Background.gameObject.AddModHelperPanel(new BTD_Mod_Helper.Api.Components.Info("PoolSlot" + index, baseX + (incrementX * x), 0, incrementX - 25, 1000));

        var bg = blankSlot.AddImage(new BTD_Mod_Helper.Api.Components.Info("PoolSlotImage" + index, 0, 0,  incrementX - 125, 250), ModContent.GetSprite<UIFunctions>("bgBar"));

        //   var bg2 = bg.AddImage(new BTD_Mod_Helper.Api.Components.Info("PoolSlotImage2" + index, incrementX - 25, 100), ModContent.GetSprite<BossHandler>("bgBarGold"));

        Il2CppSystem.Action action = (Il2CppSystem.Action)delegate ()
        {

            MessageFunctions(Username, true, index + "");


        };

        var icon = bg.AddButton(new BTD_Mod_Helper.Api.Components.Info("Icon" + index, 0, 0, 150, 150), Game.instance.model.GetBloon(bosses[index]+ "1").icon.GetGUID(), action);


 

        var send = icon.AddText(new BTD_Mod_Helper.Api.Components.Info("Text", 0, 90, 600, 100), index + "", 60);



        var text = icon.AddText(new BTD_Mod_Helper.Api.Components.Info("Text", 0, -90, 600, 100), "?????", 50, Il2CppTMPro.TextAlignmentOptions.Midline);

       bossSlotInfos[index] = new SlotInfo(text, icon, null, null);
      
        return blankSlot;
    }
    public static void ScaleImage(Image image, float value, float max)
    {
        value = Math.Min(value, max);
        float scale = value / max;

        float basePos = -50 + scale * 50;
        float baseScale = scale;

        image.rectTransform.localScale = new Vector3 (1, baseScale, 1);

        image.rectTransform.localPosition = new Vector3(image.rectTransform.localPosition.x, basePos, 0);

    }
    public static void ScaleTimer(Image image, float value, float max)
    {
        value = Math.Min(value, max);
        float scale = value / max;

        float basePos = -100 + (scale * 100);
        float baseScale = scale;

        image.rectTransform.localScale = new Vector3(1, baseScale, 1);

        image.rectTransform.localPosition = new Vector3(image.rectTransform.localPosition.x, basePos, 0);

    }

    public static string FormatNumber(long num)
    {
        // Ensure number has max 3 significant digits (no rounding up can happen)
        if (num > 0)
        {
            long i = (long)Math.Pow(10, (int)Math.Max(0, Math.Log10(num) - 2));

            num = num / i * i;

            if (num >= 1000000000)
                return (num / 1000000000D).ToString("0.##") + "B";
            if (num >= 1000000)
                return (num / 1000000D).ToString("0.##") + "M";
            if (num >= 1000)
                return (num / 1000D).ToString("0.##") + "K";

            return num.ToString("#,0");
        }
        else
        {
            return "0";
        }
        
    }
    public static string EcoText(string name, int points, int eco, bool isSub)
    {

        if (isSub)
        {
            name = CYAN + name;
        }
       
        return name + " " +  YELLOW + FormatNumber(points) + GREEN + " " + FormatNumber(eco) + "^";
    }
    public static void UpdateRecentEco()
    {

        Il2CppSystem.Action action = (Il2CppSystem.Action)delegate ()
        {

            


                Il2CppSystem.Action<int> action2 = (Il2CppSystem.Action<int>)delegate (int number)
                {

                    if (number >= 0 && number <= 9)
                    {
                        if (number >= 5)
                        {
                            pointPools[number] += activeSends[number].cost;
                        }
                        else
                        {
                            quene.Add(activeSends[number].groupToSend);
                        }
                    }
                };
                PopupScreen.instance.ShowSetValuePopup("Send Bloons", "Type a number from 0-9 to initate a Bloon send", action2, 0);


            
        };


        ModHelperPanel blankSlot = upperUI.Background.gameObject.AddModHelperPanel(new BTD_Mod_Helper.Api.Components.Info("PoolSlot", 1190, 0, 550, 240));
            var img = blankSlot.AddImage(new BTD_Mod_Helper.Api.Components.Info("PoolSlotImage", 0, 0, 550, 240),"");


        img.Image.sprite = ModContent.GetSprite<UIFunctions>("bgBar");
        img.Image.raycastTarget = false;


        ecoTimer = blankSlot.AddImage(new BTD_Mod_Helper.Api.Components.Info("PoolSlotImage",300,90, 70), ModContent.GetSprite<UIFunctions>("circle"));

        ecoTimer.Image.type = UnityEngine.UI.Image.Type.Filled;
        ecoTimer.Image.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
        ecoTimer.Image.fillOrigin = 90;
        ecoTimer.Image.fillClockwise = true;
      //  ecoTimer.Image.color = new UnityEngine.Color(1, 1f, 0f, 0.3f);
       

        int baseY = -100;
            int offY = 40;
            for (int i = 0; i < 6; i++)
            {
                ModHelperText poolText = blankSlot.AddText(new BTD_Mod_Helper.Api.Components.Info("PoolSlot" + i, 0, baseY + (offY * i), 550, 40), "", 35, Il2CppTMPro.TextAlignmentOptions.Midline);
            poolText.AddButton(new BTD_Mod_Helper.Api.Components.Info("PoolSlot", 550, 40), "blank", action).Image.sprite = ModContent.GetSprite<UIFunctions>("blank");
    
            recentEco.Add(poolText);
            }
        
            
        
    }
    public static void AddRecentText(string newText)
    {
     
        recentEco[recentIndex % 6].Text.text = newText;
        recentIndex++;

     
    }
    public static void RunBots()
    {
        int bot = random.Next(0,200);
        // Bot actions and which types are picked randomly
        if (bot <= NumberOfBots)
        {
            int mult = 1;
            bool isSub = bot <= NumberOfSubBots;
            


            bool isAggression = random.Next(0, 100) <= 20 + ( Math.Min(InGame.instance.bridge.GetCurrentRound(), 100)/5f);

            int roll = random.Next(0, 6);

           
            // If a bot "decides" to be aggressive, picks a stronger send
            if (isAggression)
            {
                roll = random.Next(5, 10);
            
            }
            
             
            

            string message = "" + roll;
           
            if (roll == 5 && !isAggression)
            {
                // If a boss picks 5 without aggression it simply types ? instead
                message = "?";
            }

            string name = "Bot";
            if (isSub)
            {
                name = "Subbed" + name;
            }
            else
            {
                name = "Reg" + name;

            }
           MessageFunctions(name + bot, isSub, message);
               
            
        }
    }
    [HarmonyLib.HarmonyPatch(typeof(InGame), nameof(InGame.GetContinueCost))]
    public class NoCostContinue
    {
        [HarmonyLib.HarmonyPostfix]
        public static void Postfix(ref KonFuze __result)
        {

            __result = new KonFuze(0);

        }

    }
    public override void OnNewGameModel(GameModel gameModel)
    {
        // This mod is difficult early on due to excessive sends so the player is given some extra starting cash and lives
        gameModel.startRound = 1;
        gameModel.startingHealth += 100;
        gameModel.cash += 1000;
        
    }
    public static Vector3 ToUnityPos(Vector3 vec)
    {

        vec.y *= -1f;

        vec = InGame.instance.GetUIFromWorld(vec, false);
       
        return vec;
    }
    public override void OnUpdate()
    {
        base.OnUpdate();

        

            
            if (client != null && InGame.instance != null)
            {
            try
            {
                if (InGame.instance.bridge.simulation.gameLost)
                {


                    var g = GameObject.Find("Canvas/DefeatPanel/Container/Buttons/ContinueButton/ContinueCost/");


                    if (g != null && retryButton == null)
                    {
                        retryButton = g.AddModHelperPanel(new BTD_Mod_Helper.Api.Components.Info("n", 330), VanillaSprites.MergeBtn);
                        retryButton.AddText(new BTD_Mod_Helper.Api.Components.Info("t", 400), "Free", 80);

                    }
                    GameObject.Find("UI/Popups/CommonPopup(Clone)/AnimatedObject/Layout/Buttons/ConfirmButton/CashInfo/").Hide();

                }
            }
            catch
            {

            }
            if (counters["FixUI"] > 0)
            {
                counters["FixUI"]--;
                if (counters["FixUI"] == 0)
                {

                    FixUI();


                    UpdateSendOptions();

                    UpdateUpperUI();
                    upperUI.Show();

                }
            }
            if (TimeManager.gamePaused)
                {

                // Do not run mod functions if the game is paused.
                }
                else
                {

               

                
                    // Increments time delay based on whether or not fast forward is active
                    if (TimeManager.FastForwardActive)
                    {
                        timeDelay += 3;
                    }
                    else
                    {
                        timeDelay++;
                    }
                    if (counters["BossVote"] > 0)
                    {
                         // If a boss vote is active, reduces the boss vote counter. Its not affected by fast forward
                        counters["BossVote"]--;
                        activeBossVote = true;

                        if (bossVoteText != null)
                        {
                            bossVoteText.SetText("Boss Vote \n\n Time Left: " + Math.Round((int)counters["BossVote"] / 60f));
                            if (counters["BossVote"] > 300)
                            {
                                for (int i = 0; i < 6; i++)
                                {

                                    bossSlotInfos[i].text.Text.color = Color.white;

                                }

                                bossVoteText.Text.color = Color.white;
                            }
                            else
                            {
                                bossVoteText.Text.color = Color.red;

                                int highest = 0;
                                for (int i = 0; i < 6; i++)
                                {
                                    if (bossVotes[i] > highest)
                                    {
                                        highest = bossVotes[i];

                                    }
                                }
                                for (int i = 0; i < 6; i++)
                                {
                                    if (bossVotes[i] == highest)
                                    {
                                        bossSlotInfos[i].text.Text.color = Color.yellow;

                                        if (counters["BossVote"] <= 0)
                                        {
                                        // When the boss vote finishes, empities the votes list
                                            bossVotes = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                                            votedForBoss = new List<string> { };
                                            activeBossVote = false;
                                            string bossToSpawn = bosses[i];
                                        int index = 8;

                                        // If it was an elite boss vote, adds elite to the boss to spawn
                                            if (eliteBossVote)
                                            {
                                                bossToSpawn += "Elite";
                                            index = 9;
                                            }
                                            InGame.instance.SpawnBloons(bossToSpawn + activeSends[index].tier, 1, 1);
                                        break;
                                        }
                                    }
                                    else
                                    {
                                        bossSlotInfos[i].text.Text.color = Color.white;
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        activeBossVote = false;
                    }
                    if (TimeManager.inBetweenRounds)
                    {
                        validTime = 0;
                    }
                   





                        
                    
                if (infoPanel != null)
                {
                    if (InGame.instance != null)
                    {
                        if (InGame.instance.bridge != null)
                        {
                            // If bot simulation is enabled, run test bots
                            if (SimulateBots)
                            {
                                RunBots();

                            }
                            Vector2 mouse = InGame.instance.inputManager.cursorPositionWorld;


                            
                            bool foundBoss = false;
                            bool updateUpperInfo = false;
                            // If the mouse is in the bottom left, show instructions
                            if(mouse.y < -115)
                            {
                                // If your mouse is at the top of the screen, show the info UI
                                if (counters["InfoUI"] < 1000)
                                {
                                    counters["InfoUI"] += 5;
                                    updateUpperInfo = true;
                                }
                            }
                            else
                            {
                                if (counters["InfoUI"] > 0)
                                {
                                    counters["InfoUI"] -= 5;
                                    updateUpperInfo = true;
                                }
                            }
                            if (counters["FixUI"] > 0)
                            {
                                updateUpperInfo = true;
                            }
                            if (updateUpperInfo)
                            {
                                int yCoord = Math.Min(counters["InfoUI"], 40);
                                upperInfoUI.gameObject.transform.localPosition = new Vector3(0, (yCoord * -10) + 370, 0);
                            }
                            foreach (BloonToSimulation sim in InGame.instance.bridge.GetAllBloons().ToList())
                            {
                                // Shows a tooltip for moab health
                                Bloon bloon = sim.GetBloon();
                                if(bloon.PercThroughMap() < 0)
                                {
                                    if (toOverlay.Contains(bloon.bloonModel.id))
                                    {
                                       // toOverlay is a list of Bloons that were quened. This applies the graphical indicator it was sent by chat
                                        var prefab = ModContent.CreatePrefabReference<TwitchIconSmall>();
                                        if (bloon.bloonModel.isMoab)
                                        {
                                         
                                            if (bloon.bloonModel.name.Contains("Bad"))
                                            {
                                                prefab = ModContent.CreatePrefabReference<TwitchIconLarge>();
                                            }
                                            else
                                            {
                                               prefab = ModContent.CreatePrefabReference<TwitchIconMedium>();
                                            }
                                        }
                                        var e = InGame.instance.bridge.simulation.SpawnEffect(prefab, Il2CppAssets.Scripts.Simulation.SMath.Vector3.zero, 180, 1, -1, Il2CppAssets.Scripts.Models.Effects.Fullscreen.No, new IRootBehavior( bloon.Behaviors.ToList()[0].Pointer), true, false, true);
                                     
                                        toOverlay.Remove(bloon.bloonModel.id);
                                     
                                    }
                                }
                                bool isBoss = bloon.bloonModel.isMoab;
                                float dist = Vector2.Distance(mouse, bloon.Position.ToVector2().ToUnity());
                               
                                if (dist < 30 && isBoss && mouse != new Vector2(0, 0))
                                {
                                    var pos = bloon.Position.ToVector3().ToUnity();
                                    pos.y -= 10;
                                    infoPanel.gameObject.transform.position = ToUnityPos(pos);
                                    string name = sim.GetBloon().bloonModel.baseId;
                                    if (sim.GetBloon().bloonModel.id.Contains("Elite")){
                                        name = "Elite " + name;
                                    }
                                    bossInfoText.SetText(name + "\n" + FormatNumber(bloon.health) + "/" + FormatNumber(bloon.bloonModel.maxHealth));
 
                                    infoPanel.RectTransform.sizeDelta = new Vector2(bossInfoText.Text.GetPreferredValues().x + 40, bossInfoText.Text.GetPreferredValues().y + 40);

                                    foundBoss = true;

                                }
                                if (counters["Speed"] > 0)
                                {
                                    // If the speed buff is active, bloons move faster
                                    // Bosses are affected less

                                    if (bloon.bloonModel.isBoss)
                                    {
                                        bloon.trackSpeedMultiplier = 1.25f;
                                    }
                                    else if (isBoss)
                                    {
                                        bloon.trackSpeedMultiplier = 1.5f;
                                    }
                                    else
                                    {
                                        bloon.trackSpeedMultiplier = 2f;
                                    }



                                }
                            }
                            if (foundBoss)
                            {
                                infoPanel.Show();
                            }
                            else
                            {
                                infoPanel.Hide();
                            }
                        }

                    }
                }

               
                float scale = (float)(time + (timeDelay/120f));
                if (ecoTimer != null)
                {
                    // Sets the eco timer fill amount proportionate to the progress towards distributing points
                    float fill = (float)(((time % 7) + (timeDelay / 120f)) / 7f);
                    if (validTime > 0)
                    {
                        if (fill >= 1)
                        {
                            UpdateUpperUI();
                            validTime--;

                            ecoTicks++;
                            time = 0;
                        }
                    }
                    else
                    {
                        fill = 0;
                    }
                   
                    ecoTimer.Image.fillAmount = fill;

                    
                   
                }

                if (timeDelay > 120)
                    {

                        UpdateUpperUI();
                     
                       
                        time++;

                   

                    // valid time is how long a round will send Bloons sent by chat and generate eco.
                    // This is so Chat cannot send Bloons endlessly during a round, preventing rounds from progressing.
                        if (validTime > 0 && activeSends.Count == 10)
                        {




                            for (int i = 0; i < 10; i++)
                            {
                                // Checks each point pool. If theres enough points, intiates the bloon sense
                                if (pointPools[i] >= activeSends[i].cost)
                                {
                                    slotInfos[i].text.Text.color = new Color(1, 1, 1);

                                     
                                    if (activeSends[i].modifier == Modifier.None)
                                    {
                                     // If the bloon send has no modifier, simply adds it to quene
                                        quene.Add(activeSends[i].groupToSend);

                                    }
                                    else if (activeSends[i].modifier == Modifier.Speed)
                                    {
                                    // If the speed buff pool is filled, temporarily activates the speed buff
                                        counters["Speed"] += 600;
                                        slotInfos[i].bg.Image.SetSprite(ModContent.GetSprite<UIFunctions>("bgBarGold"));

                                    }
                                    else if (activeSends[i].modifier == Modifier.Shield)
                                    {
                                    // If the shield buff pool is filled, temporarily activates the shield buff
                                    counters["Shield"] += 600;
                                        slotInfos[i].bg.Image.SetSprite(ModContent.GetSprite<UIFunctions>("bgBarGold"));

                                    }
                                    else if (activeSends[i].modifier == Modifier.Boss)
                                    {
                                    // If a boss vote pool is filled, initiates a boss vote.
                                    if (activeSends[i].isElite)
                                        {
                                        // If its an elite boss vote, shows elite verisons of bosses in the UI
                                        // and sends a elite boss on completion
                                            eliteBossVote = true;

                                        }
                                        else
                                        {
                                            eliteBossVote = false;
                                        }
                                        for (int o = 0; o < 6; o++)
                                        {
                                            string sprite = bosses[o];
                                            if (eliteBossVote)
                                            {
                                                sprite += "Elite";
                                            }

                                            bossSlotInfos[o].image.Image.SetSprite(Game.instance.model.GetBloon(sprite + "1").icon.GetGUID());

                                        }

                                        counters["BossVote"] = 2000;
                                        slotInfos[i].bg.Image.SetSprite(ModContent.GetSprite<UIFunctions>("bgBarGold"));

                                    }
                                    pointPools[i] -= activeSends[i].cost;
                                }
                                else
                                {
                                    if (pointPools[i] >= activeSends[i].cost * 0.8f)
                                    {
                                    // If a point pool is over 80% full, change text color to red
                                        slotInfos[i].text.Text.color = Color.red;
                                    }
                                    else if (pointPools[i] >= activeSends[i].cost * 0.5f)
                                    {
                                    // If a point pool is over 50% full, change text color to yellow
                                    slotInfos[i].text.Text.color = Color.yellow;
                                    }
                                }
                            }

                        // If buffs are active, shows a yellow bar behind the send as a form of timer
                        if (counters["Speed"] > 0)
                            {


                                counters["Speed"]-=5;



                                for (int i = 0; i < 10; i++)
                                {
                                    if (activeSends[i].modifier == Modifier.Speed)
                                    {
                                        ScaleImage(slotInfos[i].bg.Image, counters["Speed"], 600);
                                        if (counters["Speed"] <= 0)
                                        {
                                            slotInfos[i].bg.Image.SetSprite(ModContent.GetSprite<UIFunctions>("bgBar"));
                                        }

                                    }
                                }

                            }
                            if (counters["Shield"] > 0)
                            {
                                counters["Shield"]-= 5;


                                for (int i = 0; i < 10; i++)
                                {
                                    if (activeSends[i].modifier == Modifier.Shield)
                                    {
                                    // If the shield buff is active, scale the BG sprite accordingly
                                        ScaleImage(slotInfos[i].bg.Image, counters["Shield"], 600);
                                 
                                    if (counters["Shield"] <= 0)
                                        {
                                            slotInfos[i].bg.Image.SetSprite(ModContent.GetSprite<UIFunctions>("bgBar"));
                                        }
                                    }
                                }

                            }

                        if (quene.Count > 0)
                        {

                            // If there are Bloon sends quened, sends and removes the first from quene.
                            InGame.instance.SpawnBloons(quene[0].bloon, quene[0].count, 1);
                            for (int i = 0; i < quene[0].count; i++) {

                                // Add the send to the toOverlay list so it can have the graphical indicator added
                                toOverlay.Add(quene[0].bloon);

                            }
                            quene.RemoveAt(0);
                        }

                      
                       

                            // Reduces valid time by 1 and give all players eco
                            // Once valid time is below or equal to 0, Bloons can no longer be sent until the next round
                            // nor can players generate eco. This is so rounds dont go indefinitely by continous bloon sends
                           
                          
                               


                            
                        }
                        
                        timeDelay -= 120;


                    }
                   

                }
                      
             }
            
            try
            {

                if (counters["ModifyToken"] > 0)
                {
                    counters["ModifyToken"]--;

                    if (counters["ModifyToken"] == 0)
                    {
                        string text = GameObject.Find("TwitchBloonBattles-BotToken Setting Name").GetComponentInChildren<NK_TextMeshProInputField>().text;
                        
                        // If a long enough string is inputted into the text field, attempts to connect the player
                        if (text.Length > 10)
                        {
                            // Initiates a timer to check if the connection succeeded
                            counters["Connect"] = 60;

                  
                            TryLoadTwitch(Username, text);
                            

                        }
                        // Empties the field after the input
                        GameObject.Find("TwitchBloonBattles-BotToken Setting Name").GetComponentInChildren<NK_TextMeshProInputField>().text = "";
                    }
                }
            
                if (counters["Connect"] > 0)
                {
                    counters["Connect"]--;

                    // When the counter reaches 0, sends a message in the console whether or not the connection succeeded
                    if (counters["Connect"] == 0)
                    {
                        bool connected = false;
                        if (client != null)
                        {
                            if (client.IsConnected)
                            {
                                connected = true;
                               
                            }
                            
                        }
                        
                        if (connected)
                        {

                            MelonLogger.Msg("Connected user " + client.TwitchUsername);

                        }
                        else
                        {
                            MelonLogger.Msg("Connection Failed!");


                        }
                    }
                }
            }
            catch
            {

            }
            
        
    }
}