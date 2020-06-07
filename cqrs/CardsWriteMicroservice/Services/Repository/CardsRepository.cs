using System;
using System.Collections.Generic;
using System.Linq;

namespace CardsWriteMicroservice.Repository
{
    public class CardsRepository
    {
        private Dictionary<string, Card> cards = new Dictionary<string, Card>();
        private Dictionary<string, Block> blocks = new Dictionary<string, Block>();

        public Card GetCard(string id)
        {
            if (cards.ContainsKey(id))
                return cards[id];
            return null;
        }

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
            var block = new Block(Guid.NewGuid().ToString(), cardId, transactionId, timestamp.Ticks);
            blocks.Add(block.Id, block);
            return block;
        }

        public void Setup(IEnumerable<Repository.Card> cards, IEnumerable<Repository.Block> blocks)
        {
            this.cards = cards.ToDictionary(c => c.Id, c => c);
            this.blocks = blocks.ToDictionary(b => b.Id, b => b);
        }

        public void TearDown()
        {
            this.cards = new Dictionary<string, Card>();
            this.blocks = new Dictionary<string, Block>();
        }

    }
}