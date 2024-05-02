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

        public MySqlManager()
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
        }

        public void insertDB(string ip, string uniqueID)
        {
            string sql = @"INSERT INTO `Identifiers` (`ip`, `uniqueID`) " +
                "VALUES (@ip, @uniqueID);";
            connection.Open();
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@ip", ip);
            cmd.Parameters.AddWithValue("@uniqueID", uniqueID);
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        public bool searchDB(string uid)
        {
            string sql = @"SELECT `id` FROM `Identifiers` WHERE `uniqueID` = @uniqueID";
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
            }
            
        }
        
        public string searchIP_DB(string uid)
        {
            string sql = @"SELECT 'ip' FROM 'Identifiers' WHERE 'uniqueID' = @uniqueID";
            string ip = "null";
            connection.Open();
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@uniqueID", uid);
            // Используем ExecuteScalar для получения единственного значения (ID)
            object result = cmd.ExecuteScalar();

            // Проверяем результат на null
            if (result != null)
            {
                // Преобразуем результат в строку
                ip = result.ToString();
            }

            connection.Close();

            return ip;

        }
    }
}
