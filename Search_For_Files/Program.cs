using System.Diagnostics;
using System.Text;
class Program
{
    class DirectoryCollector
    {
        List<DirectoryInfo> directories;                            //список директорий
        Stack<DirectoryInfo> pathFinder;                            //вспомогательный стек
        StringBuilder deniedDirColector;                            //сбор информации о директориях, в которые не был получен доступ
        StringBuilder foundFilesCollector;                          //сбор информации о найденных файлах
        Stopwatch timer;                                            //таймер
        int deniedCounter;                                          //счетчик директорий, в которые не был получен доступ
        int foundFilesCounter;                                      //счетчик найденных файлов

        public int Count { get { return directories.Count; } }      //свойство, возвращащее количество найденных директорий
        /// <summary>
        /// Default constructor
        /// </summary>
        public DirectoryCollector()
        {
            directories = new();
            pathFinder = new();
            deniedDirColector = new();
            foundFilesCollector = new();
            timer = new();
        }
        /// <summary>
        /// Method that clears all collected data
        /// </summary>
        public void ClearData()                                     //метод очистки всей накопленной информации
        {
            Console.WriteLine("Clearing old data\n");               //логирование
            directories.Clear();                                    
            pathFinder.Clear();
            deniedDirColector.Clear();
            foundFilesCollector.Clear();
            deniedCounter = 0;
            foundFilesCounter = 0;
        }
        /// <summary>
        /// Method that finds all subdirectories in root directory
        /// and add them to the list as well as root directory
        /// </summary>
        /// <param name="path">корневая директория</param>
        public void FindDirectories(DirectoryInfo path)             //метод поиска директорий
        {
            ClearData();                                            //предварительная очистка информации

            timer.Start();                                          //таймер

            Console.WriteLine($"Searching for directories in {path}");      //логирование
            directories.Add(path);                                          //добавление корневой директории в список
            pathFinder.Push(path);                                          //добавление корневой директории в стек

            while (pathFinder.Any())                                        //до тех пор, пока в стеке содержатся директории
            {
                try
                {
                    DirectoryInfo[] subdirecories = pathFinder.Pop().GetDirectories();      //получение информации о поддиректориях каждой директории в стеке
                    foreach (var subdirecory in subdirecories)                              //из списка полученных поддиректорий
                    {
                        directories.Add(subdirecory);                                       //добавление каждой поддиректории в список                                 
                        pathFinder.Push(subdirecory);                                       //добавление каждой поддиректории в стек
                    }
                }
                catch (UnauthorizedAccessException ue)                                      //если доступ в поддиректорию запрещен
                {
                    deniedDirColector.AppendLine(ue.Message);                               //добавление информации о закрытой поддиректории
                    deniedCounter++;                                                        //инкрементация счетчика закрытых директорий
                }
                catch (DirectoryNotFoundException)                                          //если поддиректории не существует
                {   
                    Console.WriteLine($"{path} not found");                                 //логирование
                }
            }
            Console.WriteLine($"{Count - 1} directories was found");                        //логирование
            Console.WriteLine($"Access denied to {deniedCounter} directories");             //логирование

            timer.Stop();                                                                   //остановка таймера
            Console.WriteLine($"Done by {timer.Elapsed}\n");                                //логирование
            timer.Reset();                                                                  //ообнуление таймера
        }
        /// <summary>
        /// Method that finds files in directory and all of it's subdirectories
        /// </summary>
        /// <param name="directory">root directory</param>
        /// <param name="pattern">pattern of the needed file</param>
        public void FindFile(DirectoryInfo directory, string pattern)   //метод поиска файла в корневой директории и всех ее поддиректорий
        {
            FindDirectories(directory);                             //поиск поддиректорий              
            timer.Start();                                          //старт таймера

            Console.WriteLine($"Searching for files [{pattern}] in {directory.FullName}");  //логирование
            if (directories.Any())                                                          //если список существует
            {
                foreach (var dir in directories)                                            //для каждой директории
                {
                    try
                    {
                        //получение всех файлов, соответствующих паттерну, в каждой поддиректории
                        string[] Filesfound = Directory.GetFiles(dir.FullName, pattern, SearchOption.TopDirectoryOnly);
                        //добавление информации о найденных файлах
                        foreach (string fileInfo in Filesfound) { foundFilesCollector.AppendLine(fileInfo); foundFilesCounter++; }
                    }
                    catch (UnauthorizedAccessException e) { }                               //если доступ запрещен, пропустить
                }
                Console.WriteLine($"{foundFilesCounter} files was found");                  //логирование

                timer.Stop();                                                               //остановка таймера
                Console.WriteLine($"Done by {timer.Elapsed}\n");                            //логирование
                timer.Reset();                                                              //обнуление таймера
            }
            else { Console.WriteLine("Directory list was empty\n"); }                       //логирование
        }
        /// <summary>
        /// Method that returns information about denied directories
        /// </summary>
        public void DeniedDirectories() 
        {
            //логирование
            if (deniedCounter == 0) { Console.WriteLine("There were no directories where access was denied in the last search\n"); return; }
            Console.WriteLine($"List of directories where access was denied in the last search:");
            Console.WriteLine(deniedDirColector.ToString());            //список всех поддиректорий, в которые закрыт доступ
            Console.WriteLine();                                        //отступ
        }
        /// <summary>
        /// Method that returns information about found files
        /// </summary>
        public void FilesFound()
        {
            //логирование
            if (foundFilesCounter == 0) { Console.WriteLine("No files were found in the last search\n"); return; }
            Console.WriteLine("List of files found in the last search");
            Console.WriteLine(foundFilesCollector.ToString());          //список всех найденных файлов 
            Console.WriteLine();                                        //отступ
        }
    }
    static void Main()
    {
        DirectoryInfo path = new DirectoryInfo(@"C:\Games");            //корневая папка
        DirectoryCollector dirCol = new();                              //экземпляр класса
        string pattern = @"*.txt";                                      //паттерн поиска

        dirCol.FindFile(path, pattern);                                 //поиск файла
        dirCol.FilesFound();                                            //сколько файлов найдено    
        dirCol.DeniedDirectories();                                     //сколько директорий, в которые запрещен доступ
    }
}