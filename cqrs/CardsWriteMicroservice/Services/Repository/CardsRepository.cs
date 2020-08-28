using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace CardsWriteMicroservice.Repository
{
    public class CardsRepository
    {
        private Dictionary<string, Models.Card> cards = new Dictionary<string, Models.Card>();
        private ConcurrentDictionary<string, Models.Block> blocks = new ConcurrentDictionary<string, Models.Block>();

        public Models.Card GetCard(string id)
        {
            if (cards.ContainsKey(id))
                return cards[id];
            return null;
        }

        public Models.Card[] GetByAccounts(IEnumerable<string> accountIds)
        {
            var accountsSet = accountIds.ToHashSet();
            return cards.Values.Where(c => accountsSet.Contains(c.AccountId)).ToArray();
        }

        public Models.Block[] GetBlocks(string cardId)
        {
            return blocks.Values.Where(b => b.CardId == cardId).ToArray();
        }

        public Models.Block CreateBlock(string cardId, string transactionId, DateTime timestamp)
        {
            var block = new Models.Block
            {
                Id = Guid.NewGuid().ToString(),
                CardId = cardId,
                TransactionId = transactionId,
                Timestamp = timestamp
            };
            blocks.TryAdd(block.Id, block);
            return block;
        }

        public void Setup(IEnumerable<Models.Card> cards, IEnumerable<Models.Block> blocks)
        {
            this.cards = cards.ToDictionary(c => c.Id, c => c);
            this.blocks = new ConcurrentDictionary<string, Models.Block>(blocks.ToDictionary(b => b.Id, b => b));
        }
    }
}