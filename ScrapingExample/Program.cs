using AngleSharp;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScrapingExample
{
    class Program
    {
        static int? ParseLength(string input)
        {
            var match = Regex.Match(input, @"(\d{1,2})\s*'\s*(\d{1,2})?");
            if (!match.Success)
                return null;

            var inches = match.Groups[2].Value;
            if (inches.Length == 0)
                inches = "0";
            var Answer = int.Parse(match.Groups[1].Value) * 12 + int.Parse(inches);

            if (Answer >= 180)
            {
                return null;
            }
            return Answer;
        }





        static void Main(string[] args)
        {
            var context = BrowsingContext.New(AngleSharp.Configuration.Default.WithDefaultLoader());
            var doc = context.OpenAsync("https://losangeles.craigslist.org/search/sga?query=surfboard").Result;

            var nodes =
                doc.QuerySelectorAll(".result-row")
                .Select(resultNode =>
                
                new
                {
                    Title =
                        resultNode
                        .QuerySelector(".result-title")
                        .TextContent
                        .Trim(),
                    Price =
                        resultNode
                        .QuerySelector(".result-price")
                        ?.TextContent // this checks for null reference error
                        .Trim(),

                    Image =
                        resultNode
                        .QuerySelector(".result-image")
                        ?.Attributes["data-ids"]
                        .Value,

                    Link =
                        resultNode
                        .QuerySelector(".result-title")
                        ?.Attributes["href"]
                        .Value
                });




            using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["SurfboardDBConnection"].ConnectionString))
            {

                con.Open();

                // run the deletes where the 'IsInternalUser' flag is false

                var cmd2 = con.CreateCommand();
                cmd2.CommandText = "DeleteScrappedPosts";
                cmd2.CommandType = CommandType.StoredProcedure;
                cmd2.ExecuteNonQuery();


                // run the inserts

                foreach (var node in nodes)
                {

                    var surfboard = new Surfboard();

                    surfboard.Name = node.Title;
                    surfboard.Link = node.Link;

                    if (!string.IsNullOrWhiteSpace(node.Price))
                    {
                        surfboard.Price = Int32.Parse(node.Price.Split(new Char[] { '$' })[1]);
                    }

                    surfboard.Image = "https://images.craigslist.org/" + node.Image.Split(new Char[] { ':', ',' })[1] + "_300x300.jpg";

                    

                    


                    if (node.Title.IndexOf("\'") > -1)
                    {
                        surfboard.Height = ParseLength(node.Title);
                    }

                    Console.WriteLine(surfboard.Image);



                    // insert starts here
                    var cmd = con.CreateCommand();
                    cmd.CommandText = "Surfboard_Post";
                    cmd.CommandType = CommandType.StoredProcedure;
                    {
                        // cmd.Parameters.AddWithValue("@Brand", "Test");
                        cmd.Parameters.AddWithValue("@Name", surfboard.Name);
                        cmd.Parameters.AddWithValue("@Height", surfboard.Height);
                        cmd.Parameters.AddWithValue("@Price", surfboard.Price);
                        cmd.Parameters.AddWithValue("@Link", surfboard.Link);
                        cmd.Parameters.AddWithValue("@Image", surfboard.Image);
                        cmd.Parameters.AddWithValue("@FromInternalUser", false);

                        SqlParameter idParameter = new SqlParameter("@Id", System.Data.SqlDbType.Int);

                        idParameter.Direction = System.Data.ParameterDirection.Output;

                        cmd.Parameters.Add(idParameter);

                        cmd.ExecuteNonQuery();

                    }
                }











                Console.ReadLine();
            }
        }
    }
}




