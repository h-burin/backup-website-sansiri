namespace backup_website.Models.TableSansiriUrlLog
{
    public class Result
    {
        public int? amount_url { get; set; }
        public DateTime? date_time { get; set; }
        public int? failed_url { get; set; }
        public int? id_log { get; set; }
        public int? success_url { get; set; }
        public string? time_processing { get; set; }
        public bool? status { get; set; }
        public string? error_message { get; set; }

    }

    public class TableSansiriUrlLog
    {
        public int? code { get; set; }
        public string? status { get; set; }
        public int? total { get; set; }
        public List<Result> Result { get; set; } = new List<Result>();

    }
}