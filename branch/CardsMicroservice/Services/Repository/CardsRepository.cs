using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace CardsMicroservice.Repository
{
    public class CardsRepository
    {
        private Dictionary<string, Card> cards = new Dictionary<string, Card>();
        private ConcurrentDictionary<string, Block> blocks = new ConcurrentDictionary<string, Block>();

        public Card GetCard(string id)
        {
            if (cards.ContainsKey(id))
                return cards[id];
            return null;
        }

        public string[] GetIds() => cards.Select(c => c.Value.Id).ToArray();

        public Card[] GetByAccounts(IEnumerable<string> accountIds)
        {
            var accountsSet = accountIds.ToHashSet();
            return cards.Values.Where(c => accountsSet.Contains(c.AccountId)).ToArray();
        }

        public Block[] GetBlocks(string cardId)
        {
            return blocks.Values.Where(b => b.CardId == cardId).ToArray();
        }

        public Block CreateBlock(string cardId, string transactionId, DateTime timestamp)
        {
            var block = new Block { Id = Guid.NewGuid().ToString(), CardId = cardId, TransactionId = transactionId, Timestamp = timestamp.Ticks };
            blocks.TryAdd(block.Id, block);
            return block;
        }

        public void Setup(IEnumerable<Repository.Card> cards, IEnumerable<Repository.Block> blocks)
        {
            this.cards = cards.ToDictionary(c => c.Id, c => c);
            this.blocks = new ConcurrentDictionary<string, Block>(blocks.ToDictionary(b => b.Id, b => b));
        }
    }
}