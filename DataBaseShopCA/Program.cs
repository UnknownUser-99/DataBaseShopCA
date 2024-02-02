using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseShopCA
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath;

            filePath = OpenFile();

            LoadFile(filePath);

            Console.ReadKey();

        }

        static string OpenFile()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fileName;

            Console.WriteLine("Введите имя файла:");
            fileName = Console.ReadLine();

            string filePath = $"{desktopPath}\\{fileName}.xml";

            return filePath;
        }

        static void LoadFile(string filePath)
        {
            XMLReader xmlReader = new XMLReader();

            bool validation;

            try
            {
                validation = xmlReader.ReadXML(filePath);

                foreach (var order in xmlReader.Orders)
                {
                    Console.WriteLine($"Order Number: {order.Number}, Reg Date: {order.Date}, Sum: {order.Sum}");
                    Console.WriteLine($"User: {order.UserName}, Email: {order.UserEmail}");

                    foreach (var product in order.Products)
                    {
                        Console.WriteLine($"  Product: {product.Name}, Quantity: {product.Quantity}, Price: {product.Price}");
                    }

                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                validation = false;

                Console.WriteLine($"Ошибка при загрузке файла: {ex.Message}");
            }

            if (validation)
            {
                Database db = new Database();

                try
                {
                    db.OpenConnection();

                    db.DataLoad(xmlReader.Orders);

                    Console.WriteLine("Данные загружены");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"При загрузке данных произошла ошибка: {ex.Message}");
                }
                finally
                {
                    db.CloseConnection();
                }
            }
            else
            {
                Console.WriteLine("Данные не были загружены из-за несоответствия XML файла необходимой для чтения структуре");
            }
        }
    }
}
