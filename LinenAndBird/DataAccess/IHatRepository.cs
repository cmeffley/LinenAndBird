using LinenAndBird.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinenAndBird.DataAccess
{
    public interface IHatRepository
    {
        Hat GetById(Guid hatId);
        List<Hat> GetAll();
        IEnumerable<Hat> GetByStyle(HatStyle style);
        void Add(Hat newHat);


    }
}
