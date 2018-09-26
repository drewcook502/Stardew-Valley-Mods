using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;

namespace Tutorial
{
   /// <summary>The mod entry point.</summary>
   public class ModEntry : Mod
   {
      private Microsoft.Xna.Framework.Input.Keys triggerKey = Microsoft.Xna.Framework.Input.Keys.Z;

      /*********
      ** Public methods
      *********/
      /// <summary>The mod entry point, called after the mod is first loaded.</summary>
      /// <param name="helper">Provides simplified APIs for writing mods.</param>
      public override void Entry(IModHelper helper)
      {
         ControlEvents.KeyReleased += ControlEvents_KeyReleased;
      }

      private void ControlEvents_KeyReleased(object sender, EventArgs e)
      {
         try
         {
            if (!Context.IsWorldReady)
            {
               return;
            }

            EventArgsKeyPressed keyPress = (EventArgsKeyPressed)e;

            if (keyPress.KeyPressed != triggerKey)
            {
               return;
            }

            Vector2 tile = Game1.currentCursorTile;
            GameLocation curLocation = Game1.currentLocation;
            
            if (curLocation.terrainFeatures.TryGetValue(tile, out TerrainFeature feature))
            {
               string message = "";
               if (feature is HoeDirt dirt)
               {
                  if (!object.ReferenceEquals(dirt.crop, null))
                  {
                     message = GetCropMessage(dirt.crop);
                  }
               }
               else if (feature is FruitTree fruitTree)
               {
                  message = GetFruitTreeMessage(fruitTree);
               }
               else if (feature is Tree tree)
               {
                  message = GetTreeMessage(tree);
               }


               DebugLogMessage(message);
               //nothing is an empty window
               //1 is achivement
               //2 gives a giant ! in the box --new quest
               //3 is giant red X -- error
               //4 is a giant green +  --stamina
               //5 is a giant red +  --health

               if (!string.IsNullOrWhiteSpace(message) && !Game1.doesHUDMessageExist(message))
               {
                  HUDMessage hudMessage = new HUDMessage(message);

                  hudMessage.noIcon = true;//good idea if I can't figure out how to get the icon of the crop

                  Game1.addHUDMessage(hudMessage);
               }
            }

         }
         catch (Exception exception)
         {
            this.Monitor.Log(exception.GetType().Name + " caught in ControlEvents_MouseChanged.");
            this.Monitor.Log(exception.StackTrace);
         }
      }

      private string GetCropMessage(Crop crop)
      {
         string name = GetNameFromIndex(crop.indexOfHarvest.Value);

         if (crop.dead.Value)
         {
            return name + " is dead";
         }

         //get all remaining phases
         int numOfDaysRemaining = 0;
         int phaseIndex = crop.currentPhase.Value;
         for (int index = phaseIndex; index < crop.phaseDays.Count; index++)
         {
            int daysInPhase = crop.phaseDays[index];
            if (daysInPhase < 99999)
            {
               numOfDaysRemaining += daysInPhase;
            }
         }

         if ((phaseIndex == crop.phaseDays.Count - 1) && crop.regrowAfterHarvest.Value > 0)
         {
            //in a regrow forever till dead situation
            DebugMessage(numOfDaysRemaining == 0, "In a regrow, but have num of days remaining. Look into!");
            numOfDaysRemaining = crop.regrowAfterHarvest.Value - crop.dayOfCurrentPhase.Value; //so, before harvest crop.dayOfCurrentPhase is 0, after harvest it is 4 (or something) similar...
         }
         else
         {
            //counted the current phase already, need to remove the days it's already grown
            numOfDaysRemaining -= crop.dayOfCurrentPhase.Value;
         }
         DebugMessage(numOfDaysRemaining >= 0, "numOfDaysRemaining is negative. Look into!");

         string message;
         if (numOfDaysRemaining == 0)
         {
            message = name + " is ready for harvest";
         }
         else
         {
            message = name + " has " + numOfDaysRemaining + " days till harvest";
         }

         return message;

      }

      private string GetFruitTreeMessage(FruitTree tree)
      {
         string message = "";

         int fruitIndex = tree.indexOfFruit.Value;

         message = GetNameFromIndex(fruitIndex) + " tree";

         int growthStage = tree.growthStage.Value;

         int daysUntilMature = tree.daysUntilMature.Value;
         if (daysUntilMature > 0)
         {
            //not fully grown, count days till fully grown
            message += " has " + daysUntilMature + " until mature";
         }

         string season = tree.fruitSeason.Value;

         if (!string.IsNullOrWhiteSpace(season))
         {
            message += ". Bears fruit in " + season;
         }

         return message;
      }

      private string GetTreeMessage(Tree tree)
      {
         string message;

         int treeType = tree.treeType.Value;
         switch (treeType)
         {
            case Tree.bushyTree:
            case Tree.winterTree1:
            {
               message = "Oak tree";
               break;
            }
            case Tree.leafyTree:
            case Tree.winterTree2:
            {
               message = "Maple tree";
               break;
            }
            case Tree.pineTree:
            {
               message = "Pine tree";
               break;
            }
            case Tree.palmTree:
            {
               message = "Palm tree";
               break;
            }
            case Tree.mushroomTree:
            {
               message = "Mushroom tree";
               break;
            }
            default:
            {
               DebugMessage(false, "Unexpected tree type. Do not know the name.");
               message = "";
               break;
            }
         }

         return message;
      }

      private string GetNameFromIndex(int index)
      {
         Game1.objectInformation.TryGetValue(index, out string name);

         //of course I have to parse it...
         return name.Split('/')[4];
      }

      #region Debug
      [Conditional("DEBUG")]
      private void DebugMessage(bool assertion , string message)
      {
         if (!assertion)
         {
            Debug.Fail(message);
            DebugLogMessage(message);
         }
      }

      [Conditional("DEBUG")]
      private void DebugLogMessage(string message)
      {
         this.Monitor.Log(message);
      }

      #endregion Debug
   }
}