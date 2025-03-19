namespace backup_website.Models.TableUrlCategory
{
    public class Result
    {
        public int id_category_url { get; set; }
        public string? name { get; set; }
        public bool? is_delete { get; set; }
    }

    public class TableUrlCategory
    {
        public int? code { get; set; }
        public string? status { get; set; }
        public int? total { get; set; }
        public List<Result> Result { get; set; } = new List<Result>();
    }

}