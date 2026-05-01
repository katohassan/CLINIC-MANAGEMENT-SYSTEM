using Microsoft.AspNetCore.SignalR;

namespace ClinicAppointmentSystem.Hubs
{
    public class AppointmentHub : Hub
    {
        public async Task SendAppointmentUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveAppointmentUpdate", message);
        }
    }
}
