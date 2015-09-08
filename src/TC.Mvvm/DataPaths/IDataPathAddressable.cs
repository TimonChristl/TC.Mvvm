using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm.DataPaths
{

    public interface IDataPathAddressable
    {

        IDataPathAddressable Parent { get; }

        DataPathItem GetChildPathItem(IDataPathAddressable child);

        IDataPathAddressable ResolvePathItemToChild(DataPathItem item);

    }

}
