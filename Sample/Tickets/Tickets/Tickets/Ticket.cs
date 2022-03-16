using Core.Aggregates;

namespace Tickets.Tickets;

public class Ticket : HaveAggregate
{
    public Guid SeatId { get; private set; }

    public string Number { get; private set; }

    public Ticket(Guid seatId, string number)
    {
        SeatId = seatId;
        Number = number;
    }
}
