using Il2CppTLD.Trader;

namespace Trespasser
{
    internal class BannedTrades
    {
        private LootManager mLootManager;
        private Dictionary<IntPtr, List<TradeExchangeData>> mBannedExchanges = new();

        public BannedTrades(LootManager lootManager)
        {
            mLootManager = lootManager;
        }


        public void Forget(TradeExchange exchange)
        {
            if (exchange != null)
                mBannedExchanges.Remove(exchange.Pointer);
        }


        public TradeExchangeData MaybeDrawBanned(TradeExchange exchange, TraderState traderState, TradeExchangeData vanillaDraw)
        {
            if (exchange == null || traderState == null) return vanillaDraw;

            List<TradeExchangeData> banned = GetBanned(exchange).Where(data => IsDrawable(data, traderState)).ToList();
            if (banned.Count == 0) return vanillaDraw;

            int allowedCount = CountDrawable(exchange.m_FilteredExchanges, traderState);
            double drawChance = mLootManager.BannedDrawChance(mLootManager.ChanceFor(LootSource.Trader, string.Empty), banned.Count, allowedCount);
            return mLootManager.Roll(drawChance) ? banned[new System.Random().Next(banned.Count)] : vanillaDraw;
        }


        private List<TradeExchangeData> GetBanned(TradeExchange exchange)
        {
            if (mBannedExchanges.TryGetValue(exchange.Pointer, out List<TradeExchangeData> banned))
                return banned;

            banned = new();
            Il2CppSystem.Collections.Generic.List<TradeExchangeData> possible = exchange.m_PossibleExchanges;
            Il2CppSystem.Collections.Generic.List<TradeExchangeData> filtered = exchange.m_FilteredExchanges;
            for (int i = 0; possible != null && i < possible.Count; i++)
            {
                TradeExchangeData data = possible[i];
                if (data == null) continue;
                if (filtered != null && filtered.Contains(data)) continue;
                if (!mLootManager.WithBansLifted(() => data.IsValidInCurrentGamemode())) continue;
                banned.Add(data);
            }
            mBannedExchanges[exchange.Pointer] = banned;
            return banned;
        }


        private int CountDrawable(Il2CppSystem.Collections.Generic.List<TradeExchangeData> exchanges, TraderState traderState)
        {
            int count = 0;
            for (int i = 0; exchanges != null && i < exchanges.Count; i++)
                if (IsDrawable(exchanges[i], traderState)) count++;
            return count;
        }


        private bool IsDrawable(TradeExchangeData data, TraderState traderState)
            => data != null && !traderState.HasExchangeRewardingItem(data.m_GainedItem);
    }
}
