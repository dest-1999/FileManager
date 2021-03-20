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
        static int pauseOnStr; //значение пейджинга
        static string currentDir; //текущий каталог

        const string fileSettingsName = "settings.ini";
        static void Main()
        {
            (currentDir, pauseOnStr) = InitSettings();
            
            ShowFileDirTree(currentDir, int.MaxValue );
            while (true)
            {
                UserDialog();
            }
        }

        static void UserDialog()
        {
            Console.Write($"{currentDir}> ");
            string userInputString = Console.ReadLine().Trim();
            string[] userInputStringArr = userInputString.Split(' ');
            Command command = Command.help;

            foreach (var item in userInputStringArr)
            {// определяем команду из строки введенной пользователем
                if (Enum.TryParse(typeof(Command), item, true, out object obj) && obj != null)
                {
                    command = (Command)obj; //команда выяснена, далее удаляем её из строки
                    userInputString = userInputString.Substring(userInputString.IndexOf(command.ToString()) + command.ToString().Length + 1);
                    break;
                }
                else
                {//если команда не найдена - выводим помощь
                    command = Command.help;
                }
            }


            switch (command)
            {
                case Command.show:
                    ShowFileDirTree(userInputString, pauseOnStr);
                    break;
                case Command.copy:
                    Console.WriteLine("copy");
                    //TODO Copy(userInputStringArr);
                    break;
                case Command.del:
                    Console.WriteLine("del");

                    Delete(userInputString);

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

        private static void Delete(string userInputString)
        {

            userInputString = userInputString.Trim();
            userInputString = BuildFullPath(userInputString);
            try
            {
                if ( Directory.Exists(userInputString) )
                {
                    Directory.Delete(userInputString, true);
                    Console.WriteLine("Каталог удалён");
                }
                else if ( File.Exists(userInputString) )
                {
                    File.Delete(userInputString);
                    Console.WriteLine("Файл удалён");
                }
                else
                {
                    Console.WriteLine("Объект для удаления не найден");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("При попытке удаления произошла ошибка");
            }
        }

        private static void Copy(string[] userInputString)
        {
            Console.WriteLine("\n\nСДЕЛАТЬ КОПИРОВАНИЕ");
        }

        private static string BuildFullPath(string userString)
        {
            userString = userString.Trim();
            if (currentDir[currentDir.Length - 1] != '\\')
            {
                currentDir += '\\';
            }

            if (Directory.Exists(userString) || File.Exists(userString))
            { //объект существует
                return userString;
            }
            //объект не найден, пытаемся достроить путь
            userString = currentDir + userString;

            if (Directory.Exists(userString) || File.Exists(userString))
            { //объект существует
                return userString;
            }

            Console.WriteLine("Путь не найден");
            return "";
        }

        private static void ShowFileDirTree(string userString, int pauseOnString)
        {
            string pathForShow = BuildFullPath(userString) ;

            //userString = userString.Trim();
            //if (currentDir[currentDir.Length-1] != '\\' )
            //{
            //    currentDir += '\\' ;
            //}

            //if (Directory.Exists(userString))
            //{
            //    pathForShow = userString;
            //}
            //else
            //{
            //    pathForShow = currentDir + userString;
            //}
            //if (!Directory.Exists(pathForShow))
            //{
            //    Console.WriteLine("Путь не найден");
            //    return;
            //}

            currentDir = pathForShow;
            SaveSettings(currentDir);
            string[] strArr;
            int stringCounter = 1;

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
                if (pauseOnString == stringCounter)
                {
                    stringCounter = 0;
                    Console.Write("Для продолжения нажмите любую клавишу...");
                    Console.ReadKey();
                    Console.SetCursorPosition(0, Console.CursorTop--);
                    Console.Write("                                        \r");
                }
                stringCounter++;
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

        private static void SaveSettings(string currentDir)
        {
            string[] settingsArr = { currentDir, pauseOnStr.ToString() };
            try
            {
                File.WriteAllLines(fileSettingsName, settingsArr);
            }
            catch (Exception)
            {
                Console.WriteLine("Неудается сохранить настройки");
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

                if ( strArr.Length > 1 && int.TryParse(strArr[1], out int pause) )
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
