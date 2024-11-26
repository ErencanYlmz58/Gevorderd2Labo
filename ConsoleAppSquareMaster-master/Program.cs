using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;

namespace ConsoleAppSquareMaster
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            World world = new World();
            var w = world.BuildWorld2(100, 100, 0.60);

            for (int i = 0; i < w.GetLength(1); i++)
            {
                for (int j = 0; j < w.GetLength(0); j++)
                {
                    char ch;
                    if (w[j, i]) ch = '*'; else ch = ' ';
                    Console.Write(ch);
                }
                Console.WriteLine();
            }

            Console.WriteLine("Werelden worden aangemaakt en opgeslagen in de database...");
            await GenerateAndStoreWorlds();
            Console.WriteLine("Werelden zijn succesvol aangemaakt en opgeslagen in de database.");

            // Bouw een wereld en voer het veroveringsproces uit
            WorldConquer wq = new WorldConquer(w);

            // Mapping van empire naar algoritme
            Dictionary<int, int> empireMapping = new Dictionary<int, int>
            {
                { 1, 1 },  // Empire 1 gebruikt Algoritme 1
                { 2, 2 },  // Empire 2 gebruikt Algoritme 2
                { 3, 3 }   // Empire 3 gebruikt Algoritme 3
            };

            // Voer het veroveringsproces uit met verschillende algoritmes per empire
            BitmapWriter bmw = new BitmapWriter();
            for (int algorithm = 1; algorithm <= 3; algorithm++)
            {
                // Maak een nieuwe WorldConquer instantie voor elke algoritme-run
                WorldConquer conquerInstance = new WorldConquer(w);
                conquerInstance.ConquerWithDifferentAlgorithms(new Dictionary<int, int> { { algorithm, algorithm } }, 25000);
                bmw.DrawWorld(conquerInstance.GetWorldEmpires(), $"Conquer{algorithm}_FinalResult");
            }

            Console.WriteLine("Veroveringen zijn voltooid en afbeeldingen zijn gegenereerd.");
            // Voer meerdere runs uit met verschillende startposities voor dezelfde wereld
            Console.WriteLine("Start met veroveringen met verschillende startposities...");
            await RunConquerMultipleStartPositions(world, aantalRuns: 5, aantalTurns: 2);
            Console.WriteLine("Veroveringen met verschillende startposities zijn voltooid en opgeslagen in de database.");

            // Voer het veroveringsproces uit voor verschillende werelden
            Console.WriteLine("Start met veroveringen voor verschillende werelden...");
            await RunConquerForMultipleWorlds(aantalWerelden: 3, aantalTurns: 2);
            Console.WriteLine("Veroveringen voor verschillende werelden zijn voltooid en opgeslagen in de database.");

            // Bereken statistieken voor elk algoritme
            Console.WriteLine("Start met het berekenen van statistieken voor elk algoritme...");
            await CalculateAlgorithmStatistics();
            Console.WriteLine("Statistieken voor elk algoritme zijn succesvol berekend en opgeslagen in de database.");
        }

        // Functie om de werelden te genereren en op te slaan in MongoDB
        private static async Task GenerateAndStoreWorlds()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("ConquerDB");
            var collection = database.GetCollection<WorldEntity>("Worlds");

            for (int i = 0; i < 10; i++)
            {
                var world = new WorldEntity
                {
                    Naam = $"World_{i + 1}",
                    AlgoritmeType = (i % 2 == 0) ? "KolomGeoriënteerd" : "StartpuntDekken",
                    MaxX = 100,
                    MaxY = 100,
                    Coverage = 0.6
                };
                await collection.InsertOneAsync(world);
            }
        }

        // Functie om veroveringsresultaten op te slaan in MongoDB
        private static async Task StoreConquerResults(Dictionary<int, (int size, double percentage)> empireSizes, Dictionary<int, int> empireMapping)
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("ConquerDB");
            var collection = database.GetCollection<BsonDocument>("ConquerResults");

            foreach (var entry in empireSizes)
            {
                int empireId = entry.Key;
                var sizeData = entry.Value;
                var algorithm = empireMapping[empireId];

                var document = new BsonDocument
                {
                    { "EmpireId", empireId },
                    { "Size", sizeData.size },
                    { "Percentage", sizeData.percentage },
                    { "Algorithm", algorithm }
                };

                await collection.InsertOneAsync(document);
            }

            Console.WriteLine("Conquer resultaten zijn succesvol opgeslagen in de database.");
        }

        private static async Task RunConquerMultipleStartPositions(World world, int aantalRuns, int aantalTurns)
        {
            // MongoDB setup
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("ConquerDB");
            var collection = database.GetCollection<BsonDocument>("ConquerResults");

            for (int run = 0; run < aantalRuns; run++)
            {
                // Creëer een nieuwe wereldconquer instantie voor elke run
                WorldConquer wq = new WorldConquer(world.BuildWorld2(100, 100, 0.60));

                // Mapping van empire naar algoritme (hier random voor elk empire)
                Dictionary<int, int> empireMapping = new Dictionary<int, int>
                {
                    { 1, 1 },  // Empire 1 gebruikt Algoritme 1
                    { 2, 2 },  // Empire 2 gebruikt Algoritme 2
                    { 3, 3 },  // Empire 3 gebruikt Algoritme 3
                    { 4, 1 },  // Empire 4 gebruikt Algoritme 1
                    { 5, 2 }   // Empire 5 gebruikt Algoritme 2
                };

                // Voer het veroveringsproces uit
                wq.ConquerWithDifferentAlgorithms(empireMapping, aantalTurns);

                // Bereken de empire sizes en sla ze op in MongoDB
                var empireSizes = wq.CalculateEmpireSizes();
                foreach (var entry in empireSizes)
                {
                    int empireId = entry.Key;
                    var sizeData = entry.Value;
                    var algorithm = empireMapping[empireId];

                    var document = new BsonDocument
                    {
                        { "RunId", run + 1 },
                        { "EmpireId", empireId },
                        { "Size", sizeData.size },
                        { "Percentage", sizeData.percentage },
                        { "Algorithm", algorithm }
                    };

                    await collection.InsertOneAsync(document);
                }

                Console.WriteLine($"Run {run + 1} resultaten succesvol opgeslagen in de database.");
            }
        }

        private static async Task RunConquerForMultipleWorlds(int aantalWerelden, int aantalTurns)
        {
            // MongoDB setup
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("ConquerDB");
            var collection = database.GetCollection<BsonDocument>("WorldConquerResults");

            for (int wereldIndex = 0; wereldIndex < aantalWerelden; wereldIndex++)
            {
                // Creëer een nieuwe wereld
                World world = new World();
                var w = world.BuildWorld2(100, 100, 0.60);

                // Creëer een nieuwe WorldConquer instantie
                WorldConquer wq = new WorldConquer(w);

                // Mapping van empire naar algoritme
                Dictionary<int, int> empireMapping = new Dictionary<int, int>
                {
                    { 1, 1 },  // Empire 1 gebruikt Algoritme 1
                    { 2, 2 },  // Empire 2 gebruikt Algoritme 2
                    { 3, 3 },  // Empire 3 gebruikt Algoritme 3
                    { 4, 1 },  // Empire 4 gebruikt Algoritme 1
                    { 5, 2 }   // Empire 5 gebruikt Algoritme 2
                };

                // Voer het veroveringsproces uit
                wq.ConquerWithDifferentAlgorithms(empireMapping, aantalTurns);

                // Bereken de empire sizes en sla ze op in MongoDB
                var empireSizes = wq.CalculateEmpireSizes();
                foreach (var entry in empireSizes)
                {
                    int empireId = entry.Key;
                    var sizeData = entry.Value;
                    var algorithm = empireMapping[empireId];

                    var document = new BsonDocument
                    {
                        { "WereldId", wereldIndex + 1 },
                        { "EmpireId", empireId },
                        { "Size", sizeData.size },
                        { "Percentage", sizeData.percentage },
                        { "Algorithm", algorithm }
                    };

                    await collection.InsertOneAsync(document);
                }

                Console.WriteLine($"Veroveringsresultaten voor wereld {wereldIndex + 1} zijn succesvol opgeslagen in de database.");
            }
        }

        private static async Task CalculateAlgorithmStatistics()
        {
            // MongoDB setup
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("ConquerDB");
            var collection = database.GetCollection<BsonDocument>("WorldConquerResults");

            // Aggregatie om de gemiddelde grootte en percentage per algoritme te berekenen
            var pipeline = new[]
            {
                new BsonDocument
                {
                    { "$group", new BsonDocument
                        {
                            { "_id", "$Algorithm" },
                            { "GemiddeldeGrootte", new BsonDocument { { "$avg", "$Size" } } },
                            { "GemiddeldPercentage", new BsonDocument { { "$avg", "$Percentage" } } }
                        }
                    }
                }
            };

            using var cursor = await collection.AggregateAsync<BsonDocument>(pipeline);

            // Sla de statistieken op in een nieuwe collectie
            var statsCollection = database.GetCollection<BsonDocument>("AlgorithmStatistics");
            await statsCollection.DeleteManyAsync(new BsonDocument()); // Verwijder oude statistieken

            while (await cursor.MoveNextAsync())
            {
                foreach (var result in cursor.Current)
                {
                    var document = new BsonDocument
                    {
                        { "Algorithm", result["_id"] },
                        { "GemiddeldeGrootte", result["GemiddeldeGrootte"] },
                        { "GemiddeldPercentage", result["GemiddeldPercentage"] }
                    };

                    await statsCollection.InsertOneAsync(document);
                }
            }

            Console.WriteLine("Statistieken per algoritme zijn succesvol berekend en opgeslagen in de database.");
        }
    }
}
