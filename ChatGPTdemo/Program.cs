using ChatGPTdemo;
using System;
using System.Text.RegularExpressions;

var client = new OpenAIClient(ApiKey.Key);
var conversation = new List<Tuple<string, string>>();

CartItem[]? cart = null;

List<InventoryItem> groceryStoreInventory = new List<InventoryItem>()
{
    new InventoryItem { Name = "Bananas", Description = "Fresh bananas from Ecuador, the land of sunshine and rainforests! Great source of potassium.", Price = 2, Count = 50 },
    new InventoryItem { Name = "Apples", Description = "Juicy apples from Washington, the apple capital of the world! Perfect for snacking or baking.", Price = 3, Count = 30 },
    new InventoryItem { Name = "Oranges", Description = "Sweet oranges from sunny California, where the oranges are always ripe! Packed with vitamin C.", Price = 4, Count = 40 },
    new InventoryItem { Name = "Mangoes", Description = "Exotic mangoes from India, the land of spices and tropical fruits! Delicious in smoothies or as a snack.", Price = 5, Count = 20 },
    new InventoryItem { Name = "Avocados", Description = "Creamy avocados from Mexico, the land of tequila and tacos! Great on toast or in guacamole.", Price = 2, Count = 60 },
    new InventoryItem { Name = "Broccoli", Description = "Fresh broccoli from California, the state of healthy living! High in fiber and vitamin C.", Price = 3, Count = 25 },
    new InventoryItem { Name = "Salmon", Description = "Fresh salmon from Alaska, the state of pristine wilderness! Rich in omega-3 fatty acids.", Price = 10, Count = 15 },
    new InventoryItem { Name = "Cheese", Description = "Artisanal cheese from France, the land of cheese and wine! Perfect for a cheese board or melting on a sandwich.", Price = 8, Count = 10 },
    new InventoryItem { Name = "Eggs", Description = "Farm fresh eggs from local farmers! High in protein and perfect for breakfast.", Price = 4, Count = 50 },
    new InventoryItem { Name = "Spinach", Description = "Fresh spinach from California, the state of healthy living! High in iron and great for salads or sautéing.", Price = 2, Count = 35 },
};

var systemStatic = new[]
{
    "You are helpful assistant.",
    "You are grocery store seller.",
    "At the end of each reply, please append json object with buyers cart content.",
    "JSON schema for buyers cart content [{ \"Product\" : \"name\", \"Price\": 1, \"Amount\" : 1 }]",
    "Customer should enter /checkout to proceed with payment. Remind this time to time."
};

var systemDynamic = new[]
{
    () => $"Today is {DateTime.Now}",
    () => $"Sellers inventory {System.Text.Json.JsonSerializer.Serialize(groceryStoreInventory)}",
};

while (true)
{
    Console.Write(">>> ");
    string? input = Console.ReadLine();

    if (input?.ToLower() == "quit")
    {
        break;
    }

    if (input?.ToLower() == "/checkout")
    {
        if (cart != null && cart.Length > 0)
        {
            Console.WriteLine($"You have bought {string.Join(", ", cart.Select(x => x.Product))}");
            Console.WriteLine($"Total {cart.Sum(x => x.Price * x.Amount)}$");
            cart = null;
            conversation.Clear();
        }
        else
        {
            Console.WriteLine("BOT: Your cart is empty");
        }

        continue;
    }

    if (string.IsNullOrEmpty(input))
    {
        continue;
    }

    string output = await ProcessMessage(input);

    Console.WriteLine("BOT: " + output);

    Match match = Regex.Match(output, @"\s*(\{.*\}|\[.*\])\s*");

    if (match.Success)
    {
        UpdateCart(match.Value);
    }

    conversation.Add(new Tuple<string, string>(OpenAIClient.RoleUser, input));
    conversation.Add(new Tuple<string, string>(OpenAIClient.RoleAssistant, output));

    Console.WriteLine();
}

Console.WriteLine("Press any key to exit.");
Console.ReadKey();

async Task<string> ProcessMessage(string input)
{
    var system = string.Join(" ", systemDynamic.Select(x => x()).Concat(systemStatic));
    var query = new List<Tuple<string, string>>();
    query.Add(new Tuple<string, string>(OpenAIClient.RoleSystem, system));
    query.AddRange(conversation);
    query.Add(new Tuple<string, string>(OpenAIClient.RoleUser, input));
    var response = await client.Query(query);
    return response;
}

void UpdateCart(string json)
{
    try
    {
        cart = System.Text.Json.JsonSerializer.Deserialize<CartItem[]>(json);
        Console.WriteLine($"DEBUG: Your cart contains {cart?.Length} item(s)");
    }
    catch
    {
        Console.WriteLine("!!!!! The bot spoiled the json output");
    }
}

