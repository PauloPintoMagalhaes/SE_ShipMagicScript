using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    partial class Ship_Main
    {
        //It's a complete exageration to make a class just for this, but heck, I AM trying to make a program for dummies
        public class transferClass 
        {
            public transferClass()
            {
                //I will never understand why Keen Software had to make a specific number for this!
                 Dictionary<string, MyFixedPoint> __transferMats = new Dictionary<string, MyFixedPoint> { 
                    //Components
			        {"Construction", 10000}, {"MetalGrid", 2000}, {"InteriorPlate", 10000}, {"SteelPlate", 10000},
                    {"Girder", 2000}, {"SmallTube", 10000}, {"LargeTube", 2000}, {"Motor", 3000}, {"Display", 500}, {"Glass", 5000},
                    {"Superconductor", 500}, {"Computer", 3000}, {"Reactor", 2000}, {"Thrust", 2000}, {"GravityGen", 1000}, {"Medical", 500},
                    {"RadioComm", 300}, {"Detector", 300}, {"Explosives", 300}, {"SolarCell", 3000}, {"PowerCell", 3000}, 
			        //Ores
			        {"Ice", 0}, {"Stone", 0}, {"Gold Ore", 0}, {"Iron Ore", 0}, {"Silver Ore", 0}, {"Cobalt Ore", 0}, {"Nickel Ore", 0}, {"Uranium Ore", 0},
                    {"Silicon Ore", 0}, {"Platinum Ore", 0}, {"Magnesium Ore", 0}, {"Gravel", 0}, {"Gold Ingot", 0}, {"Silver Ingot", 0}, {"Nickel Ingot", 0},
                    {"Iron Ingot", 0}, {"Silicon Ingot", 0}, {"Platinum Ingot", 0}, {"Magnesium Ingot", 0}, 
			        //Tools and bottles
			        {"Drill",  0}, {"Drill2", 0}, {"Drill3", 0}, {"Drill4", 0},
                    {"Welder", 0}, {"Welder2", 0}, {"Welder3", 0}, {"Welder4", 0},
                    {"Grinder", 0}, {"Grinder2", 0}, {"Grinder3", 0}, {"Grinder4", 0},
                    {"OxygenBottle", 0}, {"HydrogenBottle", 0}, 
			        //Ammo
			        {"Missile", 0}, {"Ammo045mm", 0}, {"Ammo184mm", 0},
                    //Consumables
                    {"ClangCola", 0}
                    //List is missing Uranium Ingot because it would have been sucked up by a random reactor, anyway.
                    //This list decided which items you want to automatically pull to your ship IF, there is space or connection available.
                };
            }


            private Dictionary<string, MyFixedPoint> __transferMats = new Dictionary<string, MyFixedPoint>();
            //No point in encapsulating these.
            public bool SetToTrimZeroes = true;
            public bool SetToTrimNegatives = true;

            //The class variable itself acts as a dictionary. Encapsulate to act according to settings
            public MyFixedPoint this[string key]
            {
                get { return __transferMats[key]; }
                set
                {
                    if ((value == 0 && SetToTrimZeroes) || (value <0 && SetToTrimNegatives)) { __transferMats.Remove(key); }
                    else { __transferMats[key] = value; }
                }
            }

            public void trimTransferList()
            {
                //no point in cycling through it if there's no trimming to do
                if (SetToTrimZeroes || SetToTrimNegatives)
                {
                    foreach (var line in __transferMats) { if ((line.Value == 0 && SetToTrimZeroes) || (line.Value < 0 && SetToTrimNegatives)) { __transferMats.Remove(line.Key); } }
                }
            }
            
            //Takes a list with the current item quantity in a ship and subtracts them from the list of items that you want to transfer
            //this way, you guarantee that you'll only transfer items that are still missing or lacking
            public void updateTransferList(Dictionary<string, MyFixedPoint> vfList)
            {
                foreach (var line in __transferMats)
                {
                    //takes the transfer list and compares it to the cargo list
                    if (vfList.ContainsKey(line.Key))
                    {
                        //removes the value already in the ship from the list and if it's zero or below, removes it from the list
                        if ((line.Value - vfList[line.Key] == 0 && SetToTrimZeroes) || (line.Value - vfList[line.Key] < 0 && SetToTrimNegatives))
                        {
                            __transferMats.Remove(line.Key);    //no point cycling through this anymore
                        }
                        else
                        {
                            __transferMats[line.Key] = line.Value - vfList[line.Key];   //updates the value to what we actually need to tranfer
                        }
                    }
                }
            }





        }
    }
}
