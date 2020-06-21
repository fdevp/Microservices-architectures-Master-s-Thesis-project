using System;
using System.Collections.Generic;
using System.Linq;

namespace CardsReadMicroservice.Repository
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

        public void Setup(IEnumerable<Repository.Card> cards, IEnumerable<Repository.Block> blocks)
        {
            this.cards = cards.ToDictionary(c => c.Id, c => c);
            this.blocks = blocks.ToDictionary(b => b.Id, b => b);
        }

        public void Upsert(CardsUpsert[] updates)
        {
            foreach (var update in updates)
            {
                if (update.Card != null)
                {
                    cards[update.Card.Id] = update.Card;
                }

                if (update.Block != null)
                {
                    blocks[update.Block.Id] = update.Block;
                }
            }
        }

        public void Remove(CardsRemove[] updates)
        {
            foreach (var update in updates)
            {
                if (update.CardId != null)
                {
                    cards.Remove(update.CardId);
                }

                if (update.BlockId != null)
                {
                    blocks.Remove(update.BlockId);
                }
            }

        }
    }
}