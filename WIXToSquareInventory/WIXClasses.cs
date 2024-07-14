using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WIXToSquareInventory
{
    internal record WIXProductRecord(string handleId, string fieldType, string name, string description, string productImageUrl, string collection, string price, string quantity, string[] options);
}
