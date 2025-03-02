namespace WebApi.DTOs;

public class PhotoForModerationDto
{
    public int Id { get; set; }
    public string Url { get; set; }
    public string Username { get; set; }
    public bool IsMain { get; set; }
    public bool IsApproved { get; set; }
}
