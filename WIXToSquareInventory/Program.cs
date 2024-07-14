using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace WIXToSquareInventory
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //if (args.Length == 0)
            //{
            //    Console.WriteLine("No file arguments provided.");
            //    return;
            //}

            //string inputFilePath = args[0];
            string inputFilePath = "./catalog_products.csv";

            if (!File.Exists(inputFilePath))
            {
                Console.WriteLine($"File {inputFilePath} does not exist.");
                return;
            }

            string outputFilePath = Path.Combine(Path.GetDirectoryName(inputFilePath), "output.csv");

            WIXProductRecord[] wixProductRecords = GetWIXProductRecords(inputFilePath);

            SquareProductRecord[] squareProductRecords = WixProductToSquare(wixProductRecords);

            WriteOutputFile(outputFilePath, squareProductRecords);
        }

        static void WriteOutputFile(string outputFilePath, SquareProductRecord[] squareProductRecords)
        {
            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                writer.WriteLine("token,sku,item_name,variation_name,description,category,additional_categories,price,new_quantity,type,option_name_1,option_value_1");

                var lastName = "";
                foreach (var squareProductRecord in squareProductRecords)
                {
                    writer.WriteLine($",,{squareProductRecord.item_name},,{(squareProductRecord.item_name == lastName ? "" : squareProductRecord.description)},{squareProductRecord.category},{squareProductRecord.additional_categories},{squareProductRecord.price},{squareProductRecord.new_quantity},Physical,{(squareProductRecord.variation_name.Length > 0 ? "Color/Material" : "")},{squareProductRecord.variation_name}");
                    lastName = squareProductRecord.item_name;
                }
            }
        }

        static SquareProductRecord[] WixProductToSquare(WIXProductRecord[] wixProductRecords)
        {
            List<SquareProductRecord> squareProductRecords = new List<SquareProductRecord>();

            WIXProductRecord lastProduct = null;
            foreach (var wixProductRecord in wixProductRecords)
            {
                SquareProductRecord squareProductRecord = null;
                if (wixProductRecord.fieldType == "Product")
                {
                    lastProduct = wixProductRecord;
                    squareProductRecord = new SquareProductRecord(
                        wixProductRecord.handleId,
                        wixProductRecord.name,
                        wixProductRecord.options.Length > 0 ? FormatOptionNames(wixProductRecord.options) : "",
                        CleanupHTML(wixProductRecord.description),
                        wixProductRecord.collection.Length > 0 ? GetCategory(wixProductRecord.collection)[0] : "",
                        wixProductRecord.collection.Length > 0 ? String.Join(';', GetCategory(wixProductRecord.collection).Skip(1)) : "",
                        wixProductRecord.price,
                        CleanupInventory(wixProductRecord.quantity)
                    );
                }
                else
                {
                    squareProductRecord = new SquareProductRecord(
                        wixProductRecord.handleId,
                        lastProduct.name,
                        wixProductRecord.options.Length > 0 ? FormatOptionNames(wixProductRecord.options) : "",
                        CleanupHTML(lastProduct.description),
                        lastProduct.collection.Length > 0 ? GetCategory(lastProduct.collection)[0] : "",
                        lastProduct.collection.Length > 0 ? String.Join(';',GetCategory(lastProduct.collection).Skip(1)) : "",
                        lastProduct.price,
                        CleanupInventory(wixProductRecord.quantity)
                    );

                }

                squareProductRecords.Add(squareProductRecord);
            }

            return squareProductRecords.ToArray();
        }

        static string CleanupInventory(string str)
        {
            switch (str)
            {
                default:
                    return str;

                case "InStock":
                    return "5";

                case "OutOfStock":
                    return "0";
            }
        }

        static string CleanupHTML(string str)
        {
            string decodedString = WebUtility.HtmlDecode(str);

            // Remove HTML tags
            string cleanString = Regex.Replace(decodedString, "<.*?>", string.Empty);

            return cleanString;
        }

        static string[] GetCategory(string collection)
        {
            return collection.Split(';').Where(x=>!x.Contains(" for ")).ToArray();
        }

        static string FormatOptionNames(string[] options)
        {
            return String.Join(" & ",options.Where(x => x.Length > 0).SelectMany(x => x.Split(";").Where(y=>y.Contains(':')).Select(y=>y.Split(':')[1])).Select(x=>x.Trim('"')).Take(3));
        }

        static WIXProductRecord[] GetWIXProductRecords(string inputfile)
        {
            List<WIXProductRecord> wixProductRecords = new List<WIXProductRecord>();

            using (StreamReader reader = new StreamReader(inputfile))
            {
                string headerLine = reader.ReadLine();
                string[] headers = splitRow(headerLine);

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] fields = splitRow(line);

                    var record = new WIXProductRecord(
                        safeGetValueByHeader("handleId", headers, fields, out string handleId) ? handleId : "",
                        safeGetValueByHeader("fieldType", headers, fields, out string fieldType) ? fieldType : "",
                        safeGetValueByHeader("name", headers, fields, out string name) ? name : "",
                        safeGetValueByHeader("description", headers, fields, out string description) ? description : "",
                        safeGetValueByHeader("productImageUrl", headers, fields, out string productImageUrl) ? productImageUrl : "",
                        safeGetValueByHeader("collection", headers, fields, out string collection) ? collection : "",
                        safeGetValueByHeader("price", headers, fields, out string price) ? price : "",
                        safeGetValueByHeader("inventory", headers, fields, out string quantity) ? quantity : "",
                        safeGetArrayByContainsHeader("productOptionDescription", headers, fields, out string[] options) ? options : new string[0]
                        );

                    wixProductRecords.Add(record);
                }
            }

            return wixProductRecords.ToArray();
        }

        static bool safeGetArrayByContainsHeader(string header, string[] headers, string[] fields, out string[] value)
        {
            List<string> values = new List<string>();

            for (int i = 0; i < headers.Length; i++)
            {
                if (headers[i].Contains(header))
                {
                    values.Add(fields[i]);
                }
            }

            value = values.ToArray();
            return true;
        }

        static bool safeGetValueByHeader(string header, string[] headers, string[] fields, out string value)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                if (headers[i] == header)
                {
                    value = fields[i];
                    return true;
                }
            }

            value = null;
            return false;
        }

        static string[] splitRow(string row)
        {
            List<string> strings = new List<string>();

            string temp = "";
            bool escaped = false;
            char lastC = ' ';
            foreach (char c in row)
            {
                if (c == ',' && !escaped)
                {
                    strings.Add(temp);
                    temp = "";
                }
                else
                {
                    temp += c;
                    if (c == '"' && lastC != '\\')
                    {
                        escaped = !escaped;
                    }
                }
                lastC = c;
            }

            strings.Add(temp);
            return strings.ToArray();
        }
    }
}
