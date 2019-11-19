using System.Threading.Tasks;

namespace Convex.Core.Net
{
    public interface IEstablishedConnection
    {
        IAddress Address { get; }

        Task WriteAsync(string data);
        Task SendCommandAsync(CommandEventArgs args);
    }
}