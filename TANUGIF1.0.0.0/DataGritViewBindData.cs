using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TANUGIF
{
    internal class DataGritViewBindData
    {
        public DataGritViewBindData(string col_0, string col_1, string col_2, string col_3)
        {
            this.col_0 = col_0;
            this.col_1 = col_1;
            this.col_2 = col_2;
            this.col_3 = col_3;
        }

        public string col_0 {
            get;
            set;
        }
        public string col_1 {
            get;
            set;
        }
        public string col_2 {
            get;
            set;
        }
        public string col_3 {
            get;
            set;
        }
    }
}
