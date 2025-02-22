namespace backup_website.Models.TableSansiriUrl
{
    public class Result
    {
        public int? id_category_url { get; set; }
        public bool? is_active { get; set; }
        public bool? is_delete { get; set; }
        public string? url { get; set; }
        public string? category_name { get; set; }
        public int? url_id { get; set; }
        public string? url_thankyou { get; set; }
    }

    public class TableSansiriUrl
    {
        public int? code { get; set; }
        public string? status { get; set; }
        public int? total { get; set; }
        public List<Result> Result { get; set; } = new List<Result>();
    }
}

