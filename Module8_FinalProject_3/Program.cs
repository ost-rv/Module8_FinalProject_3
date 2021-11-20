using System;
using System.IO;

namespace Module8_FinalProject_3
{
    class Program
    {
        static void Main(string[] args)
        {
            string directoryPath = string.Empty;
            if (args.Length > 0)
            {
                directoryPath = args[0];
            }
            else
            {
                Console.WriteLine("Аргумент прогроммы directoryPath не задан.");
                return;
            }
            

            if (string.IsNullOrEmpty(directoryPath) || string.IsNullOrWhiteSpace(directoryPath))
            {
                Console.WriteLine("Путь не указан");
                return;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

            if (!directoryInfo.Exists)
            {
                Console.WriteLine($"Папка по заданному ({directoryInfo.FullName}) пути не существует");
                return;
            }

            //размер перед очисткой
            long directoryBeginSize = 0;
            try
            {
                 directoryBeginSize = GetDirectorySize(directoryInfo);
                Console.WriteLine($"Исходный размер папки: {directoryBeginSize:N0} байт.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            //очистка
            (long ClearSize, int CountRemovedFile) dataClearDirectory = ClearDirectory(directoryInfo);

            Console.WriteLine($"Файлов {dataClearDirectory.CountRemovedFile:N0} удалено");
            

            //размер после очистки
            long directoryEndSize = 0;
            try
            {
                directoryEndSize = GetDirectorySize(directoryInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            Console.WriteLine($"Освобождено:  {dataClearDirectory.ClearSize:N0} общий объем удаленных файлов (подсчет)");
            Console.WriteLine($"Освобождено:  {directoryBeginSize - directoryEndSize:N0} общий объем удаленных файлов (по формуле)");

            Console.WriteLine($"Текущий размер папки: {directoryEndSize:N0} байт.");

        }

        private static (long ClearSize, int CountRemovedFile) ClearDirectory(DirectoryInfo directoryInfo)
        {
            (long ClearSize, int CountRemovedFile) result;
            result.ClearSize = 0;
            result.CountRemovedFile = 0;
            
            if (directoryInfo == null)
            {
                throw new ArgumentNullException(nameof(directoryInfo));
            }

            DirectoryInfo[] DirectoryInfos = directoryInfo.GetDirectories();
            foreach (DirectoryInfo di in DirectoryInfos)
            {
                result = ClearDirectory(di);
            }

            (long ClearSize, int CountRemovedFile) dataRemovedFiles = RemoveFiles(directoryInfo);

            result.ClearSize += dataRemovedFiles.ClearSize;
            result.CountRemovedFile = dataRemovedFiles.CountRemovedFile;

            TimeSpan timePassed = DateTime.Now.Subtract(directoryInfo.LastAccessTime);

            if (timePassed.TotalMinutes > 30
                && directoryInfo.GetFiles().Length == 0
                && directoryInfo.GetDirectories().Length == 0)
            {
                try
                {
                    directoryInfo.Delete();
                    //Console.WriteLine($"Папка {directoryInfo.FullName} удалена");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("--------------------------------------");
                    Console.WriteLine($"Ошибка при удалении папки {directoryInfo.FullName}");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("--------------------------------------");
                }
            }
            return result;
        }

        private static (long ClearSize, int CountRemovedFile) RemoveFiles(DirectoryInfo directoryInfo)
        {
            (long ClearSize, int CountRemovedFile) result;
            result.ClearSize = 0;
            result.CountRemovedFile = 0;

            if (directoryInfo == null)
            {
                return result;
            }


            foreach (FileInfo fi in directoryInfo.GetFiles())
            {
                TimeSpan timePassed = DateTime.Now.Subtract(fi.LastAccessTime);

                long curentSizeFile = 0;
                if (timePassed.TotalMinutes > 30)
                {
                    try
                    {
                        curentSizeFile = fi.Length;
                        fi.Delete();
                        //Console.WriteLine($"Файл {fi.FullName} удален");
                        result.CountRemovedFile += 1;
                        result.ClearSize += curentSizeFile;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("--------------------------------------");
                        Console.WriteLine($"Ошибка при удалении файла {fi.FullName}");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("--------------------------------------");
                    }
                }
            }
            return result;
        }

        private static long GetDirectorySize(DirectoryInfo directoryInfo)
        {
            long result = 0;
            if (directoryInfo == null)
                return result;

            DirectoryInfo[] dirInfos = directoryInfo.GetDirectories();
            long innerDirectorySize = 0;
            long fileSize = 0;
            foreach (DirectoryInfo di in dirInfos)
            {
                innerDirectorySize += GetDirectorySize(di);
            }

            FileInfo[] fileInfos = directoryInfo.GetFiles();

            foreach (FileInfo fi in fileInfos)
            {
                fileSize += fi.Length;
            }
            result = innerDirectorySize + fileSize;

            return result;
        }
    }
}
