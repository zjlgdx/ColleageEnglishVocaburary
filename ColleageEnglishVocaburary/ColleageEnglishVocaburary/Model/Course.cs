using System.Collections.Generic;

namespace ColleageEnglishVocaburary.Model
{
    public class Course
    {
        public string Id { get; set; }
        public string CourseName { get; set; }
        public string CourseImage { get; set; }
        public List<NewWord> NewWords { get; set; }
    }
}