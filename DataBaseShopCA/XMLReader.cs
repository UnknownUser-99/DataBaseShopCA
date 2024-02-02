using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DataBaseShopCA
{
    class XMLReader
    {
        public List<Order> Orders { get; } = new List<Order>();

        public bool ReadXML(string filePath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            bool validation;
            validation = ValidationXML(xmlDoc);

            if (validation)
            {
                XmlNodeList orderNodes = xmlDoc.SelectNodes("/orders/order");

                foreach (XmlNode orderNode in orderNodes)
                {
                    Order order = new Order
                    {
                        Number = orderNode.SelectSingleNode("no").InnerText,
                        Date = orderNode.SelectSingleNode("reg_date").InnerText,
                        Sum = orderNode.SelectSingleNode("sum").InnerText,
                        UserName = orderNode.SelectSingleNode("user/fio").InnerText,
                        UserEmail = orderNode.SelectSingleNode("user/email").InnerText
                    };

                    XmlNodeList productNodes = orderNode.SelectNodes("product");

                    foreach (XmlNode productNode in productNodes)
                    {
                        Product product = new Product
                        {
                            Name = productNode.SelectSingleNode("name").InnerText,
                            Quantity = productNode.SelectSingleNode("quantity").InnerText,
                            Price = productNode.SelectSingleNode("price").InnerText
                        };

                        order.Products.Add(product);
                    }

                    Orders.Add(order);
                }
            }
            else
            {
                Console.WriteLine("XML файл не соответсвует необходимой для чтения структуре");
            }

            return validation;
        }

        private bool ValidationXML(XmlDocument xmlDoc)
        {
            XmlNodeList orderNodes = xmlDoc.SelectNodes("/orders/order");

            foreach (XmlNode orderNode in orderNodes)
            {
                if (orderNode.SelectSingleNode("no") == null ||
                    orderNode.SelectSingleNode("reg_date") == null ||
                    orderNode.SelectSingleNode("sum") == null ||
                    orderNode.SelectSingleNode("user/fio") == null ||
                    orderNode.SelectSingleNode("user/email") == null)
                {
                    return false;
                }

                XmlNodeList productNodes = orderNode.SelectNodes("product");

                foreach (XmlNode productNode in productNodes)
                {
                    if (productNode.SelectSingleNode("name") == null ||
                        productNode.SelectSingleNode("quantity") == null ||
                        productNode.SelectSingleNode("price") == null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
