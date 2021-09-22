using LinenAndBird.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace LinenAndBird.DataAccess
{
    public class BirdRepository
    {

        readonly string _connectionString;

        //http request => IConfiguration => BirdRepository => Bird Controller

        public BirdRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("LinenAndBird");
        }
       
        internal IEnumerable<Bird> GetAll()
        {
            //DAPPER

            //using 2 queries to get all the birds and their accessories
            using var db = new SqlConnection(_connectionString);

            //Query<T> is for getting results from the database and putting them into C# type
            var birds = db.Query<Bird>(@"Select * From Birds"); //replaces all of the command and reader stuff, returns IEnumerable

            //get the accessories for all birds
            var accessorySql = @"Select *
                                From BirdAccessories";

            var accessories = db.Query<BirdAccessory>(accessorySql);

            foreach (var bird in birds)
            {
                bird.Accessories = accessories.Where(accessory => accessory.BirdId == bird.Id);
            }

            

            //ADO.NET
            //connections are like the tunnel between our app and the db
            //using var connection = new SqlConnection(_connectionString);

            //connection.Open(); //need in order to open connection to db, it starts closed by default

            //this is what tells SQL what we want to do
            //var command = connection.CreateCommand();
            //command.CommandText = @"Select *
            //                        From Birds";

            //execute reader is for when we care about getting all the results of our query
            //var reader = command.ExecuteReader();

            //var birds = new List<Bird>();

            //data readers are weird, only get one row from the results at a time
            //while (reader.Read())
            //{
            //Mapping data from the relational model to the object model
            //var bird = new Bird();
            //bird.Id = reader.GetGuid(0);
            //bird.Size = reader["Size"].ToString();
            //bird.Type = (BirdType)reader["Type"];
            //another option from line above, will return default if a BirdType is not entered
            //Enum.TryParse<BirdType>(reader["Type"].ToString(), out var BirdType); 
            //bird.Type = birdType;
            //bird.Color = reader["Color"].ToString();
            //bird.Name = reader["Name"].ToString();

            //each bird goes in the list to return later
            //    birds.Add(bird);
            //}

            return birds;
        }

        internal Bird Update(Guid id, Bird bird)
        {
            using var db = new SqlConnection(_connectionString);

            var sql = @"Update Birds
                        Set Color = @color,
                        Name = @name,
                        Type = @type,
                        Size = @size
                        output inserted.*
                        Where Id = @id";

            bird.Id = id;
            var updatedBird = db.QuerySingleOrDefault<Bird>(sql, bird);

            return updatedBird;

            //ADO.NET
            //using var connection = new SqlConnection(_connectionString);
            //connection.Open();

            //var cmd = connection.CreateCommand();
            //cmd.CommandText = @"Update Birds
            //                    Set Color = @color,
            //                     Name = @name,
            //                     Type = @type,
            //                     Size = @size
            //                    output inserted.*
            //                    Where Id = @id";

            //bird comes from the http request in the controller
            //cmd.Parameters.AddWithValue("Type", bird.Type);
            //cmd.Parameters.AddWithValue("Color", bird.Color);
            //cmd.Parameters.AddWithValue("Size", bird.Size);
            //cmd.Parameters.AddWithValue("Name", bird.Name);
            //cmd.Parameters.AddWithValue("id", id);

            //execution of the sql
            //var reader = cmd.ExecuteReader();

            //working with the sql results
            //    if(reader.Read())
            //    {
            //        return MapFromReader(reader);
            //    }
            //    return null;
        }


        internal void Remove(Guid id)
        {
            using var db = new SqlConnection(_connectionString);

            var sql = @"Delete 
                        From Birds 
                        Where Id = @id";

            db.Execute(sql, new { id }); // same as new { id = id } 

            //ADO.NET
            //using var connection = new SqlConnection(_connectionString);
            //connection.Open();

            //var cmd = connection.CreateCommand();
            //cmd.CommandText = @"Delete 
            //                    From Birds 
            //                    Where Id = @id";
            //cmd.Parameters.AddWithValue("id", id); //the @id and first id have to match, the second id matches the parameter

            //cmd.ExecuteNonQuery();
        }

        internal void Add(Bird newBird)
        {
            using var db = new SqlConnection(_connectionString);

            var sql = @"insert into birds(Type,Color,Size,Name)
                        output inserted.Id
                        values (@Type,@Color,@Size,@Name)";

            var id = db.ExecuteScalar<Guid>(sql, newBird);
            newBird.Id = id;

            //ADO.NET
            //using var connection = new SqlConnection(_connectionString);
            //connection.Open();

            //var cmd = connection.CreateCommand();
            //cmd.CommandText = @"insert into birds(Type,Color,Size,Name)
            //                    output inserted.Id
            //                    values (@Type,@Color,@Size,@Name)";

            //cmd.Parameters.AddWithValue("Type", newBird.Type);
            //cmd.Parameters.AddWithValue("Color", newBird.Color);
            //cmd.Parameters.AddWithValue("Size", newBird.Size);
            //cmd.Parameters.AddWithValue("Name", newBird.Name);

            //execute the query, but don't care about the results, just number of rows
            //var numberOfRowsAffected = cmd.ExecuteNonQuery();

            //execute the quert and only get the id of the new row
            //var newId = (Guid) cmd.ExecuteScalar();

            //newBird.Id = newId;
        }

        internal Bird GetById(Guid birdId)
        {
            //Get one to many relationships using 2 separate queries

            using var db = new SqlConnection(_connectionString);

            var birdSql = @"Select *
                        From Birds
                        Where id = @id";

            var bird = db.QuerySingleOrDefault<Bird>(birdSql, new {id = birdId }); //id matches @id, birdId match function parameter

            if (bird == null) return null;

            //get accessories for the bird.  one to many (one bird, many accessories)
            var accessorySql = @"Select *
                                From BirdAccessories
                                Where birdid = @birdId";

            var accessories = db.Query<BirdAccessory>(accessorySql, new { birdId });

            bird.Accessories = accessories;

            return bird;


            //ADO.NET
            //using var connection = new SqlConnection(_connectionString);
            //connection.Open(); 

            //var command = connection.CreateCommand();
            //command.CommandText = @"Select *
            //                        From Birds
            //                        Where id = @id"; //prevents sql injection
            //parameterization prevents sql injection
            //command.Parameters.AddWithValue("id", birdId);

            //var reader = command.ExecuteReader();

            //only want one row
            //if (reader.Read())
            //{
            //    return MapFromReader(reader);

            //}
            //return null;
        }

        //Used for ADO.NET
        //Bird MapFromReader(SqlDataReader reader)
        //{
        //    var bird = new Bird();
        //    bird.Id = reader.GetGuid(0);
        //    bird.Size = reader["Size"].ToString();
        //    bird.Type = (BirdType)reader["Type"];
        //    bird.Color = reader["Color"].ToString();
        //    bird.Name = reader["Name"].ToString();

        //    return bird;

        //}

    }
}
