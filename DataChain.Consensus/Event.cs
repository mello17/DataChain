
namespace DataChain.Consensus
{
   public class EventConsensus
    {
        private EventType type;
        public EventType Type
        {
            set
            {
                type = value;
            }
            get
            {
                return type;
            }
        }

        private object data;
        public object Data
        {
            set
            {
                data = value;
            }
            get
            {
                return data;
            }
        }
        
        public EventConsensus(EventType _type)
        {
            this.type = _type;
        }

        public EventConsensus (EventType _type, object _data)
        {
            this.type = _type;
            this.data = _data;
        }


    }
}
