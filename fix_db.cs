using System;
using MySqlConnector;

class Program
{
    static void Main()
    {
        string connStr = "Server=localhost;Database=ddms_db;User=root;Password=;";
        using var conn = new MySqlConnection(connStr);
        conn.Open();

        using var cmd = new MySqlCommand("ALTER TABLE boats ADD COLUMN owner_id CHAR(36) NULL AFTER id;", conn);
        try { cmd.ExecuteNonQuery(); Console.WriteLine("Added owner_id to boats."); } catch (Exception ex) { Console.WriteLine(ex.Message); }

        using var cmd2 = new MySqlCommand("ALTER TABLE boats ADD CONSTRAINT fk_boats_owner FOREIGN KEY (owner_id) REFERENCES users(id);", conn);
        try { cmd2.ExecuteNonQuery(); Console.WriteLine("Added FK for owner_id."); } catch (Exception ex) { Console.WriteLine(ex.Message); }
    }
}
