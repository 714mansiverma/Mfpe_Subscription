using SubscriptionService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SubscriptionService.Repository;
//using SubscriptionService.Models;
using SubscriptionService.Controllers;
namespace SubscriptionService.Provider
{
    public class SubscribeProvider : ISubscribeProvider
    {
        ISubscribeDrugs subs;
        public SubscribeProvider(ISubscribeDrugs subscribeDrugs)
        {
            subs = subscribeDrugs;
        }

         public SubscriptionDetails Subscribe(PrescriptionDetails subscription, string PolicyDetails, int MemberId)
        {
           return subs.PostSubscription(subscription, PolicyDetails, MemberId);
        }

        public SubscriptionDetails UnSubscribe(int Member_Id, int Subscription_Id)
        {
            return subs.PostUnSubscription(Member_Id, Subscription_Id);
        }
    }
}
