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
        //-calculate grid battery status like available energy %
        //-calculate average expected battery duration
        //-control battery mode according to situation (Auto when ship is on, recharge when it's off and has solar panels available, etc)
        //-check for movable solar panels and move them to maximize solar energy capture
        //-should not automatically activate hydrogen engines (those are for emergencies only[burns through ice storage like a 8 year old through candy])
        public class energyClass
        {

        }
    }
}
