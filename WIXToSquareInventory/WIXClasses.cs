using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WIXToSquareInventory
{
    internal record WIXProductRecord(string handleId, string fieldType, string name, string description, string productImageUrl, string collection, string price, string quantity, string[] options);

    internal record SquareProductRecord(string ident, string item_name, string variation_name, string description, string category, string additional_categories, string price, string new_quantity);
}
