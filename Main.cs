using MelonLoader;
using UnityEngine;
using System.Reflection;
using Il2Cpp;


namespace Trespasser
{
    internal class Main : MelonMod
	{
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg(Initialize() ? "Initialized Successfully!" : "Initialization Errors!");
        }


        public override void OnDeinitializeMelon()
        {
            LoggerInstance.Msg(Shutdown() ? "Shutdown Successfully!" : "Shutdown Errors!");
        }

        protected bool Initialize()
        {
            Settings settings = new Settings();
            return true;
        }


        protected bool Shutdown()
        {
            return true;
        }
    }
}