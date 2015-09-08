using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm.DataPaths
{

    public interface IDataPathRoot
    {

        DataPath GetPath(IDataPathAddressable obj);

        IDataPathAddressable ResolvePath(DataPath dataPath);

    }

}
