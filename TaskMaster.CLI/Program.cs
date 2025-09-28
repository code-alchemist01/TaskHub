using CommandLine;
using TaskMaster.CLI.Commands;

namespace TaskMaster.CLI
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            if (args.Length == 0)
            {
                ShowHelp();
                return 0;
            }

            var result = await Parser.Default.ParseArguments<
                ListCommand,
                AddCommand,
                UpdateCommand,
                DeleteCommand,
                SearchCommand
            >(args)
            .MapResult(
                async (BaseCommand command) => await command.ExecuteAsync(),
                errors => Task.FromResult(HandleParseError(errors))
            );

            return result;
        }

        private static void ShowHelp()
        {
            Console.WriteLine("TaskMaster - Görev Yönetim Sistemi");
            Console.WriteLine("==================================");
            Console.WriteLine();
            Console.WriteLine("Kullanılabilir komutlar:");
            Console.WriteLine("  list      - Görevleri listele");
            Console.WriteLine("  add       - Yeni görev ekle");
            Console.WriteLine("  update    - Mevcut görevi güncelle");
            Console.WriteLine("  delete    - Görevi sil");
            Console.WriteLine("  search    - Görevlerde arama yap");
            Console.WriteLine();
            Console.WriteLine("Detaylı yardım için: taskmaster [komut] --help");
            Console.WriteLine();
            Console.WriteLine("Örnekler:");
            Console.WriteLine("  taskmaster list");
            Console.WriteLine("  taskmaster add -t \"Yeni görev\" -d \"Açıklama\" --due 25.12.2024");
            Console.WriteLine("  taskmaster update 1 -s Completed");
            Console.WriteLine("  taskmaster search \"proje\"");
        }

        private static int HandleParseError(IEnumerable<Error> errors)
        {
            var errorList = errors.ToList();
            
            if (errorList.Any(e => e is HelpRequestedError || e is VersionRequestedError))
            {
                return 0;
            }

            Console.WriteLine("Komut hatası. Yardım için: taskmaster --help");
            return 1;
        }
    }
}
