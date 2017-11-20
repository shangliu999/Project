using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.WashingCabinet.Domain
{
    public interface IRFIDScan
    {
        void ScanNew(List<TagModel> tags);

        void NoScanTag();
    }
}
