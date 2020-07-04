using SharedClasses.Models;
using SharedClasses.Models.Cards;

namespace SharedClasses.Events.Cards
{
    public class SelectedBlocksEvent
    {
        public Block[] Blocks { get; set; }
    }
}