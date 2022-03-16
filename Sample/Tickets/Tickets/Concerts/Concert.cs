using Core.Aggregates;

namespace Tickets.Concerts;

public class Concert : HaveAggregate
{
    public string Name { get; private set; }

    public DateTime Date { get; private set; }

    public Concert(string name, DateTime date)
    {
        Name = name;
        Date = date;
    }
}