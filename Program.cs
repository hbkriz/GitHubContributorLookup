using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace TestGe
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.github.com");
                client.DefaultRequestHeaders.Add("User-Agent", "007");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                PrintTopTenContributors(client);
                var gitHubMostCommitWeekResult = PrintTheMostCommitsWeekStats(client);
                PrintDayCommitsStats(gitHubMostCommitWeekResult.Days);
            }
        }

        static void PrintTopTenContributors(HttpClient client) 
        {
            Console.WriteLine("Top Ten GitHub contributors for Rails");
            var response = client.GetAsync("repos/rails/rails/contributors").Result;
            response.EnsureSuccessStatusCode();
            var contributors = response.Content.ReadAsAsync<List<Contributor>>().Result;
            var topTenContributors = contributors.OrderByDescending(t => t.Contributions).Take(10).ToList();
            topTenContributors.ForEach(Console.WriteLine);
        }

        static WeekContributions PrintTheMostCommitsWeekStats(HttpClient client) 
        {
            Console.WriteLine("\nMost Commits made week");
            var response = client.GetAsync("repos/rails/rails/stats/commit_activity").Result;
            response.EnsureSuccessStatusCode();
            var commits = response.Content.ReadAsAsync<List<WeekContributions>>().Result;
            var highestCommitsMadeWeek = commits.OrderByDescending(t => t.Total).Take(1).First();
            Console.WriteLine(highestCommitsMadeWeek);
            return highestCommitsMadeWeek;
        }

        static void PrintDayCommitsStats(int[] commitsPerDay){
            var DayOfTheWeekMaxCharacters = LongestDayCount();
            var maxCommit = commitsPerDay.Max();

            for(int i=0; i<commitsPerDay.Length;i++){
                var day = GitHubWeek[GitHubWeek.Keys.ElementAt(i)];
                var commits = PrintSymbols(commitsPerDay[i]);
                Console.WriteLine("{0} | {1} | {2}",  
                                            AddSpacing(DayOfTheWeekMaxCharacters, day),
                                            AddSpacing(maxCommit,PrintSymbols(commitsPerDay[i])),
                                            commitsPerDay[i]);
            }
        }

        static string PrintSymbols(int iterations)
        {
            var diagram=string.Empty;
            for(int i=0; i<iterations;i++){
                diagram+="=";
            }
            return diagram;
        }

        static string AddSpacing(int maxCount, string currentString)
        {
            var spacing = string.Empty;
            var difference= maxCount - currentString.Length;
            for(int i=0; i<difference;i++){
                spacing+=" ";
            }
            return currentString+spacing;
        }

        private static Dictionary<int, string> GitHubWeek = new Dictionary<int, string>()
        {
            { 0, "Sunday"},
            { 1, "Monday"},
            { 2, "Tuesday"},
            { 3, "Wednesday"},
            { 4, "Thursday"},
            { 5, "Friday"},
            { 6, "Saturday"}
        };


        private static int LongestDayCount() {
            return GitHubWeek.Select(a => a.Value).OrderByDescending(s => s.Length).First().Count();
        }
    }

    
    internal class Contributor
    {
        public string Login { get; set; }
        public short Contributions { get; set; }

        public override string ToString()
        {
            return $"{Login, 20}: {Contributions} contributions";
        }
    }

    internal class WeekContributions
    {
        public ulong Week { get; set; }
        public int Total { get; set; }
        public int[] Days {get; set;}
        
        public override string ToString()
        {
            return $"{Week, 20}: {Total} commits";
        }
    }
}
