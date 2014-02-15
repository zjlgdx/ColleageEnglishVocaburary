using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColleageEnglishVocaburary.Model
{
    public interface IDataService
    {
        Task<IList<Course>> GetCourses(string bookId);
    }
}
