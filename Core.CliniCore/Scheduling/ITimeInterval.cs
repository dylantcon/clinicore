using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Scheduling
{
    public interface ITimeInterval
    {
        DateTime Start { get; }
        DateTime End { get; }
        TimeSpan Duration { get; }
        bool Overlaps(ITimeInterval other);
        bool Contains(ITimeInterval other);
        bool IsValid();
    }
}
