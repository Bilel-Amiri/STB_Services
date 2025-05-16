using System;
using System.Collections.Generic;

namespace Card_Service.Models;

public partial class Card
{
    public int CardId { get; set; }

    public int AccountId { get; set; }

    public string CardNumber { get; set; } = null!;

    public string CardType { get; set; } = null!;

    public DateOnly ExpirationDate { get; set; }

    public string Status { get; set; } = null!;
}
