using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.Mvvm
{

    public interface ISelectable
    {

        bool IsSelected { get; set; }

        object Kind { get; }

    }

}
