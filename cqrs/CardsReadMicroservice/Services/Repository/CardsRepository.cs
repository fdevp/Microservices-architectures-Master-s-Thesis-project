using System;
using System.Collections.Generic;
using System.Linq;
using Models;

namespace CardsReadMicroservice.Repository
{
    public class CardsRepository
    {
        private Dictionary<string, Models.Card> cards = new Dictionary<string, Models.Card>();
        private Dictionary<string, Models.Block> blocks = new Dictionary<string, Models.Block>();

        public Models.Card GetCard(string id)
        {
            if (cards.ContainsKey(id))
                return cards[id];
            return null;
        }

        public string[] GetCardsIds() => cards.Select(c => c.Value.Id).ToArray();

        public Models.Card[] GetByAccounts(IEnumerable<string> accountIds)
        {
            var accountsSet = accountIds.ToHashSet();
            return cards.Values.Where(c => accountsSet.Contains(c.AccountId)).ToArray();
        }

        public Models.Block[] GetBlocks(string cardId)
        {
            return blocks.Values.Where(b => b.CardId == cardId).ToArray();
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