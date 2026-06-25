using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeDose.Domain.Entities
{
    public class PushSubscription
    {
        public int PushSubscriptionId { get; set; }

        public string Endpoint { get; set; }

        public string P256DH { get; set; }

        public string Auth { get; set; }

        public string AccountId { get; set; }

        public Account Account { get; set; }

        public DateOnly CreatedAt { get; set; }
    }
}
