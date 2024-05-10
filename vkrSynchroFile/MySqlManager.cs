using MySql.Data.MySqlClient;

namespace vkrSynchroFile
{

    internal class MySqlManager
    {
        MySqlConnection connection = new MySqlConnection(Properties.Settings.Default.newconnectionString);

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
            try
            {
                connection.Open();
                MySqlCommand cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@ip", ip);
                cmd.Parameters.AddWithValue("@uniqueID", uniqueID);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при отправке запроса: " + ex.Message);
            }
            finally
            {
                connection.Close(); // Всегда закрываем соединение после попытки
            }
        }

        public void insertDB(string ip, string uniqueID)
        {
            string sql = @"INSERT INTO `Identifiers` (`ip`, `uniqueID`) VALUES (@ip, @uniqueID);";
            try
            {
                connection.Open();
                MySqlCommand cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@ip", ip);
                cmd.Parameters.AddWithValue("@uniqueID", uniqueID);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при отправке запроса: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        public bool searchDB(string uid)
        {
            string sql = @"SELECT `id` FROM `Identifiers` WHERE `uniqueID` = @uniqueID";
            bool result = false;

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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при отправке запроса: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return result;
        }

        public string searchIP_DB(string uid)
        {
            string sql = @"SELECT `ip` FROM `Identifiers` WHERE `uniqueID` = @uniqueID";
            string ip = "null";
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при отправке запроса: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }

            return ip;
        }
    }
}
