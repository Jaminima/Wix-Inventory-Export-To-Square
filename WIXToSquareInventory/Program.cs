namespace WIXToSquareInventory
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No file arguments provided.");
                return;
            }

            string inputFilePath = args[0];

            if (!File.Exists(inputFilePath))
            {
                Console.WriteLine($"File {inputFilePath} does not exist.");
                return;
            }

            string outputFilePath = Path.Combine(Path.GetDirectoryName(inputFilePath), "output.csv");

        }
    }
}
