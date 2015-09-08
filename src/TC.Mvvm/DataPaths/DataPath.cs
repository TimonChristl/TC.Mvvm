using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm.DataPaths
{

    public class DataPath
    {

        private DataPathItem[] items;

        public DataPath(IEnumerable<DataPathItem> items)
        {
            this.items = items.ToArray();
        }

        public DataPathItem[] Items
        {
            get { return items; }
        }

    }

}
