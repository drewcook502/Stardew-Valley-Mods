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

            Vector2 location = Game1.currentCursorTile;
            Farm farm = Game1.getFarm();
            if (farm.terrainFeatures.TryGetValue(location, out TerrainFeature feature))
            {
               HoeDirt dirt = feature as HoeDirt;
               if (!object.ReferenceEquals(dirt, null) && !object.ReferenceEquals(dirt.crop, null))
               {
                  string message = GetMessage(dirt.crop);
                  DebugLogMessage(message);
                  //nothing is an empty window
                  //1 is achivement
                  //2 gives a giant ! in the box --new quest
                  //3 is giant red X -- error
                  //4 is a giant green +  --stamina
                  //5 is a giant red +  --health

                  if (!Game1.doesHUDMessageExist(message))
                  {
                     HUDMessage hudMessage = new HUDMessage(message);

                     hudMessage.noIcon = true;//good idea if I can't figure out how to get the icon of the crop
                     
                     Game1.addHUDMessage(hudMessage);
                  }

               }
            }

         }
         catch (Exception exception)
         {
            this.Monitor.Log(exception.GetType().Name + " caught in ControlEvents_MouseChanged.");
            this.Monitor.Log(exception.StackTrace);
         }
      }

      private string GetMessage(Crop crop)
      {
         if (crop.dead.Value)
         {
            return GetCropName(crop) + " is dead";
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
            numOfDaysRemaining = crop.regrowAfterHarvest.Value - crop.dayOfCurrentPhase.Value;
         }
         else
         {
            //counted the current phase already, need to remove the days it's already grown
            numOfDaysRemaining -= crop.dayOfCurrentPhase.Value;
         }
         DebugMessage(numOfDaysRemaining >= 0, "numOfDaysRemaining is negative. Look into!");

         string name = GetCropName(crop);
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

      private string GetCropName(Crop crop)
      {
         int nameIndex = crop.indexOfHarvest.Value;

         Game1.objectInformation.TryGetValue(nameIndex, out string name);

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