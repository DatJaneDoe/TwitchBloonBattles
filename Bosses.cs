using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Display;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.Display;
using Il2CppAssets.Scripts.Unity.Scenes;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Client.Events;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using UnityEngine;
using System.Diagnostics;
using Il2CppNinjaKiwi.GUTS.Extensions;
using BTD_Mod_Helper.Api.Enums;
using Harmony;
using HarmonyPatch = HarmonyLib.HarmonyPatch;
using HarmonyPostfix = HarmonyLib.HarmonyPostfix;
using TwitchLib.Api.Core.Models.Undocumented.Chatters;
using UnityEngine.UIElements;
using Il2CppAssets.Scripts.Models.Bloons.Behaviors;
using Il2CppSystem.Threading;
using BTD_Mod_Helper.Api.Components;
using Il2CppTMPro;
using Il2CppNewtonsoft.Json.Bson;
using BTD_Mod_Helper.Api.ModOptions;
using UnityEngine.UI;
using Il2CppAssets.Scripts.Data.Quests;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Profile;
using HarmonyPrefix = Harmony.HarmonyPrefix;
using Il2CppInterop.Common;
using Il2CppInterop.Runtime;
using MelonLoader.NativeUtils;
using AccessTools = HarmonyLib.AccessTools;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Unity.GameEditor.UI.PopupPanels;
using static Il2CppAssets.Scripts.Utils.ObjectCache;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Input;
using Il2CppAssets.Scripts.Unity.UI_New.LevelUp;
using Il2CppAssets.Scripts.Unity.Bridge;

namespace TwitchBloonBattles;

public class Bosses
{    

    public static Chatter bot;
    public static ProfileModel profile;

    public class TwitchIconLarge : ModDisplay
    {
        public override string BaseDisplay => "bdbeaa256e6c63b45829535831843376";


        public override void ModifyDisplayNode(UnityDisplayNode node)
        {
            node.GetRenderer<SpriteRenderer>().gameObject.transform.localPosition = new Vector3(20, 0, 30);

            Set2DTexture(node, "chat");

        }
    }
    public class TwitchIconMedium : ModDisplay
    {
        public override string BaseDisplay => "bdbeaa256e6c63b45829535831843376";


        public override void ModifyDisplayNode(UnityDisplayNode node)
        {
    
          node.GetRenderer<SpriteRenderer>().gameObject.transform.localPosition = new Vector3(14, 0, 22);
      
            Set2DTexture(node, "chat");
           
        }
    }

    public class TwitchIconSmall : ModDisplay
    {
        public override string BaseDisplay => "bdbeaa256e6c63b45829535831843376";


        public override void ModifyDisplayNode(UnityDisplayNode node)
        {
            node.GetRenderer<SpriteRenderer>().gameObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            node.GetRenderer<SpriteRenderer>().gameObject.transform.localPosition = new Vector3(6,0,10);
          
            Set2DTexture(node, "chat");

        }
    }
   

    

            [HarmonyPatch(typeof(TitleScreen), nameof(TitleScreen.Start))]
    public class TitleScreenInit
    {
        [HarmonyPostfix]
        public static void Postfix()
        {


            // Adjust mod settings
            SubscriberEcoMultiplier.icon = VanillaSprites.MonkeyBoostIcon;
            EcoMultiplier.icon = VanillaSprites.ThriveIcon;
            SubscriberEcoMultiplier.slider = EcoMultiplier.slider = true;
            SubscriberEcoMultiplier.min = EcoMultiplier.min = 0;
            SubscriberEcoMultiplier.max = EcoMultiplier.max = 1000;


            var bots = new ModSettingCategory("Bots");
            SimulateBots.category = NumberOfBots.category = NumberOfSubBots.category = bots;
            bots.icon = VanillaSprites.TechbotIcon;


            var connect = new ModSettingCategory("Connection");
            connect.icon = VanillaSprites.TwitchIcon;
                Username.category = BotToken.category = TokenGeneratorLinkYouCanCopy.category = connect;


            var ecoMod = new ModSettingCategory("Eco Modifiers");
            ecoMod.icon = VanillaSprites.CashDropIcon;
            SubscriberEcoMultiplier.category =EcoMultiplier.category = ScalePointsPerBit.category = PointsPerBit.category = ecoMod;

            var sendModifier = new ModSettingCategory("Send Modifiers");
            sendModifier.icon = VanillaSprites.BloonariusIcon;
            CostMultiplier.category =  AllowedBosses.category = OnlySubsCanVoteBoss.category = sendModifier;



            
            Action< ModHelperInputField > something = (Action<ModHelperInputField>)delegate (ModHelperInputField s)
                {
                    try
                    {

                        GameObject.Find("TwitchBloonBattles-BotToken Setting Name").GetComponentInChildren<NK_TextMeshProInputField>().inputType = Il2CppTMPro.TMP_InputField.InputType.Password;

                    }
                    catch
                    {

                    }


                };

            BotToken.modifyInput = something;

            Action<string> modifyToken = (Action<string>)delegate (string s)
            {
                try
                {

                    counters["ModifyToken"] = 10;
                    
                }
                catch
                {

                }


            };

          
            BotToken.onValueChanged = modifyToken;
       

            for(int i =0; i < 120; i++)
            {
                sends[i] = new System.Collections.Generic.Dictionary<int, BloonSend> ();
            }

            // Sets base costs for Bloon sends
            baseBloonCosts["Lock"] = 10;


            baseBloonCosts["Red"] = 3;
            baseBloonCosts["Blue"] = 5;
            baseBloonCosts["Green"] = 7;
            baseBloonCosts["Yellow"] = 9;
            baseBloonCosts["Pink"] = 11;
            baseBloonCosts["Purple"] = 15;
            baseBloonCosts["Black"] = 15;
            baseBloonCosts["White"] = 15;
            baseBloonCosts["Zebra"] = 20;
            baseBloonCosts["Lead"] = 22;
            baseBloonCosts["Rainbow"] = 25;
            baseBloonCosts["Ceramic"] = 30;
            baseBloonCosts["Moab"] = 100;
            baseBloonCosts["Bfb"] = 160;
            baseBloonCosts["Ddt"] = 180;
            baseBloonCosts["Zomg"] = 300;
            baseBloonCosts["Bad"] = 500;

            baseBloonCosts["Speed"] = 60; 
            baseBloonCosts["Shield"] = 60;


            baseBloonCosts["Boss1"] = 500;
            baseBloonCosts["Boss2"] = 1000;
            baseBloonCosts["Boss3"] = 2000;
            baseBloonCosts["Boss4"] = 4000;
            baseBloonCosts["Boss5"] = 8000;

            baseBloonCosts["BossElite1"] = 1000;
            baseBloonCosts["BossElite2"] = 2500;
            baseBloonCosts["BossElite3"] = 6000;
            baseBloonCosts["BossElite4"] = 14000;
            baseBloonCosts["BossElite5"] = 32000;



            // Sets Bloon sends for specific rounds

            RegisterBloonSendsForRound(["Red", "Blue", "Green", "Lock", "Lock", "Lock", "Lock", "Lock"], 1);

            RegisterBloonSendsForRound(["RedClump", "Blue", "Green", "BlueClump", "YellowRegrow", "Rainbow", "Lock", "Lock"], 5);

            RegisterBloonSendsForRound(["BlueClump", "Green", "Yellow", "GreenClump", "PinkCamo", "Ceramic", "Lock", "Lock"], 10);

             RegisterBloonSendsForRound(["GreenClump", "Yellow", "PinkRegrow", "YellowClump", "PurpleRegrowCamo", "CeramicFortified", "Lock", "Lock"], 20);
           
        
            RegisterBloonSendsForRound(["YellowRegrowClump", "PinkRegrow", "White", "PinkRegrowClump", "Rainbow", "Moab", "Boss1", "Lock"], 30);

          
            RegisterBloonSendsForRound(["PurpleClump", "LeadFortified", "WhiteRegrow", "BlackClump", "Ceramic", "Moab", "Boss2", "BossElite1"], 40);

         
            RegisterBloonSendsForRound(["WhiteClump", "Rainbow", "RainbowCamo", "ZebraClump", "CeramicFortifiedCamo", "Bfb", "Boss3", "BossElite2"], 50);

       




            RegisterBloonSendsForRound(["ZebraCamoClump", "Ceramic", "CeramicFortified", "CeramicRegrowClump", "Moab", "Bfb", "Boss3", "BossElite3"], 60);


           
            RegisterBloonSendsForRound(["RainbowRegrowClump", "CeramicFortifiedCamo", "Moab", "Bfb", "Ddt", "Zomg", "Boss4", "BossElite3"], 70);




            RegisterBloonSendsForRound(["CeramicFortifiedCamoClump", "Moab", "Bfb", "MoabFortifiedClump", "DdtFortified", "ZomgFortified", "Boss4", "BossElite4"], 80);

           

            RegisterBloonSendsForRound(["BfbClump", "Bfb", "Zomg", "DdtClump", "ZomgFortified", "Bad", "Boss5", "BossElite4"], 90);


            RegisterBloonSendsForRound(["DdtClump", "BfbFortified", "ZomgFortified", "DdtFortifiedClump", "Bad", "BadFortifiedClump", "Boss5", "BossElite5"], 100);
            
            new BloonSend("Speed", 5, 1);
            new BloonSend("Shield", 6, 1);


            counters["Speed"] = 0;
         
            counters["Shield"] = 0;
            counters["BossVote"] = 0;

            counters["ModifyToken"] = 0;
             counters["Connect"] = 0;
            counters["FixUI"] = 0;
            counters["InfoUI"] = 0;

           if(client == null)
            {
                // Creates a blank client incase the player wants to do single player.
                // Uses my username to prevent an error until another username is inputted under mod settings
                TryLoadTwitch(Username,"");
            }
        }
    }
    public static void RegisterBloonSendsForRound(string[] sends, int round)
    {
        
        for (int i = 0; i < 8; i++)
        {
            if (sends[i] != "")
            {
                int index = i;
                if (i >= 5)
                {
                    // Skips changing the Bloon Sends that are temporary buffs
                    index += 2;
                }
                new BloonSend(sends[i], index, round);

            }
        }
      
    }
    public static void UpdateSendOptions()
    {
        
        // Updates the bloon send options
        for (int i = 0; i < 10; i++)
        {
            try
            {
                BloonSend retrieved = sends[relativeRound][i];
                // If a boss is to be a bloon send, checks if the player has it enabled. If not, does not update the bloon send
                bool valid = true;
                if (retrieved.modifier == Modifier.Boss)
                {
                    if (AllowedBosses == BossAllowMode.None)
                    {
                        valid = false;
                      
                    }
                    else if (retrieved.isElite && AllowedBosses == BossAllowMode.OnlyNormal)
                    {
                        valid = false;
                    }
                }
                if (valid)
                {

                    BloonSend bloonSend = activeSends[i] = retrieved;




                    SlotInfo slot = slotInfos[i];
                    bool showModifier = false;

                    if (bloonSend.isClump)
                    {
                        showModifier = true;
                        slot.modifier.Text.SetText("10x");

                    }


                    if (bloonSend.modifier == Modifier.None)
                    {
                        slot.image.Image.SetSprite(Game.instance.model.GetBloon(bloonSend.groupToSend.bloon).icon);
                    }
                    else if (bloonSend.modifier == Modifier.Speed)
                    {
                        //  activeSends[i].cost = activeSends[i].baseCost * Math.Pow(1.1f, relativeRound);
                        slot.image.Image.SetSprite(VanillaSprites.DartTimeIcon);
                    }
                    else if (bloonSend.modifier == Modifier.Shield)
                    {
                        //    activeSends[i].cost = activeSends[i].baseCost * Math.Pow(1.1f, relativeRound);
                        slot.image.Image.SetSprite(VanillaSprites.MonkeyShield);
                    }
                    else if (bloonSend.modifier == Modifier.Lock)
                    {
                        //    activeSends[i].cost = activeSends[i].baseCost * Math.Pow(1.1f, relativeRound);
                        slot.image.Image.SetSprite(VanillaSprites.LockIcon);
                    }
                    else if (bloonSend.modifier == Modifier.Boss)
                    {

                      
                        showModifier = true;
                        slot.modifier.Text.SetText(bloonSend.tier + "");
                        slot.modifier.Text.color = Color.yellow;
                        slot.modifier.Text.fontSize = 70;
                        slot.modifier.Text.outlineColor = Color.red;
                        if (bloonSend.isElite)
                        {
                            slot.image.Image.SetSprite(ModContent.GetSprite<UIFunctions>("elite"));
                        }
                        else
                        {
                            slot.image.Image.SetSprite(VanillaSprites.SkullAndCrossbonesEmoteIcon);
                        }
                    }
                    bloonSend.cost = bloonSend.baseCost * (chatters.Count + 1);

                    if (showModifier)
                    {
                        slot.modifier.Show();
                    }
                    else
                    {
                        slot.modifier.Hide();
                    }
                }
            }
            catch
            {

            }

        }

    }

    public static void TryLoadTwitch(string username, string token)
    {



        var clientOptions = new ClientOptions
        {
            ThrottlingPeriod = TimeSpan.FromSeconds(20),
            MessagesAllowedInPeriod = 1000,
         

        };
     
        client = new TwitchClient(new WebSocketClient(clientOptions));
   
        client.Initialize(new ConnectionCredentials(username, token),
            username);
     
        Task.Run(() =>
       
        {
           
            client.Connect();
      
        });
    
        client.OnMessageReceived += (sender, messageRecieved) =>
        {

            

              MessageFunctions(messageRecieved.ChatMessage.Username, messageRecieved.ChatMessage.IsSubscriber, messageRecieved.ChatMessage.Message, messageRecieved.ChatMessage.Bits);
        };
    }

    [HarmonyPatch(typeof(InGame), nameof(InGame.Lose))]
    public class LossPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            // If you lose, gives you the money equal to the continue cost effectively making it free
            profile.monkeyMoney.Value += InGame.instance.GetContinueCost().Value;
            Reset();
            foreach (var item in counters)
            {
                if (item.Key != "BossVote")
                {
                    counters[item.Key] = 0;
                }
            }


        }
    }
    public static void Reset(bool resetPools = false)
    {
        
        validTime = 0;
        
        quene.Clear();
        

        if (resetPools)
        {
            ecoTicks = 0;
            for (int i = 0; i < 10; i++)
            {
                pointPools[i] = 0;
            }
            foreach(var chatter in chatters)
            {
                chatter.Value.points = 0;
                chatter.Value.eco = 100;
            }
            chatters.Clear();
        }
        if (slotInfos.Count >= 5 && !resetPools)
        {
            ScaleImage(slotInfos[5].bg.Image, 0, 600);
            ScaleImage(slotInfos[6].bg.Image, 0, 600);
        }
        counters["FixUI"] = 100;
    }
   
        [HarmonyPatch(typeof(InGame), nameof(InGame.StartMatch))]
    public class TitleScreenInits
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
        
           
            relativeRound = 1;
            counters["InfoUI"] = 1000;
            Reset(true);
           
           
        }
    }
    public static void FixUI()
    {
        // Adjust the cash and heart counters to make room for the bloon send UI
        InGame.instance.GetInGameUI().GetComponentInChildrenByName<LayoutGroup>("LayoutGroup").gameObject.transform.localPosition = new Vector3(2900, -45, 0);

  
        InGame.instance.GetInGameUI().GetComponentInChildrenByName<LayoutGroup>("LayoutGroup").gameObject.GetComponent<Component>("CashGroup").gameObject.transform.localPosition = new Vector3(300, -150, 0);
       

    }
    public static void DistributeBonusXP(double xp)
    {
        float total = 0;
        if (xp > 0)
        {
            profile.xp.Value += xp * 0.001f;


            foreach (TowerToSimulation simtower in InGame.instance.bridge.GetAllTowers().ToList())
            {
                if (!simtower.tower.towerModel.IsHero() && !simtower.tower.towerModel.isSubTower)
                {
                    total += simtower.tower.worth;
                }
            }
            foreach (TowerToSimulation simtower in InGame.instance.bridge.GetAllTowers().ToList())
            {
                if (!simtower.tower.towerModel.IsHero() && !simtower.tower.towerModel.isSubTower)
                {
                    float perc = simtower.tower.worth / total;
                    profile.towerXp[simtower.tower.towerModel.baseId].Value += perc * xp;


                }
            }
            foreach (var pair in profile.towerXp)
            {
                profile.towerXp[pair.Key].Value += xp * 0.05f;
            }
            profile.monkeyMoney.Value += xp * 0.001f;
        }
    }
    
    [HarmonyPatch(typeof(ProfileModel), nameof(ProfileModel.Validate))]
    public class RoundUpdateSendsStarts
    {
        [HarmonyPostfix]
        public static void Postfix(ProfileModel __instance)
        {
            profile = __instance;
           
            profile.HasCompletedTutorial = true;
            
            
          
        }

    }
    
            [HarmonyPatch(typeof(InGame), nameof(InGame.RoundStart))]
    public class RoundUpdateSendsStart
    {
        [HarmonyPostfix]
        public static void Postfix(int roundArrayIndex)
        {
           
          
            if (profile != null && BonusForSends)
            {
                DistributeBonusXP(roundSpent);
            }

          roundSpent = 0;

            if (roundArrayIndex > 2)
            {
                foreach (var chatter in chatters)
                {
                    chatter.Value.eco++;
                    chatter.Value.points += chatter.Value.eco * (1 + (InGame.instance.bridge.GetCurrentRound() / 20f));
                }
                relativeRound++;
                validTime = 20;
                UpdateSendOptions();

                // If starting at a later round than normal, updates the relative round faster to catch up.
                if (relativeRound < roundArrayIndex)
                {
                    relativeRound++;

                    UpdateSendOptions();

                }
            }
        }

    }
  
}