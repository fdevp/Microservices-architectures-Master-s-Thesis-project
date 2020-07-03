using SharedClasses.Models;

namespace SharedClasses.Events.Cards
{
    public class SelectedCardsEvent
    {
        public Card[] Cards { get; set; }
    }
}