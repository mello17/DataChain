using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text;

namespace DataChain.Consensus
{
    public class ConsensusBft
    {
        private System.Timers.Timer timer;
        public VoteSet ValidateVotes { get; set; }
        public VoteSet PrecommitVotes { get; set; }
        public VoteSet CommitVotes { get; set; }
        public EventType Type { get; set; }
        public ConsensusStatus Status { get; set; }
        public Vote Votes { get; set; }
        public BlockingCollection<EventType> events;

        private long lastUpdate;
         

        public ConsensusBft()
        {
            timer = new System.Timers.Timer();
            this.Status = ConsensusStatus.STOPPED;
            this.Type = EventType.NEW_HEIGHT;

        }

        public void Start()
        {

            if(Status == ConsensusStatus.STOPPED)
            {
                Status = ConsensusStatus.RUNNING;
                timer.Start();

                EnterNewHeight();
                EventLoop();

            }

        }

        public void EnterNewHeight()
        {

        }

        public void UpdateValidators()
        {

        }

        public void EventLoop()
        {
            while (Thread.CurrentThread.ThreadState != ThreadState.Aborted && Status != ConsensusStatus.STOPPED)
            {
                try
                {
                    EventType event1 = events.Take();
                    if (Status != ConsensusStatus.STOPPED)
                    {
                        continue;
                    }
                    if (this.lastUpdate + (long)(2 * 60 * 1000) < DateTime.UtcNow.Millisecond)
                    {
                        UpdateValidators();
                    }

                }
                catch
                {

                }
            }

        }

       



    }
}
