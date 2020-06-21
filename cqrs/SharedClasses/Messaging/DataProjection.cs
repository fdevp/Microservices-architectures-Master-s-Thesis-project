
using SharedClasses;

namespace SharedClasses.Messaging
{
    public class DataProjection<TUpsert,TRemove>
    {
        public TUpsert[] Upsert { get; set; }
        public TRemove[] Remove { get; set; }
    }
}