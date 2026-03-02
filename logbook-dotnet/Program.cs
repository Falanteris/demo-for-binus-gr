using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

public class LogController : Controller
{
    private string _connectionString = "Server=localhost;Database=LogbookDB;Trusted_Connection=True;TrustServerCertificate=True;";

    // 1. VULNERABLE ENDPOINT: Takes 'message' directly from the URL
    // Example: /Log/Add?message=Hello
    [HttpGet("Add")]
    public IActionResult Add(string message)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            
            // DANGER: SQL INJECTION (Raw Query)
            // If message is: '); DROP TABLE Logs;--
            string sql = "INSERT INTO Logs (Content) VALUES ('" + message + "')";
            
            var command = new SqlCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        return Content($"Logged: {message}");
    }

    // 2. VULNERABLE DISPLAY: Renders logs from the database
    [HttpGet("View")]
    public IActionResult ViewLogs()
    {
        var logs = new List<string>();

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("SELECT Content FROM Logs", connection);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read()) logs.Add(reader.GetString(0));
            }
        }

        // DANGER: XSS (Html.Raw)
        // If a log contains <script>alert(1)</script>, it will execute in the browser.
        string htmlOutput = "<h1>User Logs</h1><ul>";
        foreach (var log in logs)
        {
            htmlOutput += $"<li>{log}</li>"; // In a real Razor view, you'd use @Html.Raw(log)
        }
        htmlOutput += "</ul>";

        return Content(htmlOutput, "text/html");
    }
}