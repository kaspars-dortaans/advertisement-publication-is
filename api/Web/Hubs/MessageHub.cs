using BusinessLogic.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Web.Hubs;

[HasPermission(Permissions.ViewMessages)]
public class MessageHub : Hub<IMessageHub>
{  }
