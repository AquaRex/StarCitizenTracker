using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarCitizenTracker.Interfaces
{
    public interface IIDConverter
    {
        string ConvertLocationID(string id);
    }
}
