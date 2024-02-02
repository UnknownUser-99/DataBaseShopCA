using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DataBaseShopCA
{
    class Database
    {
        private SqlConnection sqlConnection = null;

        public void OpenConnection()
        {
            try
            {
                sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBShop"].ConnectionString);

                sqlConnection.Open();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Ошибка при открытии соединения: {ex.Message}");
            }
        }

        public void CloseConnection()
        {
            try
            {
                if (sqlConnection != null && sqlConnection.State == ConnectionState.Open)
                {
                    sqlConnection.Close();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Ошибка при закрытии соединения: {ex.Message}");
            }
        }

        public void DataLoad(List<Order> orders)
        {
            foreach (Order xmlOrder in orders)
            {
                int userId = UserCheck(xmlOrder);
                int saleId = InsertSale(userId, xmlOrder);
                InsertSalesProducts(saleId, xmlOrder.Products);
            }
        }

        private int ProductCheck(Product product)
        {
            int productID = 0;

            using (SqlCommand command = new SqlCommand("SELECT id FROM Products WHERE name = @productName", sqlConnection))
            {
                command.Parameters.AddWithValue("@productName", product.Name);
                productID = (int)(command.ExecuteScalar() ?? 0);

                if (productID == 0)
                {
                    using (SqlCommand insertCommand = new SqlCommand("INSERT INTO Products (name, price, quantity) OUTPUT INSERTED.id VALUES (@productName, @productPrice, @productQuantity)", sqlConnection))
                    {
                        insertCommand.Parameters.AddWithValue("@productName", product.Name);
                        insertCommand.Parameters.AddWithValue("@productPrice", product.Price);
                        insertCommand.Parameters.AddWithValue("@productQuantity", 0);

                        productID = (int)insertCommand.ExecuteScalar();
                    }
                }
            }

            return productID;
        }

        private int UserCheck(Order order)
        {
            int userID = 0;

            using (SqlCommand command = new SqlCommand("SELECT id FROM Users WHERE name = @userName AND email = @userEmail", sqlConnection))
            {
                command.Parameters.AddWithValue("@userName", order.UserName);
                command.Parameters.AddWithValue("@userEmail", order.UserEmail);
                userID = (int)(command.ExecuteScalar() ?? 0);

                if (userID == 0)
                {
                    using (SqlCommand insertCommand = new SqlCommand("INSERT INTO Users (name, email) OUTPUT INSERTED.id VALUES (@userName, @userEmail)", sqlConnection))
                    {
                        insertCommand.Parameters.AddWithValue("@userName", order.UserName);
                        insertCommand.Parameters.AddWithValue("@userEmail", order.UserEmail);

                        userID = (int)insertCommand.ExecuteScalar();
                    }
                }
            }

            return userID;
        }

        private int InsertSale(int userId, Order order)
        {
            int saleID = 0;

            using (SqlCommand command = new SqlCommand("INSERT INTO Sales (user_id, order_price, date) OUTPUT INSERTED.id VALUES (@userId, @orderPrice, @orderDate)", sqlConnection))
            {
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@orderPrice", order.Sum);
                command.Parameters.AddWithValue("@orderDate", order.Date);

                saleID = (int)command.ExecuteScalar();
            }

            return saleID;
        }

        private void InsertSalesProducts(int saleId, List<Product> products)
        {
            foreach (Product product in products)
            {
                int productID = ProductCheck(product);

                using (SqlCommand command = new SqlCommand("INSERT INTO Sales_products (sale_id, product_id, quantity, price) VALUES (@saleId, @productId, @quantity, @price)", sqlConnection))
                {
                    command.Parameters.AddWithValue("@saleId", saleId);
                    command.Parameters.AddWithValue("@productId", productID);
                    command.Parameters.AddWithValue("@quantity", product.Quantity);
                    command.Parameters.AddWithValue("@price", product.Price);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
