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
        //Placeholder class, for now. Needs to:
        //-receive data from refinery and assembly
        //-prevent them from stopping production due to "clogging"
        //  *Empty end product inventory when over a certain % of space taken
        //  *Empty and refill raw product inventory according to certain specifications when its over a certain % and
        //      there are other raw materials available to refine/construct. This way there will always be a variety of available items
        //-print production lists into an LCD, if present
        //-control conveyor systems to prevent facilities from pulling items. Their control should only be carried out by this script

        public class facilitiesClass 
        {
            public cargoClass Cargo = new cargoClass();


        }
    }
}
