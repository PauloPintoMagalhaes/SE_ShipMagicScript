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
        public class cargoClass : MyGridProgram
        {
            //contains every information about the cargos, what items they have, in what cargo id, what quantities 
            //and in what position of the cargo array, for easier transfer, if needed.
            public class individualCargo : MyGridProgram
            {
                //Declare initial variables. Lists are proving to be more versatile than common arrays. Note: this is the reasonable way out. See edit below
                public List<MyTuple<string, string, float>> MaterialQuantity = new List<MyTuple<string, string, float>>();
                public List<long> ID = new List<long>();
                public List<MyFixedPoint> FreeVolume = new List<MyFixedPoint>();
                //SubType, Type, quantity, totalVolume, position, volume of 1 item
                public List<List<MyTuple<string, string, VRage.MyFixedPoint, int>>> MaterialList = new List<List<MyTuple<string, string, VRage.MyFixedPoint, int>>>();
                //I'm sorely lacking in encapsulation quality, but... one step at a time. Still... not bad for 2 day's work

                public individualCargo()
                {
                    ID.Clear();
                    FreeVolume.Clear();
                    MaterialList.Clear();
                    MaterialQuantity.Clear();
                }

                //adds items to the cargo list in a controlled fashion. Serves for transfering
                public void addItem(long vfID, MyFixedPoint vfFreespace, string vfType, string vfSubType, VRage.MyFixedPoint vfQuant, int vfPos)
                {
                    // MaterialList index has to correspond with ID index and FreeVolume index. This way, ID[10] corresponds to the FreeSpace[10] and the MaterialList[10]
                    //To achieve this, items can only be added in this method.
                    MyTuple<string, string, VRage.MyFixedPoint, int> vlNewMats = new MyTuple<string, string, VRage.MyFixedPoint, int>( vfType, vfSubType, vfQuant, vfPos);
                    //check if id in question exists.
                    if (!ID.Contains(vfID))
                    {
                        //it does not, so create. 
                        ID.Add(vfID);
                        FreeVolume.Add(vfFreespace);
                        List<MyTuple<string, string, VRage.MyFixedPoint, int>> listTMP = new List<MyTuple<string, string, VRage.MyFixedPoint, int>>();
                        listTMP.Add(vlNewMats);
                        MaterialList.Add(listTMP);
                    }
                    else
                    {
                        //it does, so increment materials to id position. No need to add or change Id or freespace, since they're the same until a transfer
                        int vlIndex = ID.IndexOf(vfID);
                        //checks if the current SubType already exists in this ID
                        List<MyTuple<string, string, VRage.MyFixedPoint, int>> vlInnerList = MaterialList[vlIndex]; //PM????
                        //it does not, so create
                        vlInnerList.Add(vlNewMats);
                        //Then replace the old list with the new 
                        MaterialList[vlIndex] = vlInnerList;
                        //Note that this system does not increment the quantity of the same material. It lists it so it is easier to access it for transfer
                        //An increment is made, but it is made on the same method that calls this one.
                    }
                    addQuantities(vfSubType, vfType, (float)vfQuant);
                }

                //adds items to the quantity list in a controlled fashion. Serves for printing info
                public void addQuantities(string vfSubType, string vfType, float vfQuantity)
                {
                    bool vlFoundItem = false;
                    int vlI = 0;
                    if (MaterialQuantity.Count > 0) // Checks if it does have that type, otherwise no point in continuing
                    {
                        foreach (MyTuple<string, string, float> line in MaterialQuantity)
                        {
                            if (line.Item1 == vfType && line.Item2 == vfSubType)
                            {
                                float vlTMP = line.Item3 + vfQuantity;
                                MaterialQuantity[vlI] = MyTuple.Create(vfType, vfSubType, vlTMP);
                                if (!vlFoundItem) { vlFoundItem = true; }
                            }
                            vlI++;
                        }
                    }
                }
            }

            //Declare initial variables. Note that refineries and assemblies don't need this flag because they'll be managed in another way
            private bool impCargo, impReactor, impGenerator, impDrill, impWelder, impGrinder, impConnector = false;
            public MyFixedPoint CurrentVolume, TotalVolume = 0;
            public float CargoRacio = 0;
            public int Count = 0;
            public individualCargo cargoLst = new individualCargo();

            public List<long> TEST = new List<long>();

            //Don't actually need a constructor in this case, but use it to guarantee the values are reinitialized to avoid data contamination
            public cargoClass()
            {
                impCargo = false; impReactor = false; impGenerator = false; impDrill = false; impWelder = false; impGrinder = false; impConnector = false;
                CurrentVolume = 0; TotalVolume = 0; CargoRacio = 0; Count = 0;
            }

            public void Clear()
            {
                impCargo = false; impReactor = false; impGenerator = false; impDrill = false; impWelder = false; impGrinder = false; impConnector = false;
                CurrentVolume = 0; TotalVolume = 0; CargoRacio = 0; Count = 0;
            }

            private float calcPercentage(float value, float total)
            {
                return (total == 0) ? 0 : value / total * 100;
            }

            private bool foundItemInInventory(MyInventoryItem vfInventory, List<string> vfType, string vfSubType = "All")
            {
                bool vlTMP = false;
                //I could do this with less conditions but this way it's easier to understand
                if ((vfSubType != "All" && vfInventory.Type.SubtypeId.ToString() == vfSubType)
                    || (vfType[0] != "All" && vfSubType == "All" && vfType.Any())
                    || (vfType[0] == "All" && vfSubType == "All"))
                {
                    //if the item in this slot corresponds to a specific subtype, you can pass it
                    //if the item in this slot corresponds to a specific type, you can pass it regardless of subtype
                    //all items found can be passed
                    vlTMP = true;
                }
                return vlTMP;
            }

            //checks if this cargo type has already been searched through
            private bool isRepeatedSearch(Type vfBlock)
            {
                bool vlTMP = false;
                if (vfBlock == typeof(IMyCargoContainer) && impCargo) { vlTMP = true; }
                else if (vfBlock == typeof(IMyShipConnector) && impConnector) { vlTMP = true; }
                else if (vfBlock == typeof(IMyShipDrill) && impDrill) { vlTMP = true; }
                else if (vfBlock == typeof(IMyShipWelder) && impWelder) { vlTMP = true; }
                else if (vfBlock == typeof(IMyShipGrinder) && impGrinder) { vlTMP = true; }
                else if (vfBlock == typeof(IMyReactor) && impReactor) { vlTMP = true; }
                else if (vfBlock == typeof(IMyGasGenerator) && impGenerator) { vlTMP = true; }
                return vlTMP;
            }

            //checks what type was used and marks it as checked so the search for this cargo type is not repeated
            private void markAsRepeated(Type vfBlock)
            {
                if (vfBlock == typeof(IMyCargoContainer)) { impCargo = true; }
                else if (vfBlock == typeof(IMyShipConnector)) { impConnector = true; }
                else if (vfBlock == typeof(IMyShipDrill)) { impDrill = true; }
                else if (vfBlock == typeof(IMyShipWelder)) { impWelder = true; }
                else if (vfBlock == typeof(IMyShipGrinder)) { impGrinder = true; }
                else if (vfBlock == typeof(IMyReactor)) { impReactor = true; }
                else if (vfBlock == typeof(IMyGasGenerator)) { impGenerator = true; }
            }

            //This function is not very well made, but it worked on a previous version of the code and I'm too tired to
            //"pimp" it up now. 
            private string[] cToSE_Key(string thisItem)
            {
                string[] components = new string[]{"Construction", "MetalGrid", "InteriorPlate", "SteelPlate", "Girder", "SmallTube", "LargeTube", "Motor", "Display", "Glass",
                "Superconductor", "Computer", "Reactor", "Thrust", "GravityGen", "Medical", "RadioComm", "Detector", "Explosives", "SolarCell", "PowerCell"};
                string[] ores = new string[] { "Ice", "Stone", "Gold Ore", "Iron Ore", "Silver Ore", "Cobalt Ore", "Nickel Ore", "Uranium Ore", "Silicon Ore", "Platinum Ore", "Magnesium Ore" };
                string[] ingots = new string[] { "Gravel", "Gold Ingot", "Silver Ingot", "Nickel Ingot", "Iron Ingot", "Silicon Ingot", "Platinum Ingot", "Magnesium Ingot" };
                string[] tools = new string[] { "Drill", "Drill2", "Drill3", "Drill4", "WelderI", "WelderI2", "WelderI3", "WelderI4", "Grinder", "Grinder2", "Grinder3", "Grinder4" };
                string[] ammo = new string[] { "Missile", "Ammo045mm", "Ammo184mm" };
                string SE_Type = "";
                string SE_SubType = "";
                //Determines Type
                if (components.Any(thisItem.Contains)) { SE_Type = "Component"; }
                else if (ores.Any(thisItem.Contains)) { SE_Type = "Ore"; }
                else if (ingots.Any(thisItem.Contains)) { SE_Type = "Ing"; }
                else if (tools.Any(thisItem.Contains)) { SE_Type = "PhysicalGunObject"; }
                else if (ammo.Any(thisItem.Contains)) { SE_Type = "AmmoMagazine"; }
                else if (thisItem == "OxygenBottle") { SE_Type = "OxygenContainerObject"; }
                else if (thisItem == "HydrogenBottle") { SE_Type = "GasContainerObject"; }
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

                return new string[] { SE_SubType, SE_Type };
            }

            //converts a game subtype to a more understandable, printable name
            private string cFromSE_Key(string SE_SubType, string Type)
            {
                string nonSE_Name = "";
                //Don't know who had the bright idea to name different items with the same subtype. Makes things unnecessarily hard
                if (Type == "Ore" || Type == "Ingot")
                {
                    if (SE_SubType == "Ice" || SE_SubType == "Stone" || SE_SubType == "Gravel")
                    {
                        nonSE_Name = SE_SubType;
                    }
                    else
                    {
                        string vlNewType = (Type == "Ore") ? "Ore" : "Ing";
                        nonSE_Name = SE_SubType + " " + Type;
                    }
                }
                else
                {
                    if (SE_SubType == "BulletproofGlass") { nonSE_Name = "Glass"; }
                    else if (SE_SubType == "GravityGenerator") { nonSE_Name = "GravityGen"; }
                    else if (SE_SubType == "RadioCommunication") { nonSE_Name = "RadioComm"; }
                    else if (SE_SubType == "HandDrillItem") { nonSE_Name = "Drill"; }
                    else if (SE_SubType == "HandDrill2Item") { nonSE_Name = "Drill2"; }
                    else if (SE_SubType == "HandDrill3Item") { nonSE_Name = "Drill3"; }
                    else if (SE_SubType == "HandDrill4Item") { nonSE_Name = "Drill4"; }
                    else if (SE_SubType == "WelderItem") { nonSE_Name = "Welder"; }
                    else if (SE_SubType == "Welder2Item") { nonSE_Name = "Welder2"; }
                    else if (SE_SubType == "Welder3Item") { nonSE_Name = "Welder3"; }
                    else if (SE_SubType == "Welder4Item") { nonSE_Name = "Welder4"; }
                    else if (SE_SubType == "AngleGrinderItem") { nonSE_Name = "Grinder"; }
                    else if (SE_SubType == "AngleGrinder2Item") { nonSE_Name = "Grinder2"; }
                    else if (SE_SubType == "AngleGrinder3Item") { nonSE_Name = "Grinder3"; }
                    else if (SE_SubType == "AngleGrinder4Item") { nonSE_Name = "Grinder4"; }
                    else if (SE_SubType == "OxygenContainerObject") { nonSE_Name = "OxygenBottle"; }
                    else if (SE_SubType == "GasContainerObject") { nonSE_Name = "HydrogenBottle"; }
                    else if (SE_SubType == "Missile200mm") { nonSE_Name = "Missile200mm"; }
                    else if (SE_SubType == "NATO_5p56x45mm") { nonSE_Name = "NATO_5p56x45mm"; }
                    else if (SE_SubType == "NATO_25x184mm") { nonSE_Name = "NATO_25x184mm"; }
                    else { nonSE_Name = SE_SubType; }
                }
                return nonSE_Name;
            }

            //Cycles through the inventory to catalog the requested items
            private void searchInventory(IMyTerminalBlock vfCargo, List<string> vfType = "All", string vfSubType = "All")
            {
                if ((vfCargo.IsFunctional == true))
                {
                    IMyInventory vlInventory = vfCargo.GetInventory(0);
                    TotalVolume += vlInventory.MaxVolume;
                    CurrentVolume += vlInventory.CurrentVolume;
                    MyFixedPoint vlFreeSpace = vlInventory.MaxVolume - vlInventory.CurrentVolume;
                    var vlItemLst = fillListFromInventory(vlInventory);
                    //each vlI corresponds to a slot in the inventory. items may be divided into different stacks 
                    //despite being the same component type and subtype
                    for (int vlI = 0; vlI < vlItemLst.Count; vlI++)
                    {
                        //check if the item in question is within the search parametres
                        if (foundItemInInventory(vlItemLst[vlI], vfType, vfSubType))
                        {
                            cargoLst.addItem(vfCargo.GetId(), vlFreeSpace, vlItemLst[vlI].Type.TypeId.ToString(), vlItemLst[vlI].Type.SubtypeId.ToString(), vlItemLst[vlI].Amount, vlI);
                        }
                    }
                }
            }

            //I could probably do all in one single function without dificulty, but it would probably be more complicated if someone wants to pick this up
            //... namely me, after months or years without touching this
            private void listAllMaterialsByType(IMyInventory vfCargo, string vfType = "All")
            {
                //Types are: Ores, Ingots, Components, Tools, Ammo
                //listAllMaterials(vfCargo, "All", vfType);
            }

            private void listAllMaterialsBySubType(IMyInventory vfCargo, string vfSubType = "All")
            {
                string[] types = new string[] { "All", "All" };
                if (vfSubType != "All") { types = cToSE_Key(vfSubType); }
                //Subtypes are all individual items under each of the type categories Ores, Ingots, Components, Tools, Ammo
                //listAllMaterials(vfCargo, types[0], types[1]);
            }

            //auxiliary function to skil a boring step from the process of declaring the inventory lists
            private List<MyInventoryItem> fillListFromInventory(IMyInventory vfCargo)
            {
                List<MyInventoryItem> itemInv = new List<MyInventoryItem>();
                vfCargo.GetItems(itemInv);
                return itemInv;
            }

            //Turns a bool group into a list of item types. I could have done this directly, but I judged it would be easier for a non programmer to use "getCargoCount" with booleans
            private List<string> buildTypeArray(bool vfComp = false, bool vfOres = false, bool vfIngots = false, bool vfTools = false, bool vfAmmo = false)
            {
                List<string> vlTMP = new List<string>();
                if (vfComp) { vlTMP.Add("Component"); }
                if (vfOres) { vlTMP.Add("Ore"); }
                if (vfIngots) { vlTMP.Add("Ingot"); }
                if (vfTools) { vlTMP.Add("Tool"); }
                if (vfAmmo) { vlTMP.Add("Ammo"); }
                if (!vfComp && !vfOres && !vfIngots && !vfTools && !vfAmmo) { vlTMP.Add("All"); }
                return vlTMP;
            }

            //Makes the class search the designated cargo type in search of the requested type.
            //DOES NOT HANDLE REFINERIES OR ASSEMBLIES!! That's in another place
            public void getCargoCount(List<IMyTerminalBlock> vfBlockLst, bool vfComp = false, bool vfOres = false, bool vfIngots = false, bool vfTools = false, bool vfAmmo = false) //Every basic cargo container. Small, Medium or Large
            {
                List<string> vlTypeLst = buildTypeArray(vfComp, vfOres, vfIngots, vfTools, vfAmmo);
                //No point trying to do anything if list is empty, found an error or already searched this cargo type.
                if (null != vfBlockLst && vfBlockLst.Count != 0 && !impCargo)
                {
                    if (!isRepeatedSearch(vfBlockLst[0].GetType()))
                    {
                        markAsRepeated(vfBlockLst[0].GetType());
                        for (int vlI = 0; vlI < vfBlockLst.Count; vlI++)
                        {
                            //searches each individual inventory block for the required items
                            searchInventory(vfBlockLst[vlI], vlTypeLst);

                            /*Here we debate with the same ancient question: elegance or effectiveness? use this opportunity to gather 
                            info about blocks that besides cargo have other important data or keep things clean and simple at the cost of
                            multiple cycles through the same block? When both can me used, use both, when only one is available, I always choose 
                            effectiveness unless otherwise instructed*/

                        }
                        CargoRacio = calcPercentage((float)CurrentVolume, (float)TotalVolume);
                    }
                }
            }
        }
    }
}