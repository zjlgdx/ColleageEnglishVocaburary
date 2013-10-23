using System.Collections.Generic;

namespace ColleageEnglishVocaburary.Model
{
    public class Book
    {
        public string Id { get; set; }

        public string BookName { get; set; }

        public List<Course> Courses { get; set; }
    }
}
