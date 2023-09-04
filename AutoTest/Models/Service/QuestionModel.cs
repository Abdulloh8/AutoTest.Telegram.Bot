
namespace AutoTest.Models.Service
{
    class QuestionModel
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string Description { get; set; }
        public QuestionMedia Media { get; set; }
        public List<QuestionChoice> choices { get; set; }
    }
}
