using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vkrSynchroFile
{

    internal class MySqlManager
    {
        MySqlConnection connection = new MySqlConnection(Properties.Settings.Default.connectionString);

        /*public MySqlManager()
        {
            string sql = @"CREATE TABLE IF NOT EXISTS `Identifiers` (
                        `id` int(11) NOT NULL AUTO_INCREMENT,
                        `ip` text NOT NULL,
                        `uniqueID` text NOT NULL,
                        PRIMARY KEY (`id`)
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8;";
            Console.WriteLine(sql);
            connection.Open();
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
            connection.Close();
        }*/

        public void updateDB(string ip, string uniqueID)
        {
            string sql = @"UPDATE `Identifiers` SET `ip` = @ip WHERE `uniqueID` = @uniqueID";
            //bool success = false;
            //int attempts = 0;

            //while (!success && attempts < 3) // Попытаемся отправить запрос не более 3 раз{
            try
            {
                connection.Open();
                MySqlCommand cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@ip", ip);
                cmd.Parameters.AddWithValue("@uniqueID", uniqueID);
                cmd.ExecuteNonQuery();
                //success = true; // Успешно отправлено
            }
            catch (Exception ex)
            {
                //attempts++; // Увеличиваем количество попыток
                            // Здесь можно добавить логирование ошибки, чтобы понять причину
                Console.WriteLine("Ошибка при отправке запроса: " + ex.Message);
            }
            finally
            {
                connection.Close(); // Всегда закрываем соединение после попытки
            }
            //}

            /*if (!success)
            {
                Console.WriteLine("Не удалось выполнить запрос после 3 попыток.");
                // Здесь можно принять решение о дальнейших действиях, например, выйти из метода или сгенерировать исключение.
            }*/
        }

        public void insertDB(string ip, string uniqueID)
        {
            /*string sql = @"INSERT INTO `Identifiers` (`ip`, `uniqueID`) " +
                "VALUES (@ip, @uniqueID);";
            connection.Open();
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@ip", ip);
            cmd.Parameters.AddWithValue("@uniqueID", uniqueID);
            cmd.ExecuteNonQuery();
            connection.Close();*/

            string sql = @"INSERT INTO `Identifiers` (`ip`, `uniqueID`) VALUES (@ip, @uniqueID);";
            //bool success = false;
            //int attempts = 0;

            //while (!success && attempts < 3) // Попытаемся отправить запрос не более 3 раз
            //{
                try
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@ip", ip);
                    cmd.Parameters.AddWithValue("@uniqueID", uniqueID);
                    cmd.ExecuteNonQuery();
                    //success = true; // Успешно отправлено
                }
                catch (Exception ex)
                {
                   // attempts++; // Увеличиваем количество попыток
                                // Здесь можно добавить логирование ошибки, чтобы понять причину
                    Console.WriteLine("Ошибка при отправке запроса: " + ex.Message);
                }
                finally
                {
                    connection.Close(); // Всегда закрываем соединение после попытки
                }
            //}

            /*if (!success)
            {
                Console.WriteLine("Не удалось выполнить запрос после 3 попыток.");
                // Здесь можно принять решение о дальнейших действиях, например, выйти из метода или сгенерировать исключение.
            }*/
        }

        public bool searchDB(string uid)
        {
            /*string sql = @"SELECT `id` FROM `Identifiers` WHERE `uniqueID` = @uniqueID";
            connection.Open();
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@uniqueID", uid);
            // Используем ExecuteScalar для получения единственного значения (ID)
            object result = cmd.ExecuteScalar();

            // Если результат запроса не равен DBNull (т.е. запись найдена), преобразуем его в int
            if (result != DBNull.Value && result != null)
            {
                connection.Close();
                return true;
            }
            else
            {
                connection.Close();
                return false;
            }*/

            string sql = @"SELECT `id` FROM `Identifiers` WHERE `uniqueID` = @uniqueID";
            //bool success = false;
            //int attempts = 0;
            bool result = false;

            //while (!success && attempts < 3) // Попытаемся отправить запрос не более 3 раз
            //{
                try
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@uniqueID", uid);
                    object resultObj = cmd.ExecuteScalar();

                    if (resultObj != DBNull.Value && resultObj != null)
                    {
                        result = true;
                    }

                    //success = true; // Успешно выполнено
                }
                catch (Exception ex)
                {
                    //attempts++; // Увеличиваем количество попыток
                                // Здесь можно добавить логирование ошибки, чтобы понять причину
                    Console.WriteLine("Ошибка при отправке запроса: " + ex.Message);
                }
                finally
                {
                    connection.Close(); // Всегда закрываем соединение после попытки
                }
            //}

            return result;
        }
        
        public string searchIP_DB(string uid)
        {

            string sql = @"SELECT `ip` FROM `Identifiers` WHERE `uniqueID` = @uniqueID";
            string ip = "null";
            //bool success = false;
            //int attempts = 0;

            //while (!success && attempts < 3) // Попытаемся отправить запрос не более 3 раз
            //{
                try
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@uniqueID", uid);
                    object resultObj = cmd.ExecuteScalar();

                    if (resultObj != null)
                    {
                        ip = resultObj.ToString();
                    }

                    //success = true; // Успешно выполнено
                }
                catch (Exception ex)
                {
                    //attempts++; // Увеличиваем количество попыток
                                // Здесь можно добавить логирование ошибки, чтобы понять причину
                    Console.WriteLine("Ошибка при отправке запроса: " + ex.Message);
                }
                finally
                {
                    connection.Close(); // Всегда закрываем соединение после попытки
                }
            //}

            return ip;
        }
    }
}
