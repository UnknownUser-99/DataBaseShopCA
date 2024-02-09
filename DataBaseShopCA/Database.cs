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
                if (sqlConnection == null || sqlConnection.State != ConnectionState.Open)
                {
                    sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBShop"].ConnectionString);
                    sqlConnection.Open();
                }
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
            OpenConnection();

            try
            {
                foreach (Order xmlOrder in orders)
                {
                    using (SqlTransaction sqlTransaction = sqlConnection.BeginTransaction())
                    {
                        try
                        {
                            if (!SaleCheck(xmlOrder, sqlTransaction))
                            {
                                int userId = UserCheck(xmlOrder, sqlTransaction);
                                int saleId = InsertSale(userId, xmlOrder, sqlTransaction);
                                InsertSalesProducts(saleId, xmlOrder.Products, sqlTransaction);

                                sqlTransaction.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            if (sqlTransaction != null)
                            {
                                sqlTransaction.Rollback();
                            }

                            Console.WriteLine($"Ошибка при выполнении операции: {ex.Message}");
                        }
                        finally
                        {
                            if (sqlTransaction != null)
                            {
                                sqlTransaction.Dispose();
                            }
                        }
                    }
                }

                Console.WriteLine("Данные загружены.");
            }
            finally
            {
                CloseConnection();
            }
        }

        private bool SaleCheck(Order order, SqlTransaction transaction)
        {
            using (SqlCommand command = new SqlCommand("SELECT id FROM Sales WHERE id = @orderNumber", sqlConnection, transaction))
            {
                command.Parameters.AddWithValue("@orderNumber", order.Number);
                int result = (int)(command.ExecuteScalar() ?? 0);

                if (result != 0)
                {
                    Console.WriteLine($"Заказ с номером {order.Number} уже существует в базе данных.");

                    return true;
                }

                return false;
            }
        }

        private int ProductCheck(Product product, SqlTransaction transaction)
        {
            int productID = 0;

            using (SqlCommand command = new SqlCommand("SELECT id FROM Products WHERE name = @productName", sqlConnection, transaction))
            {
                command.Parameters.AddWithValue("@productName", product.Name);
                productID = (int)(command.ExecuteScalar() ?? 0);

                if (productID == 0)
                {
                    using (SqlCommand insertCommand = new SqlCommand("INSERT INTO Products (name, price, quantity) OUTPUT INSERTED.id VALUES (@productName, @productPrice, @productQuantity)", sqlConnection, transaction))
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

        private int UserCheck(Order order, SqlTransaction transaction)
        {
            int userID = 0;

            using (SqlCommand command = new SqlCommand("SELECT id FROM Users WHERE name = @userName AND email = @userEmail", sqlConnection, transaction))
            {
                command.Parameters.AddWithValue("@userName", order.UserName);
                command.Parameters.AddWithValue("@userEmail", order.UserEmail);
                userID = (int)(command.ExecuteScalar() ?? 0);

                if (userID == 0)
                {
                    using (SqlCommand insertCommand = new SqlCommand("INSERT INTO Users (name, email) OUTPUT INSERTED.id VALUES (@userName, @userEmail)", sqlConnection, transaction))
                    {
                        insertCommand.Parameters.AddWithValue("@userName", order.UserName);
                        insertCommand.Parameters.AddWithValue("@userEmail", order.UserEmail);

                        userID = (int)insertCommand.ExecuteScalar();
                    }
                }
            }

            return userID;
        }

        private int InsertSale(int userId, Order order, SqlTransaction transaction)
        {
            int saleID = 0;

            using (SqlCommand command = new SqlCommand("INSERT INTO Sales (id, user_id, order_price, date) OUTPUT INSERTED.id VALUES (@saleId, @userId, @orderPrice, @orderDate)", sqlConnection, transaction))
            {
                command.Parameters.AddWithValue("@saleId", order.Number);
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@orderPrice", order.Sum);
                command.Parameters.AddWithValue("@orderDate", order.Date);

                saleID = (int)command.ExecuteScalar();
            }

            return saleID;
        }

        private void InsertSalesProducts(int saleId, List<Product> products, SqlTransaction transaction)
        {
            foreach (Product product in products)
            {
                int productID = ProductCheck(product, transaction);

                using (SqlCommand command = new SqlCommand("INSERT INTO Sales_products (sale_id, product_id, quantity, price) VALUES (@saleId, @productId, @quantity, @price)", sqlConnection, transaction))
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
