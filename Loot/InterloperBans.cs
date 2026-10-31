using Il2Cpp;
using Il2CppTLD.Gameplay;
using Il2CppTLD.GameplayTags;
using UnityEngine;

namespace Trespasser
{
    internal class InterloperBans
    {
        private HashSet<int>? mInterloperTags;


        public bool IsInterloperBan(Component disabler)
        {
            DisableObjectForXPMode xpDisabler = disabler.TryCast<DisableObjectForXPMode>();
            if (xpDisabler != null)
                return xpDisabler.m_XPModesToDisable != null && xpDisabler.m_XPModesToDisable.Contains(ExperienceModeType.Interloper);

            DisableObjectForGameMode gameModeDisabler = disabler.TryCast<DisableObjectForGameMode>();
            if (gameModeDisabler?.m_DisableIfAnyActive == null) return false;

            GameModeConfig currentMode = ExperienceModeManager.s_CurrentGameMode;
            if (currentMode == null) return false;
            foreach (GameplayTag tag in gameModeDisabler.m_DisableIfAnyActive)
            {
                if (InterloperTags().Contains(tag.m_Tag) && currentMode.HasActiveTag(tag))
                    return true;
            }
            return false;
        }


        private HashSet<int> InterloperTags()
        {
            if (mInterloperTags != null) return mInterloperTags;

            mInterloperTags = new();
            Il2CppSystem.Collections.Generic.List<GameplayTag> tags = ExperienceModeManager.GetGameModeFromName("Interloper")?.m_ActiveTags;
            for (int i = 0; tags != null && i < tags.Count; i++)
                mInterloperTags.Add(tags[i].m_Tag);
            return mInterloperTags;
        }
    }
}
