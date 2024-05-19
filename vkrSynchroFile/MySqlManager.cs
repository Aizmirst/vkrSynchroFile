using MySql.Data.MySqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Xml;

namespace vkrSynchroFile
{

    internal class MySqlManager
    {
        MySqlConnection connection = new MySqlConnection(Properties.Settings.Default.newconnectionString);

        /*public MySqlManager()
        {
            *//*string sql = @"CREATE TABLE IF NOT EXISTS `Identifiers` (
                        `id` int(11) NOT NULL AUTO_INCREMENT,
                        `ip` text NOT NULL,
                        `uniqueID` text NOT NULL,
                        PRIMARY KEY (`id`)
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8;";
            Console.WriteLine(sql);
            connection.Open();
            MySqlCommand cmd = new MySqlCommand(sql, connection);
            cmd.ExecuteNonQuery();
            connection.Close();*//*
            try
            {
                // Открытие соединения
                connection.Open();

                // Теперь вы можете выполнять операции с базой данных

                // Например, выполнение запроса SQL
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM ваша_таблица";
                MessageBox.Show("cool: ");
                // Выполнение запроса и обработка результата

                // Закрытие соединения
                connection.Close();
            }
            catch (MySqlException ex)
            {
                // Обработка ошибок подключения
                MessageBox.Show("Ошибка подключения: ");
            }
        }*/

        public void updateDB(string ip, string uniqueID)
        {
            string sql = @"UPDATE `Identifiers` SET `ip` = @ip WHERE `uniqueID` = @uniqueID";
            try
            {
                connection.Open();
                MySqlCommand cmd = new MySqlCommand(sql, connection);
                string encryptIP = EncryptString(ip);
                string encryptUID = EncryptString(uniqueID);
                cmd.Parameters.AddWithValue("@ip", encryptIP);
                cmd.Parameters.AddWithValue("@uniqueID", encryptUID);
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
                string encryptIP = EncryptString(ip);
                string encryptUID = EncryptString(uniqueID);
                cmd.Parameters.AddWithValue("@ip", encryptIP);
                cmd.Parameters.AddWithValue("@uniqueID", encryptUID);
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
                string encryptUID = EncryptString(uid);
                cmd.Parameters.AddWithValue("@uniqueID", encryptUID);
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
                string encryptUID = EncryptString(uid);
                cmd.Parameters.AddWithValue("@uniqueID", encryptUID);
                object resultObj = cmd.ExecuteScalar();

                if (resultObj != null)
                {
                    ip = DecryptString(resultObj.ToString());
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

        private static readonly string key = Properties.Settings.Default.aesKey; // 32 байта для AES-256
        private static readonly string iv = Properties.Settings.Default.aesIV; // 16 байт для AES

        public static string EncryptString(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(key);
                aesAlg.IV = Convert.FromBase64String(iv);

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public static string DecryptString(string cipherText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(key);
                aesAlg.IV = Convert.FromBase64String(iv);

                var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
}
