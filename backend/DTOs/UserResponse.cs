using System;

namespace backend.DTOs;

public class UserResponse
{
   public string Email { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}
