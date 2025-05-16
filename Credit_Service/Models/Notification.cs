using System;
using System.Collections.Generic;

namespace Credit_Service.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int AccountId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? NotificationDate { get; set; }

    public string Status { get; set; } = null!;
}
