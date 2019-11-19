using System.Collections.Generic;
using System.Threading.Tasks;
using Convex.Core.Net;
using Convex.Core.Plugins.Compositions;
using SharpConfig;

namespace Convex.Core
{
    public interface IInitializedClient
    {
        Configuration Configuration { get; }
        Server Server { get; }
        IReadOnlyDictionary<string, CompositionDescription> CompositionDescriptions { get; }

        Task Connect(IAddress address);
    }
}