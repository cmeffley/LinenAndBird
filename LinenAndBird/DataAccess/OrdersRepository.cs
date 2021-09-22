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
    public class OrdersRepository
    {
        //static List<Order> _orders = new List<Order>();
        readonly string _connectionString;

        //http request => IConfiguration => BirdRepository => Bird Controller

        public OrdersRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("LinenAndBird");
        }

        internal IEnumerable<Order> GetAll()
        {
            using var db = new SqlConnection(_connectionString);

            var sql = @"Select *
                        From Orders o
	                        JOIN Birds b
	                            ON b.Id = o.BirdId
	                        JOIN Hats h
	                            ON h.Id = o.HatId";

            var results = db.Query<Order, Bird, Hat, Order>(sql, (order, bird, hat) => //one to one relationship ex: a customer has one shipping address
            {
                order.Bird = bird;
                order.Hat = hat;
                return order;
            }, splitOn:"Id");

            return results;

        }


        internal void Add(Order order)
        {
            //Create a connection
            using var db = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO [dbo].[Orders]
                            ([BirdId]
                            ,[HatId]
                            ,[Price])
                        output inserted.Id
                        VALUES
                            (@BirdId
                            ,@HatId
                            ,@Price)";

            var parameters = new 
            { 
                BirdId = order.Bird.Id, 
                HatId = order.Hat.Id,
                Price = order.Price
            };

            var id = db.ExecuteScalar<Guid>(sql, parameters);

            order.Id = id;

            //order.Id = Guid.NewGuid();

            //_orders.Add(order);
        }


        internal Order Get(Guid id)
        {
            using var db = new SqlConnection(_connectionString);

            var sql = @"Select *
                        From Orders o
	                        JOIN Birds b
	                            ON b.Id = o.BirdId
	                        JOIN Hats h
	                            ON h.Id = o.HatId  
                        Where o.Id = @id";

            //multimapping doesn't work for any other kind of dapper call, so we take the collection and turn it into one item ourselves.
            var orders = db.Query<Order, Bird, Hat, Order>(sql, (order, bird, hat) => 
            {
                order.Bird = bird;
                order.Hat = hat;
                return order;
            }, new { id }, splitOn: "Id");

            return orders.FirstOrDefault();

        }
       
    }
}
