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
            TrespasserManager.Instance?.Initialize();
            return TrespasserManager.Instance != null;
        }


        protected bool Shutdown()
        {
            return true;
        }
    }
}