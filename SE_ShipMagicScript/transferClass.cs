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
            //I will never understand why Keen Software had to make a variable number for this! What's wrong with float? I can't even do basic math with MyFixedPoint!!!!
            //This list decided which items you want to automatically pull to your ship IF, there is space or connection available.
            private Dictionary<string, MyFixedPoint> EasyList = new Dictionary<string, MyFixedPoint> { 
                //Components
			    {"Construction", 10000}, {"MetalGrid", 2000}, {"InteriorPlate", 10000}, {"SteelPlate", 10000},
                {"Girder", 2000}, {"SmallTube", 10000}, {"LargeTube", 2000}, {"Motor", 3000}, {"Display", 500}, {"Glass", 5000},
                {"Superconductor", 500}, {"Computer", 3000}, {"Reactor", 2000}, {"Thrust", 2000}, {"GravityGen", 1000}, {"Medical", 500},
                {"RadioComm", 300}, {"Detector", 300}, {"Explosives", 300}, {"SolarCell", 3000}, {"PowerCell", 3000}, 
			    //Ores
			    {"Ice", 0}, {"Stone", 0}, {"Gold Ore", 0}, {"Iron Ore", 0}, {"Silver Ore", 0}, {"Cobalt Ore", 0}, {"Nickel Ore", 0},
                {"Silicon Ore", 0}, {"Platinum Ore", 0}, {"Magnesium Ore", 0}, {"Gravel", 0}, {"Uranium Ore", 0},
                //Ingots 
                {"Gold Ingot", 0}, {"Silver Ingot", 0}, {"Nickel Ingot", 0}, {"Uranium Ingot", 0},
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
            };
            private List<MyTuple<string, string, float>> __TransferList = new List<MyTuple<string, string, float>>();
            //No point in encapsulating these.
            public bool SetToTrimZeroes = true;
            public bool SetToTrimNegatives = true;
            
            //Attempt to agilize the access to values. Is agilize a word? Google sensei says it isn't.
            //Basically, I don't need to change this entire list. I just need to update values as I transfer them
            public float this[int viIndex]
            {
                get { return __TransferList[viIndex].Item3; }
                set
                {
                    //removes the value already in the ship from the list and if it's zero or below, removes it from the list
                    if ((value == 0 && SetToTrimZeroes) || ( value < 0 && SetToTrimNegatives))
                    {
                        __TransferList.RemoveAt(viIndex);   //no point cycling through this anymore
                    }
                    else
                    {
                        __TransferList[viIndex] = MyTuple.Create(__TransferList[viIndex].Item1, __TransferList[viIndex].Item2, value);   //updates the value to what we actually need to tranfer
                    }
                }
            }
            //Better to provide the entire list to allow a "foreach" cycle and change the single value it corresponds to. Probably cleaner
            public List<MyTuple<string, string, float>> TransferList { get { return __TransferList; } set { __TransferList = value; } }
            //Initially I made this without a get, but there are ocasions where I want to replace this with an existing MaterialQuantity from cargoClass

            public transferClass()
            {
                cToDict_trsf();
            }

            //Searches the current string and trims if of useless values
            public void trimTransferList()
            {
                //no point in cycling through it if there's no trimming to do
                if (SetToTrimZeroes || SetToTrimNegatives)
                {
                    foreach(var line in __TransferList)
                    {
                        //searches through the list and, according to filtering, removes the offending lines
                        if ((line.Item3 == 0 && SetToTrimZeroes) || (line.Item3 < 0 && SetToTrimNegatives)) { __TransferList.Remove(line); }
                    }
                }
            }
            
            //Takes a list with the current item quantity in a ship and subtracts them from the list of items that you want to transfer
            //this way, you guarantee that you'll only transfer items that are still missing or lacking
            public void updateTransferList(List<MyTuple<string, string, float>> vfExistingList)
            {
                int vlIndexTrsf = 0;
                int vlIndexExist = 0;
                //__TransferList is the one in charge of this process. It matters not if vfExistingList has more items than indicated
                foreach (var trsf in __TransferList)
                {
                    //If there are 
                    vlIndexExist = vfExistingList.FindIndex(a => a.Item1 == trsf.Item1 && a.Item2 == trsf.Item2);
                    if(vlIndexExist >= 0)
                    {
                        //checks configured filtering types and if they are in conditions to be removed, removes them, else, updates the transfer list value
                        if ((__TransferList[vlIndexTrsf].Item3 - vfExistingList[vlIndexExist].Item3 == 0 && SetToTrimZeroes) || (__TransferList[vlIndexTrsf].Item3 - vfExistingList[vlIndexExist].Item3 < 0 && SetToTrimNegatives))
                        {
                            __TransferList.RemoveAt(vlIndexTrsf);
                        }
                        else
                        {
                            __TransferList[vlIndexTrsf] = MyTuple.Create(__TransferList[vlIndexTrsf].Item1, __TransferList[vlIndexTrsf].Item2, __TransferList[vlIndexTrsf].Item3 - vfExistingList[vlIndexExist].Item3);
                        }
                    }
                }
            }


            //Maybe I'm dumb, but I'm sure there's a way to get that public cToSE_Key from Ship_Main. It's giving me an error, so until I figure why, I have to repeat code
            private List<string> cToSE_Key(string thisItem)
            {
                //Some new items may be missing
                string[] components = new string[]{"Construction", "MetalGrid", "InteriorPlate", "SteelPlate", "Girder", "SmallTube", "LargeTube", "Motor", "Display", "Glass",
                "Superconductor", "Computer", "Reactor", "Thrust", "GravityGen", "Medical", "RadioComm", "Detector", "Explosives", "SolarCell", "PowerCell"};
                string[] ores = new string[] { "Ice", "Stone", "Gold Ore", "Iron Ore", "Silver Ore", "Cobalt Ore", "Nickel Ore", "Uranium Ore", "Silicon Ore", "Platinum Ore", "Magnesium Ore" };
                string[] ingots = new string[] { "Gravel", "Gold Ingot", "Silver Ingot", "Nickel Ingot", "Iron Ingot", "Silicon Ingot", "Platinum Ingot", "Magnesium Ingot" };
                string[] tools = new string[] { "Drill", "Drill2", "Drill3", "Drill4", "WelderI", "WelderI2", "WelderI3", "WelderI4", "Grinder", "Grinder2", "Grinder3", "Grinder4" };
                string[] ammo = new string[] { "Missile", "Ammo045mm", "Ammo184mm" };
                string SE_Type = "";
                string SE_SubType = "";
                //Determines Type
                if (components.Any(thisItem.Contains)) { SE_Type = "MyObjectBuilder_Component"; }
                else if (ores.Any(thisItem.Contains)) { SE_Type = "MyObjectBuilder_Ore"; }
                else if (ingots.Any(thisItem.Contains)) { SE_Type = "MyObjectBuilder_Ingot"; }
                else if (tools.Any(thisItem.Contains)) { SE_Type = "MyObjectBuilder_PhysicalGunObject"; }
                else if (ammo.Any(thisItem.Contains)) { SE_Type = "MyObjectBuilder_AmmoMagazine"; }
                else if (thisItem == "OxygenBottle") { SE_Type = "MyObjectBuilder_OxygenContainerObject"; }
                else if (thisItem == "HydrogenBottle") { SE_Type = "MyObjectBuilder_GasContainerObject"; }
                else if (thisItem == "ClankCola") { SE_Type = "MyObjectBuilder_ConsumableItem"; }
                else { SE_Type = ""; }
                //Determines SubType
                if (thisItem == "Glass") { SE_SubType = "BulletproofGlass"; }
                else if (thisItem == "GravityGen") { SE_SubType = "GravityGenerator"; }
                else if (thisItem == "RadioComm") { SE_SubType = "RadioCommunication"; }
                else if (thisItem == "Drill") { SE_SubType = "HandDrillItem"; }
                else if (thisItem == "Drill2") { SE_SubType = "HandDrill2Item"; }
                else if (thisItem == "Drill3") { SE_SubType = "HandDrill3Item"; }
                else if (thisItem == "Drill4") { SE_SubType = "HandDrill4Item"; }
                else if (thisItem == "Welder") { SE_SubType = "WelderItem"; }
                else if (thisItem == "Welder2") { SE_SubType = "Welder2Item"; }
                else if (thisItem == "Welder3") { SE_SubType = "Welder3Item"; }
                else if (thisItem == "Welder4") { SE_SubType = "Welder4Item"; }
                else if (thisItem == "Grinder") { SE_SubType = "AngleGrinderItem"; }
                else if (thisItem == "Grinder2") { SE_SubType = "AngleGrinder2Item"; }
                else if (thisItem == "Grinder3") { SE_SubType = "AngleGrinder3Item"; }
                else if (thisItem == "Grinder4") { SE_SubType = "AngleGrinder4Item"; }
                else if (thisItem == "OxygenBottle") { SE_SubType = "OxygenContainerObject"; }
                else if (thisItem == "HydrogenBottle") { SE_SubType = "GasContainerObject"; }
                else if (thisItem == "Missile") { SE_SubType = "Missile200mm"; }
                else if (thisItem == "Ammo045mm") { SE_SubType = "NATO_5p56x45mm"; }
                else if (thisItem == "Ammo184mm") { SE_SubType = "NATO_25x184mm"; }
                else if (thisItem.Contains(" ")) { SE_SubType = null; }
                else { SE_SubType = thisItem; }

                return new List<string> { SE_Type, SE_SubType };
            }

            //Converts the EasyList dictionary to something more program friendly. The list is only there for the user to fill in. The script takes care of the rest
            private void cToDict_trsf()
            {
                __TransferList.Clear();
                List<string> vlKeys;
                MyTuple<string, string, float> vlNewLine; ;
                //cycles through every item in the user friendly list and transforms it into the METAL HARDCORE MEGA TUPLE LIST that we'll use to easily compare items with
                foreach (var line in EasyList)
                {
                    vlKeys = cToSE_Key(line.Key);   //converts the user friendly name to Type and SubType directly understood by the SE functions
                    vlNewLine = MyTuple.Create(vlKeys[0], vlKeys[1], (float)line.Value);
                    __TransferList.Add(vlNewLine);  //Adds the converted line to the transfer list
                }
            }
        }
    }
}
