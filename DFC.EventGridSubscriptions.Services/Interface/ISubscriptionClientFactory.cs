using Microsoft.Azure.Management.EventGrid;
using System.Threading.Tasks;

namespace DFC.EventGridSubscriptions.Services.Interface
{
    public interface ISubscriptionClientFactory
    {
       Task<EventGridManagementClient> CreateClient();
    }
}
