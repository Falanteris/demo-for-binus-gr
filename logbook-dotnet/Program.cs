using Microsoft.Data.Sqlite;

string dbPath = "logbook.db";
string connectionString = $"Data Source={dbPath}";

// Initialize the database
InitializeDatabase();

Console.WriteLine("--- Welcome to your .NET Logbook ---");

while (true)
{
    Console.WriteLine("\nChoose an option: [1] Add Entry [2] View All [3] Exit");
    var choice = Console.ReadLine();

    if (choice == "1") AddEntry();
    else if (choice == "2") ViewEntries();
    else if (choice == "3") break;
    else Console.WriteLine("Invalid option.");
}

void InitializeDatabase()
{
    using (var connection = new SqliteConnection(connectionString))
    {
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = 
        @"
            CREATE TABLE IF NOT EXISTS Logs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                Content TEXT NOT NULL
            );
        ";
        command.ExecuteNonQuery();
    }
}

/*
void AddEntry()
{
    Console.Write("Enter your log message: ");
    string content = Console.ReadLine() ?? "";

    using (var connection = new SqliteConnection(connectionString))
    {
        connection.Open();
        var command = connection.CreateCommand();
        
        // VULNERABLE: Direct string interpolation
        command.CommandText = $"INSERT INTO Logs (Content) VALUES ('{content}')";
        
        command.ExecuteNonQuery();
    }
    Console.WriteLine("Log saved (dangerously)!");
}
*/

void AddEntry()
{
    Console.Write("Enter your log message: ");
    string content = Console.ReadLine() ?? "";

    using (var connection = new SqliteConnection(connectionString))
    {
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Logs (Content) VALUES ($content)";
        command.Parameters.AddWithValue("$content", content);
        command.ExecuteNonQuery();
    }
    Console.WriteLine("Log saved!");
}

void ViewEntries()
{
    Console.WriteLine("\n--- Your Logs ---");
    using (var connection = new SqliteConnection(connectionString))
    {
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Timestamp, Content FROM Logs ORDER BY Timestamp DESC";

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var id = reader.GetInt32(0);
                var time = reader.GetDateTime(1);
                var text = reader.GetString(2);
                Console.WriteLine($"[{id}] {time:yyyy-MM-dd HH:mm} - {text}");
            }
        }
    }
}