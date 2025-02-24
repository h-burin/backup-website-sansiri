namespace backup_website.Models.TableSansiriUrlLogDetail
{
    public class Result
    {
        public int? id_log { get; set; }
        public int? id_log_detail { get; set; }
        public bool? status { get; set; }
        public string? status_detail { get; set; }
        public string? url { get; set; }
    }

    public class TableSansiriUrlLogDetail
    {
        public int? code { get; set; }
        public string? status { get; set; }
        public int? total { get; set; }
        public List<Result> Result { get; set; } = new List<Result>();
    }
}
