using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mymooo.workbench.core.SystemManage
{
    public class MenuModel
    {
        public string Path { get; set; }

        public int Sort { get; set; }

        public string Component { get; set; }

        public string Name { get; set; }

        public Meta Meta { get; set; }

        public List<MenuModel> children { set; get; }
    }

    public class Meta
    {
        public string Title { get; set; }

        public string Icon { get; set; }
    }
}
