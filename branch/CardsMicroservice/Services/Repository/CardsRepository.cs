using System;
using System.Collections.Generic;
using System.Linq;

namespace CardsMicroservice.Repository
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

    }
}