using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.Services.Interface
{
    public interface ISubscriptionRegistrationService
    {
        Task<string> AddSubscription();
        Task<string> DeleteSubcription();
    }
}
