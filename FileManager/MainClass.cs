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
        static string errorLog = $"errors\\{Path.GetRandomFileName()}_exeption.txt";
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
                    userInputString = userInputString.Substring(userInputString.IndexOf(command.ToString()) + command.ToString().Length ).Trim() ;
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
                    Copy(userInputString);
                    break;
                case Command.del:
                    Delete(userInputString);
                    break;
                case Command.info:
                    Console.WriteLine(AdditionalInfo(userInputString));
                    break;
                case Command.help:
                    Console.WriteLine(Help.ToString());
                    break;

                default:
                    break;
            }

        }

        private static string AdditionalInfo(string userInputString)
        {
            string obj = CheckPathBuildFullPath(userInputString);
            if (obj == "")
            {
                return "Объект не найден";
            }
            string additionalInfo;
            if (Directory.Exists(obj))
            {
                DirectoryInfo di = new DirectoryInfo(obj);
                additionalInfo = $"\nСоздан: {di.CreationTime}\tИзменен {di.LastWriteTime}\nАтрибуты: {di.Attributes}";
            }
            else
            {
                FileInfo fi = new FileInfo(obj);
                additionalInfo = $"\nСоздан: {fi.CreationTime}\tИзменен {fi.LastWriteTime}\nАтрибуты: {fi.Attributes}";
            }

            return ElementInfo(obj) + additionalInfo;
        }

        private static void Delete(string userInputString)
        {
            userInputString = CheckPathBuildFullPath(userInputString);
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
            catch (Exception e)
            {
                Console.WriteLine($"При попытке удаления произошла ошибка\n{e}");
            }
        }

        private static void Copy(string userInputString)
      {
            string source = "", destination = "";
            string[] strArr = userInputString.Split(' ');
            int i = 1;
            if (!strArr[0].Contains(':') )
            {// путь источника относительный, достраиваем до абсолютного и начинаем проверять наличие объекта-источника
                if (currentDir[currentDir.Length - 1] != '\\' & strArr[0][0] != '\\' )
                {
                    currentDir += '\\';
                }
                source = currentDir + strArr[0];
            }
            else
            {
                source = strArr[0];
            }
            for ( ; !(Directory.Exists(source) || File.Exists(source)) & i < strArr.Length ; i++)
            {
                source += " " + strArr[i];
            }
            //пытаемся построить путь приёмника на основе остатков массива
            for ( ; i < strArr.Length; i++)
            {
                if (destination != "")
                {
                    destination += " ";
                }
                destination += strArr[i];
            }

            if (destination != "" && destination[1] != ':' )
            {// путь приёмника относительный, достраиваем
                if (currentDir[currentDir.Length - 1] != '\\' & destination[0] != '\\')
                {
                    currentDir += '\\';
                }
                destination = currentDir + destination;
            }

            if (!(Directory.Exists(source) || File.Exists(source)) || destination == "" )
            {//если объект-источник не найден или объект-приёмник не указан, то выходим из метода
                Console.WriteLine("Объект не найден, проверьте параметры");
                Console.WriteLine(Help.ToString());
                return;
            }

            if (Directory.Exists(destination) || File.Exists(destination))
            {
                Console.WriteLine("Объект с таким названием уже существует, указанное действие невозможно");
                return;
            }
            
            if (File.Exists(source))
            {
                Console.WriteLine("Копируем файл");
                File.Copy(source, destination);
            }
            else
            {
                


                DirCopy(new DirectoryInfo(source), new DirectoryInfo(destination));
            }

        }

        private static void DirCopy(DirectoryInfo source, DirectoryInfo destination)
        {
            if (source.FullName.ToLower() == destination.FullName.ToLower())
            {
                return;
            }

            if (!Directory.Exists(destination.FullName)) // == false)
            {
                Directory.CreateDirectory(destination.FullName);
            }


            try
            {
                foreach (FileInfo fi in source.GetFiles())
                {
                    fi.CopyTo(Path.Combine(destination.ToString(), fi.Name), true);
                }

            }
            catch (Exception e)
            {
                ErrorLog(e);
                Console.WriteLine(e);
            }

            try
            {
                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir = destination.CreateSubdirectory(diSourceSubDir.Name);
                    DirCopy(diSourceSubDir, nextTargetSubDir);
                }

            }
            catch (Exception e)
            {
                ErrorLog(e);
                Console.WriteLine(e);
            }






        }

        private static string CheckPathBuildFullPath(string userString)
        {
            if (currentDir[currentDir.Length - 1] != '\\')
            {
                currentDir += '\\';
            }

            if (Directory.Exists(userString) || File.Exists(userString))
            { //объект существует и путь полный
                return userString;
            }
            //объект не найден, пытаемся достроить путь
            userString = currentDir + userString;

            if (Directory.Exists(userString) || File.Exists(userString))
            { //объект существует
                return userString;
            }

            return "";
        }

        private static void ShowFileDirTree(string userString, int pauseOnString)
        {
            string pathForShow = CheckPathBuildFullPath(userString);
            if (pathForShow == "")
            {
                Console.WriteLine("Путь не найден");
                return;
            }

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
                catch (Exception e)
                {
                    ErrorLog(e);
                    Console.WriteLine($"\nПроизошла ошибка\n{e}");
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

        private static void ErrorLog(Exception e)
        {
            if (!Directory.Exists("errors"))
            {
                Directory.CreateDirectory("errors");
            }
            
            File.AppendAllText(errorLog, $"{DateTime.Now} возникла ошибка:\n{e}\n\n");
        }

        private static void SaveSettings(string currentDir)
        {
            string[] settingsArr = { currentDir, "stringsOnPage=" + pauseOnStr.ToString() };
            try
            {
                File.WriteAllLines(fileSettingsName, settingsArr);
            }
            catch (Exception e)
            {
                ErrorLog(e);
                Console.WriteLine("Неудается сохранить настройки");
            }

        }

        private static string ElementInfo(string dirOrFileName)
        {
            //dirOrFileName = dirOrFileName.Trim();
            //string[] strArr = dirOrFileName.Split('\\');
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
                catch (Exception e)
                {
                    ErrorLog(e);
                    Console.WriteLine($"\nПроизошла ошибка\n{e}");
                }
                //return $"{strArr[strArr.Length - 1]}Подкаталогов: {dirs}, Файлов: {files}, Объем: {ConvertSizeInfo(size)}";
                return $"Подкаталогов: {dirs}, Файлов: {files}, Объем: {ConvertSizeInfo(size)}";
            }
            else //нет, это файл
            {
                try
                {
                    FileInfo info = new FileInfo(dirOrFileName);
                    return $"{ ConvertSizeInfo((ulong)info.Length) }";
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    Console.WriteLine($"\nПроизошла ошибка\n{e}");
                }
                return "";
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
            if (File.Exists(fileSettingsName))
            {
                string[] strArr = File.ReadAllLines(fileSettingsName);
                if (!string.IsNullOrEmpty(strArr[0]) && Directory.Exists(strArr[0]))
                {
                    path = strArr[0];
                }
                else
                {
                    path = Directory.GetCurrentDirectory();
                }

                if (strArr.Length > 1 && strArr[1].Contains('='))
                {
                    string[] s = strArr[1].Split('=');
                    if (int.TryParse(s[s.Length - 1].Trim(), out int pause))
                    {
                        return (path, pause);
                    }
                    else
                    {
                        return (path, 20);
                    }
                }

            }
            return (Directory.GetCurrentDirectory(), 20);
        }

    }
}
