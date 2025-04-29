using BusinessLogic.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AdvertisementWebsite.Server.Hubs;

[HasPermission(Permissions.ViewMessages)]
public class MessageHub : Hub<IMessageHub>
{  }
