using System.Windows.Forms;

namespace app
{
    class Program // General program class
    {
        static void Main(string[] args) // App entry point
        {
            const string pathF31 = "F31.txt"; // F31 file fullpath
            const string pathFBL5N = "FBL5N.txt"; // FBL5N file fullpath

            FileParser.ParseFiles(pathF31, pathFBL5N); // Execute parsing and html-files creation

            MessageBox.Show(string.Format("В ходе работы создан(о) {0} файл(ов)", FileParser.FileCreatedCounter)); // Show message about count of created files
        }

    }
}
