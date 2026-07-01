using System;
using MySqlConnector;

class Program {
    static void Main() {
        string connStr = "server=34.57.208.199;port=3306;database=boat_tour;uid=linh;pwd=123456;";
        using var conn = new MySqlConnection(connStr);
        try {
            conn.Open();
            var cmd = new MySqlCommand("SHOW COLUMNS FROM boats LIKE 'is_deleted';", conn);
            using var reader = cmd.ExecuteReader();
            if (reader.HasRows) {
                Console.WriteLine("is_deleted EXISTS");
            } else {
                Console.WriteLine("is_deleted DOES NOT EXIST");
            }
        } catch (Exception ex) {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
