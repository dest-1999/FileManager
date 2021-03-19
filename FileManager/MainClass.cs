using System;
using System.IO;
namespace FileManager
{
    class MainClass
    {
        static void Main(string[] args)
        {
            //Console.WindowHeight = 40;
            string initPath = @"d:\downloads"; //Directory.GetDirectoryRoot(Directory.GetCurrentDirectory());

            ShowFileDirTree(initPath, 20);
            Console.ReadLine();
        }

        private static void ShowFileDirTree(string pathForShow, int pauseOnString)
        {
            string[] strArr;
            int tmp = 0;
            Console.Clear();
            Console.WriteLine($"{pathForShow}"); //показали текущий каталог

            foreach (var directory in Directory.GetDirectories(pathForShow, "*", SearchOption.TopDirectoryOnly)) //пробегаем текущий каталог
            {
                strArr = directory.Split('\\');

                Console.Write($"  [{strArr[strArr.Length - 1]}]");
                Console.CursorLeft = 70;
                Console.WriteLine(ElementInfo(directory));
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

    }
}
