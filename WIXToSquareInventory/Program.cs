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

            List<WIXProductRecord> wixProductRecords = new List<WIXProductRecord>();

            using (StreamReader reader = new StreamReader(inputFilePath))
            {
                string headerLine = reader.ReadLine();
                string[] headers = splitRow(headerLine);

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] fields = splitRow(line);

                    var record = new WIXProductRecord(
                        safeGetValueByHeader("handleId",headers, fields, out string handleId) ? handleId : "",
                        safeGetValueByHeader("fieldType", headers, fields, out string fieldType) ? fieldType : "",
                        safeGetValueByHeader("name", headers, fields, out string name) ? name : "",
                        safeGetValueByHeader("description", headers, fields, out string description) ? description : "",
                        safeGetValueByHeader("productImageUrl", headers, fields, out string productImageUrl) ? productImageUrl : "",
                        safeGetValueByHeader("collection", headers, fields, out string collection) ? collection : "",
                        safeGetValueByHeader("price", headers, fields, out string price) ? price : "",
                        safeGetValueByHeader("inventory", headers, fields, out string quantity) ? quantity : "",
                        safeGetValueByHeader("options", headers, fields, out string options) ? options.Split('|') : new string[0]
                        );

                    wixProductRecords.Add(record);
                }
            }
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
