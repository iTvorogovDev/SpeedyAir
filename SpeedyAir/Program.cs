using System.Text.Json;

/*
 * Igor Tvorogov
 * itvorogov@hotmail.com
 * Coding Assessment
 * 
 * Assumptions:
 * - Valid, parsable JSON data is provided
 * - All orders in the provided schedule are shipping from YUL
*/
class Flight
{
    public required string FlightId { get; set; }
    public required string Origin { get; set; }
    public required string Destination { get; set; }
    public required int Day { get; set; }
    public int MaxCapacity { get; } = 20;
    public int Load { get; set; } = 0;
}

class Order
{
    public required string OrderId { get; set; }
    public required string Destination { get; set; }
    public string Origin { get; set; } = "YUL";
    public Flight? AssignedFlight { get; set; } = null;
}

class Program
{
    static void Main(string[] args)
    {
        // User story 1: Load and print a flight schedule on the console

        // Load flight schedule from the provided data file
        List<Flight> flightSchedule = LoadFlightSchedule("../../../data/flight-schedule.json");

        // Print the loaded flight schedule
        PrintFlightSchedule(flightSchedule);

        // User story 2: load orders, generate, and print an itinerary

        // Load order schedule from the provided data file
        List<Order> orderSchedule = LoadOrderSchedule("../../../data/orders.json");

        // Assign orders to flights based on the previously loaded flight schedule
        AssignFlightsToOrders(orderSchedule, flightSchedule);

        // Print the itinerary
        PrintOrderSchedule(orderSchedule);
    }

    static List<Flight> LoadFlightSchedule(string path)
    {
        // Read the JSON file into a string for parsing
        string jsonString = File.ReadAllText(path);

        // Parse the JSON string
        // Use a nested dictionary template due to the flights being represented as nested objects
        var flightDictionary = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonString);

        // Iterate through the dictionary and extract data for a collection of Flight objects
        List<Flight> flights = new List<Flight>();
        foreach (var flight in flightDictionary)
        {
            // Extract the flight ID from the outer object
            string flightId = flight.Key;

            // Parse day of the flight from provided data
            int day;
            try
            {
                day = Int32.Parse(flight.Value["day"]);

                // Create a new Flight object and add it to the list
                flights.Add(new Flight
                {
                    FlightId = flightId,
                    Day = day,
                    Destination = flight.Value["destination"],
                    Origin = flight.Value["origin"]
                });
            }
            catch (FormatException e)
            {
                // Skip loading the flight if provided day is not in integer format
                Console.WriteLine("Bad day format provided for flight ID {0}", flightId);
            }
        }

        return flights;
    }

    static List<Order> LoadOrderSchedule(string path)
    {
        // Read the JSON file into a string for parsing
        string jsonString = File.ReadAllText(path);

        // Parse the JSON string
        // Use a nested dictionary template due to the orders being represented as nested objects
        var orderDictionary = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonString);

        // Iterate through the dictionary and extract data for a collection of Order objects
        List<Order> orders = new List<Order>();
        foreach (var order in orderDictionary)
        {
            // Extract the order ID from the outer object
            string orderId = order.Key;

            // Create a new Order object and add it to the list
            orders.Add(new Order
            {
                OrderId = orderId,
                Destination = order.Value["destination"]
            });
        }

        return orders;
    }

    static void AssignFlightsToOrders(List<Order> orders, List<Flight> flights)
    {
        foreach (var order in orders)
        {
            // Query the flight list for the earliest flight available
            // (matches order origin, destination, and has available capacity)
            var matchingFlight = flights
                .Where(
                    f => f.Origin == order.Origin &&
                    f.Destination == order.Destination && 
                    f.MaxCapacity - f.Load > 0)
                .OrderBy(f => f.Day)
                .FirstOrDefault();

            if (matchingFlight != null)
            {
                order.AssignedFlight = matchingFlight;
                matchingFlight.Load++;
            }
        }
    }

    static void PrintFlightSchedule(List<Flight> flightSchedule)
    {
        foreach (var flight in flightSchedule)
        {
            Console.WriteLine(
                "Flight: {0}, departure: {1}, arrival: {2}, day: {3}", 
                flight.FlightId,
                flight.Origin,
                flight.Destination,
                flight.Day
            );
        }
    }

    static void PrintOrderSchedule(List<Order> orderSchedule)
    {
        foreach (var order in orderSchedule)
        {
            if (order.AssignedFlight == null)
            {
                Console.WriteLine(
                    "order: {0}, flightNumber: not scheduled",
                    order.OrderId
                );
            }
            else
            {
                Console.WriteLine(
                    "order: {0}, flightNumber: {1}, departure: {2}, arrival: {3}, day: {4}",
                    order.OrderId,
                    order.AssignedFlight.FlightId,
                    order.AssignedFlight.Origin,
                    order.AssignedFlight.Destination,
                    order.AssignedFlight.Day
                );
            }
        }
    }
}

