using System;
using System.IO;
namespace FileManager
{
    class MainClass
    {
        enum Command
        {
            help,
            show,
            copy,
            del,
            info,
        }
        static int pauseOnStr;
        static string initPath;

        static string fileSettingsName = "settings.ini";
        static void Main(string[] args)
        {
            (initPath, pauseOnStr) = InitSettings();

            while (true)
            {
                UserDialog();
            }



            //ShowFileDirTree(initPath, pauseOnStr);
            Console.ReadLine();
        }

        static void UserDialog()
        {
            Console.Write("CMD> ");
            string[] cmd = Console.ReadLine().Split(' ');
            Command command = Command.help;

            foreach (var item in cmd)
            {
                Enum.TryParse(typeof(Command), cmd[0], true, out object obj);
                if (obj != null)
                {
                    command = (Command)obj;
                    break;
                }
                else
                {
                    command = Command.help;
                }
            }


            switch (command)
            {
                case Command.show:
                    ShowFileDirTree(initPath, pauseOnStr);
                    break;
                case Command.copy:
                    Console.WriteLine("copy");
                    break;
                case Command.del:
                    Console.WriteLine("del");
                    break;
                case Command.info:
                    Console.WriteLine("info");
                    break;
                case Command.help:
                    Console.WriteLine(Help.ToString());
                    break;



                default:
                    break;
            }

        }


        private static void ShowFileDirTree(string pathForShow, int pauseOnString)
        {
            string[] strArr;
            int tmp = 1;
            //Console.Clear();
            Console.WriteLine($"{pathForShow}");

            foreach (var directory in Directory.GetDirectories(pathForShow, "*", SearchOption.TopDirectoryOnly)) //пробегаем текущий каталог
            {
                strArr = directory.Split('\\');

                Console.Write($"  [{strArr[strArr.Length - 1]}]");
                Console.CursorLeft = 70;
                Console.WriteLine(ElementInfo(directory));
                CheckForPause();
                try
                {
                    foreach (var item in Directory.GetDirectories (directory) )
                    {
                        InnerShow(item, true);
                    }

                    foreach (var item in Directory.GetFiles (directory))
                    {
                        InnerShow(item, false);
                    }
                }
                catch (Exception)
                {

                }
            }

            foreach (var item in Directory.GetFiles(pathForShow, "*", SearchOption.TopDirectoryOnly))
            {
                strArr = item.Split('\\');

                Console.Write($"  {strArr[strArr.Length - 1]}");
                Console.CursorLeft = 70;
                Console.WriteLine(ElementInfo(item));
                CheckForPause();
            }


            void CheckForPause()
            {
                if (pauseOnString == tmp)
                {
                    tmp = 0;
                    Console.Write("Для продолжения нажмите любую клавишу...");
                    Console.ReadKey();
                    Console.SetCursorPosition(0, Console.CursorTop--);
                    Console.Write("                                        \r");
                }
                tmp++;
            }

            void InnerShow(string item, bool isDirectory)
            {
                strArr = item.Split('\\');
                if (isDirectory)
                {
                    Console.Write($"    [{strArr[strArr.Length-1]}]");
                }
                else
                {
                    Console.Write($"    {strArr[strArr.Length - 1]}");
                }
                Console.CursorLeft = 70;
                Console.WriteLine(ElementInfo(item));
                CheckForPause();
            }
        }

        private static string ElementInfo(string dirOrFileName)
        {
            string[] strArr = dirOrFileName.Split('\\');
            ulong size = 0;
            int files = 0,
                dirs = 0;

            if (Directory.Exists(dirOrFileName)) //да, это каталог
            {
                DirectoryInfo dirInf = new DirectoryInfo(dirOrFileName);
                try
                {
                    dirs = dirInf.GetDirectories("*", SearchOption.AllDirectories).Length;
                    files = dirInf.GetFiles("*", SearchOption.AllDirectories).Length;
                    foreach (FileInfo item in dirInf.GetFiles("*", SearchOption.AllDirectories))
                    {
                        size += (ulong)item.Length;
                    }

                }
                catch (Exception)
                {
                }
                //return $"{strArr[strArr.Length - 1]}Подкаталогов: {dirs}, Файлов: {files}, Объем: {ConvertSizeInfo(size)}";
                return $"Подкаталогов: {dirs}, Файлов: {files}, Объем: {ConvertSizeInfo(size)}";
            }
            else //нет, это файл
            {
                FileInfo info = new FileInfo(dirOrFileName);
                return $"{ ConvertSizeInfo((ulong)info.Length) }";
            }

            static string ConvertSizeInfo(ulong length)
            {
                if (length < 1000)
                {
                    return $"{length} байт";
                }
                else if (length > 1000 & length < 1_000_000)
                {
                    return $"{length / 1000.0:F1} Кб";
                }
                else if (length > 1000_000 & length < 1000_000_000 )
                {
                    return $"{length / 1000_000.0:F2} Мб";
                }
                else
                {
                    return $"{length / 1000_000_000.0:F3} Гб";
                }
            }
        }

        static (string path, int pause) InitSettings()
        {
            string path;
            int pause;
            string[] strArr;
            if (File.Exists(fileSettingsName))
            {
                strArr = File.ReadAllLines(fileSettingsName);
                if (!string.IsNullOrEmpty(strArr[0]) && Directory.Exists(strArr[0]))
                {
                    path = strArr[0];
                }
                else
                {
                    path = Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());
                }

                if ( strArr.Length > 1 && int.TryParse(strArr[1], out pause) )
                {
                    return (path, pause);
                }
                else
                {
                    return (path, 20);
                }

            }
            return (Directory.GetDirectoryRoot(Directory.GetCurrentDirectory()), 20);
        }

    }
}
