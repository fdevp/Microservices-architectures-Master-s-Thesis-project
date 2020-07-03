using SharedClasses.Models;
using SharedClasses.Models.Cards;

namespace SharedClasses.Events.Cards
{
    public class SetupCardsEvent
    {
        public Card[] Cards { get; set; }
        public Block[] Blocks { get; set; }
    }
}