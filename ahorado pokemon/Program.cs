using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System;
using System.Collections.Generic;

class Program
{
    static Random random = new Random();

    static async System.Threading.Tasks.Task Main(string[] args)
    {
        var client = new HttpClient();
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

            string selectedName = pokemonNames[random.Next(pokemonNames.Count)];//escoje poke random
            string hiddenName = new string('_', selectedName.Length);//sustituir nombre por _
            int attemptsLeft = 6;//"vidas"
            List<char> guessedLetters = new List<char>();//letras adivinadas

            Console.WriteLine("¡Bienvenido al juego del ahorcado con nombres de Pokémon!");
            Console.WriteLine("Adivina el nombre del Pokémon: " + hiddenName);

            //while mientras tenga "vidas"
            while (attemptsLeft > 0)
            {
                Console.Write("Ingresa una letra: ");
                char guessChar = char.Parse(Console.ReadLine());

                //para las letras repetidas
                if (guessedLetters.Contains(guessChar))
                {
                    Console.WriteLine("Ya has intentado con esa letra.");
                    continue;//se regresa
                }

                guessedLetters.Add(guessChar);

                //Para sustituir los _ por las letras e imprimirlo
                if (selectedName.Contains(guessChar.ToString()))
                {
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
                    //quitar vidas
                    attemptsLeft--;
                    Console.WriteLine($"La letra '{guessChar}' no está en el nombre. Intentos restantes: {attemptsLeft}");
                    DisplayHangman(attemptsLeft);
                }

                Console.WriteLine("Nombre actual: " + hiddenName);

                //ganar
                if (hiddenName == selectedName)
                {
                    Console.WriteLine("¡Has adivinado el nombre del Pokémon!");
                    break;
                }
            }

            //perder
            if (hiddenName != selectedName)
            {
                Console.WriteLine("¡Has agotado tus intentos! El Pokémon era: " + selectedName);
                DisplayHangman(0);
            }
            Console.ReadKey();
        }
    }

    static void DisplayHangman(int attemptsLeft)
    {
        //dibujar al ahorcado
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
