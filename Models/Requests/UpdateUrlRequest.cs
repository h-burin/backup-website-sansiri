public class UpdateUrlRequest
{
    public int? url_id { get; set; }
    public string? url { get; set; }
    public string? url_thankyou { get; set; }
    public int? id_category_url { get; set; }
    public bool? is_delete { get; set; } = false;
}
