using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static Random random = new Random(); //generar números aleatorios
    static HttpClient client = new HttpClient();
    static async Task Main(string[] args)
    {
        //lista de nombres de Pokémon desde la API
        List<string> pokemonNames = await GetPokemonNamesAsync();

        //nombre de Pokémon al azar de la lista
        string selectedName = pokemonNames[random.Next(pokemonNames.Count)];

        // Crear una cadena de guiones bajos del mismo tamaño que el nombre del Pokémon
        string hiddenName = new string('_', selectedName.Length);

        // Inicializar el contador de intentos restantes en 6
        int attemptsLeft = 6;

        // Crear una lista para llevar un registro de las letras adivinadas
        List<char> guessedLetters = new List<char>();

        Console.WriteLine("¡Bienvenido al juego del ahorcado con nombres de Pokémon!");
        Console.WriteLine("Adivina el nombre del Pokémon: " + hiddenName);
        //Console.WriteLine(selectedName);

        while (attemptsLeft > 0)
        {
            Console.Write("Ingresa una letra: ");
            char guessChar = char.Parse(Console.ReadLine());

            if (guessedLetters.Contains(guessChar))
            {
                Console.WriteLine("Ya has intentado con esa letra.");
                continue;
            }

            guessedLetters.Add(guessChar);

            if (selectedName.Contains(guessChar.ToString()))
            {
                // Actualizar la cadena con las letras adivinadas.
                char[] newHiddenName = hiddenName.ToCharArray();

                for (int i = 0; i < selectedName.Length; i++)
                {
                    if (selectedName[i] == guessChar)
                    {
                        newHiddenName[i] = guessChar;
                    }
                }

                hiddenName = new string(newHiddenName);
            }
            else
            {
                // Reducir el contador de intentos restantes y mostrar el ahorcado.
                attemptsLeft--;
                Console.WriteLine($"La letra '{guessChar}' no está en el nombre. Intentos restantes: {attemptsLeft}");
                DisplayHangman(attemptsLeft);
            }

            Console.WriteLine("Nombre actual: " + hiddenName);

            if (hiddenName == selectedName)
            {
                Console.WriteLine("¡Has adivinado el nombre del Pokémon!");

                // Obtener información adicional del Pokémon y mostrarla.
                await GetPokemonDetailsAsync(selectedName);

                break;
            }
        }

        if (hiddenName != selectedName)
        {
            Console.WriteLine("¡Has agotado tus intentos! El Pokémon era: " + selectedName);
            DisplayHangman(0);

            // Mostrar información del Pokémon incluso si se agotan los intentos.
            await GetPokemonDetailsAsync(selectedName);
        }

        Console.ReadKey();
    }

    // Función para obtener la lista de nombres de Pokémon desde la API.
    static async Task<List<string>> GetPokemonNamesAsync()
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://pokeapi.co/api/v2/pokemon?limit=100000&offset=0")
        };

        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            string body = await response.Content.ReadAsStringAsync();
            JObject jsonObject = JsonConvert.DeserializeObject<JObject>(body);

            JArray resultsArray = (JArray)jsonObject["results"];

            List<string> pokemonNames = new List<string>();

            foreach (JObject result in resultsArray)
            {
                string nombre = (string)result["name"];
                pokemonNames.Add(nombre);
            }

            return pokemonNames;
        }
    }

    // Función para obtener información adicional de un Pokémon
    static async Task GetPokemonDetailsAsync(string pokemonName)
    {
        // Crear una solicitud HTTP GET para obtener detalles del Pokémon
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://pokeapi.co/api/v2/pokemon/{pokemonName}")
        };

        // Enviar la solicitud y almacenar la respuesta en respuesta
        using (var respuesta = await client.SendAsync(request))
        {
            // Asegurarse de que la respuesta tenga un código de estado exitoso
            respuesta.EnsureSuccessStatusCode();

            // Leer el contenido de la respuesta en una cadena llamada body
            string body = await respuesta.Content.ReadAsStringAsync();

            // Deserializar el contenido JSON en un objeto JObject llamado pokemonData
            JObject pokemonData = JsonConvert.DeserializeObject<JObject>(body);

            // Extraer y formatear los tipos del Pokémon en una cadena type
            string type = string.Join(", ", pokemonData["types"].Select(t => (string)t["type"]["name"]));

            // Convertir el peso de decagramos a kilogramos
            double weight = (double)pokemonData["weight"] / 10.0;

            // Convertir la altura de decímetros a metros
            double height = (double)pokemonData["height"] / 10.0;

            // Imprimir los detalles del Pokémon en la consola
            Console.WriteLine($"Tipo: {type}");
            Console.WriteLine($"Peso: {weight} kg");
            Console.WriteLine($"Altura: {height} m");
        }
    }

    // Función para mostrar el dibujo del ahorcado.
    static void DisplayHangman(int attemptsLeft)
    {
        string[] hangmanArt =
        {
            "  +---+",
            "  |   |",
            $"  {(attemptsLeft < 6 ? "O" : " ")}   |",
            $" {(attemptsLeft < 4 ? "/" : " ")}{(attemptsLeft < 5 ? "|" : " ")}{(attemptsLeft < 3 ? "\\" : " ")}  |",
            $" {(attemptsLeft < 2 ? "/" : " ")} {(attemptsLeft < 1 ? "\\" : " ")}  |",
            "      |",
            "=========",
        };

        Console.WriteLine("\nAhorcado:");

        foreach (string line in hangmanArt)
        {
            Console.WriteLine(line);
        }

        Console.WriteLine();
    }
}
