using BusinessLogic.Authorization;
using BusinessLogic.Dto.Message;
using Microsoft.AspNetCore.SignalR;

namespace Web.Hubs;

[HasPermission(Permissions.ViewMessages)]
public class MessageHub : Hub<IMessageHub>
{  }
