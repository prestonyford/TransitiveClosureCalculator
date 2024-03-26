using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransitiveClosureCalculator
{
    public interface IGraphUpdateHandler
    {
        void HandleCanvasUpdate(Dictionary<string, HashSet<string>> stringAdjacencyList);
    }
}
