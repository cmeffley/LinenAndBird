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
    public class HatRepository : IHatRepository
    {

        static List<Hat> _hats = new List<Hat>
        {
            new Hat
            {
                Id = Guid.NewGuid(),
                Color = "Blue",
                Designer = "Charlie",
                Style = HatStyle.OpenBack
            },
            new Hat
            {
                Id = Guid.NewGuid(),
                Color = "Black",
                Designer = "Nathan",
                Style = HatStyle.WideBrim
             },
            new Hat
            {
                Id = Guid.NewGuid(),
                Color = "Magenta",
                Designer = "Charlie",
                Style = HatStyle.Normal
            }
        };
        readonly string _connectionString;

        //http request => IConfiguration => BirdRepository => Bird Controller

        public HatRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("LinenAndBird");
        }

        public Hat GetById(Guid hatId)
        {
            using var db = new SqlConnection(_connectionString);

            var hat = db.QueryFirstOrDefault<Hat>("Select * From Hats Where Id = @id", new { id = hatId });

            return hat;

           //return _hats.FirstOrDefault(hat => hat.Id == hatId);
            
        }

        public List<Hat> GetAll()
        {
            return _hats;
        }
        public IEnumerable<Hat> GetByStyle(HatStyle style)
        {
            return _hats.Where(hat => hat.Style == style);
        }
        public void Add(Hat newHat)
        {
            newHat.Id = Guid.NewGuid();
            _hats.Add(newHat);
        }

    
    }
}
