using System;
using System.Collections.Generic;
using System.Text;

namespace FileManager
{
    static class Help
    {
        public static string ToString()
        {
            return
                "Список команд:\n" +
                "help этот текст\n" +
                "show <каталог> вывод каталога на консоль\n" +
                "<откуда> copy <куда> копирование файла или каталога\n" +
                "del <файл/каталог> удаление файла или каталога\n" +
                "info <файл/каталог> вывод информации о файле или каталоге\n";
        }
    }
}
