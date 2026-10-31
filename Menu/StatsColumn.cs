using Il2Cpp;
using Il2CppTLD.Stats;
using UnityEngine;

namespace Trespasser
{
    internal class StatsColumn
    {
        private const string HeaderIconTemplate = "Button_FilterInterloper";
        private const string HeaderIconName = "Button_FilterTrespasser";
        private const string IconOverlayName = "TrespasserColumnIcon";
        private const string CurrentModeIconName = "Sprite_CurrentExperienceMode";
        private const string ValueLabelName = "Label_AllTimeValueTrespasser";
        private const string FilterButtonPrefix = "Button_Filter";
        private const string NoValueText = "-";

        private static readonly string[] mColumnOrder = { "Pilgrim", "Voyageur", "Stalker", "Trespasser", "Interloper", "Misery" };

        private TrespasserManager mManager;
        private TrespasserOverlay mOverlay;
        private Dictionary<string, float>? mColumnX;
        private Texture2D? mIconTexture;

        public StatsColumn(TrespasserManager manager, TrespasserOverlay overlay)
        {
            mManager = manager;
            mOverlay = overlay;
        }


        public void Apply(DetailedStatsView view)
        {
            Transform header = view?.m_GameObject?.transform.Find("Header");
            if (header == null || !TryMeasureColumns(header)) return;

            ApplyHeader(header);
            ShowCurrentModeIcon(header.Find(CurrentModeIconName)?.GetComponent<UISprite>());
            ApplyRows(view);
        }


        private bool TryMeasureColumns(Transform header)
        {
            if (mColumnX != null) return true;
            Transform first = header.Find(FilterButtonPrefix + mColumnOrder[0]);
            Transform last = header.Find(FilterButtonPrefix + mColumnOrder[mColumnOrder.Length - 1]);
            if (first == null || last == null) return false;

            float start = first.localPosition.x;
            float step = (last.localPosition.x - start) / (mColumnOrder.Length - 1);
            mColumnX = new();
            for (int i = 0; i < mColumnOrder.Length; i++)
                mColumnX[mColumnOrder[i]] = start + step * i;
            return true;
        }


        private void ApplyHeader(Transform header)
        {
            foreach (string column in mColumnOrder)
                SetX(header.Find(FilterButtonPrefix + column), mColumnX[column]);

            Transform template = header.Find(HeaderIconTemplate);
            if (template == null || header.Find(HeaderIconName) != null) return;

            GameObject icon = UnityEngine.Object.Instantiate(template.gameObject, header, false);
            icon.name = HeaderIconName;
            SetX(icon.transform, mColumnX["Trespasser"]);
            foreach (UIButton button in icon.GetComponentsInChildren<UIButton>(true))
                button.enabled = false;
            AddIconOverlay(icon.GetComponent<UISprite>());
        }


        private void ShowCurrentModeIcon(UISprite sprite)
        {
            if (sprite == null) return;
            bool isTrespasser = mManager.SandboxConfigManager.IsTrespasserMode();
            Transform overlay = sprite.transform.Find(IconOverlayName);
            if (overlay == null && isTrespasser)
                overlay = AddIconOverlay(sprite);
            if (overlay == null) return;
            overlay.gameObject.SetActive(isTrespasser);
            sprite.enabled = !isTrespasser;
        }


        private Transform? AddIconOverlay(UISprite sprite)
        {
            if (sprite == null) return null;
            mIconTexture ??= mOverlay.LoadEmbeddedTexture("trespasser_bw_icon.png");
            if (mIconTexture == null) return null;

            sprite.enabled = false;
            GameObject overlay = new(IconOverlayName);
            overlay.transform.SetParent(sprite.transform, false);
            UITexture texture = overlay.AddComponent<UITexture>();
            texture.mainTexture = mIconTexture;
            texture.width = sprite.width;
            texture.height = sprite.height;
            texture.depth = sprite.depth + 1;
            texture.color = sprite.color;
            return overlay.transform;
        }


        private void ApplyRows(DetailedStatsView view)
        {
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<StatID> statOrder = view.m_StatsOrderToShow;
            Transform list = view.m_ScrollList?.transform;
            if (statOrder == null || list == null) return;

            int rows = Mathf.Min(list.childCount, statOrder.Length);
            for (int i = 0; i < rows; i++)
            {
                Panel_Stats_DetailedEntry entry = list.GetChild(i).GetComponentInChildren<Panel_Stats_DetailedEntry>(true);
                if (entry != null)
                    ApplyRow(view, entry, statOrder[i]);
            }
        }


        private void ApplyRow(DetailedStatsView view, Panel_Stats_DetailedEntry entry, StatID statId)
        {
            SetX(entry.m_Label_AllTimeValuePilgrim?.transform, mColumnX["Pilgrim"]);
            SetX(entry.m_Label_AllTimeValueVoyageur?.transform, mColumnX["Voyageur"]);
            SetX(entry.m_Label_AllTimeValueStalker?.transform, mColumnX["Stalker"]);
            SetX(entry.m_Label_AllTimeValueInterloper?.transform, mColumnX["Interloper"]);
            SetX(entry.m_Label_AllTimeValueMisery?.transform, mColumnX["Misery"]);

            UILabel template = entry.m_Label_AllTimeValueInterloper;
            if (template == null) return;
            Transform existing = template.transform.parent.Find(ValueLabelName);
            UILabel label = existing != null ? existing.GetComponent<UILabel>() : CreateValueLabel(template);
            label.text = FormatValue(view, statId);
        }


        private UILabel CreateValueLabel(UILabel template)
        {
            GameObject clone = UnityEngine.Object.Instantiate(template.gameObject, template.transform.parent, false);
            clone.name = ValueLabelName;
            SetX(clone.transform, mColumnX["Trespasser"]);
            return clone.GetComponent<UILabel>();
        }


        private string FormatValue(DetailedStatsView view, StatID statId)
        {
            StatInfo info = StatsManager.GetStatInfo(statId);
            if (info == null) return NoValueText;
            mManager.Stats.TryGetAllTimeBest(statId, out float value);
            return view.GetFormattedValue(info.m_Format, value);
        }


        private void SetX(Transform transform, float x)
        {
            if (transform == null) return;
            Vector3 position = transform.localPosition;
            transform.localPosition = new Vector3(x, position.y, position.z);
        }
    }
}
