using Microsoft.Data.Sqlite;

class Program
{
    static string dbPath = "inventory.db";

    static void Main(string[] args)
    {
        InitializeDatabase();
        bool running = true;

        while (running)
        {
            Console.WriteLine("\n=== Inventory Manager ===");
            Console.WriteLine("1. View all items");
            Console.WriteLine("2. Add item");
            Console.WriteLine("3. Update item quantity");
            Console.WriteLine("4. Delete item");
            Console.WriteLine("5. Search by category");
            Console.WriteLine("0. Exit");
            Console.Write("Choose an option: ");

            switch (Console.ReadLine())
            {
                case "1": ViewItems(); break;
                case "2": AddItem(); break;
                case "3": UpdateItem(); break;
                case "4": DeleteItem(); break;
                case "5": SearchByCategory(); break;
                case "0": running = false; break;
                default: Console.WriteLine("Invalid option."); break;
            }
        }
    }

    static SqliteConnection GetConnection()
    {
        return new SqliteConnection($"Data Source={dbPath}");
    }

    static void InitializeDatabase()
    {
        using var conn = GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Items (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Category TEXT NOT NULL,
                Quantity INTEGER NOT NULL,
                Price REAL NOT NULL
            );";
        cmd.ExecuteNonQuery();
        Console.WriteLine("Database initialized.");
    }

    static void ViewItems()
    {
        using var conn = GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Items ORDER BY Category, Name;";
        using var reader = cmd.ExecuteReader();

        Console.WriteLine("\n{0,-5} {1,-20} {2,-15} {3,-10} {4,-10}",
            "ID", "Name", "Category", "Qty", "Price");
        Console.WriteLine(new string('-', 65));

        bool hasRows = false;
        while (reader.Read())
        {
            hasRows = true;
            Console.WriteLine("{0,-5} {1,-20} {2,-15} {3,-10} {4,-10:C}",
                reader["Id"], reader["Name"], reader["Category"],
                reader["Quantity"], reader["Price"]);
        }
        if (!hasRows) Console.WriteLine("No items found.");
    }

    static void AddItem()
    {
        Console.Write("Name: ");
        string name = Console.ReadLine();
        Console.Write("Category: ");
        string category = Console.ReadLine();
        Console.Write("Quantity: ");
        int qty = int.Parse(Console.ReadLine());
        Console.Write("Price: ");
        double price = double.Parse(Console.ReadLine());

        using var conn = GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Items (Name, Category, Quantity, Price) VALUES ($name, $cat, $qty, $price);";
        cmd.Parameters.AddWithValue("$name", name);
        cmd.Parameters.AddWithValue("$cat", category);
        cmd.Parameters.AddWithValue("$qty", qty);
        cmd.Parameters.AddWithValue("$price", price);
        cmd.ExecuteNonQuery();
        Console.WriteLine("Item added.");
    }

    static void UpdateItem()
    {
        ViewItems();
        Console.Write("Enter ID to update: ");
        int id = int.Parse(Console.ReadLine());
        Console.Write("New quantity: ");
        int qty = int.Parse(Console.ReadLine());

        using var conn = GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Items SET Quantity = $qty WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$qty", qty);
        cmd.Parameters.AddWithValue("$id", id);
        int rows = cmd.ExecuteNonQuery();
        Console.WriteLine(rows > 0 ? "Item updated." : "Item not found.");
    }

    static void DeleteItem()
    {
        ViewItems();
        Console.Write("Enter ID to delete: ");
        int id = int.Parse(Console.ReadLine());

        using var conn = GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Items WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);
        int rows = cmd.ExecuteNonQuery();
        Console.WriteLine(rows > 0 ? "Item deleted." : "Item not found.");
    }

    static void SearchByCategory()
    {
        Console.Write("Enter category: ");
        string category = Console.ReadLine();

        using var conn = GetConnection();
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Items WHERE Category LIKE $cat ORDER BY Name;";
        cmd.Parameters.AddWithValue("$cat", $"%{category}%");
        using var reader = cmd.ExecuteReader();

        Console.WriteLine("\n{0,-5} {1,-20} {2,-15} {3,-10} {4,-10}",
            "ID", "Name", "Category", "Qty", "Price");
        Console.WriteLine(new string('-', 65));

        bool hasRows = false;
        while (reader.Read())
        {
            hasRows = true;
            Console.WriteLine("{0,-5} {1,-20} {2,-15} {3,-10} {4,-10:C}",
                reader["Id"], reader["Name"], reader["Category"],
                reader["Quantity"], reader["Price"]);
        }
        if (!hasRows) Console.WriteLine("No items found.");
    }
}