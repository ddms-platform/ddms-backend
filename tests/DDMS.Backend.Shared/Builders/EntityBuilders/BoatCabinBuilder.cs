using DDMS.Backend.Models.Entities;
using DDMS.Backend.Shared.Constants;

namespace DDMS.Backend.Shared.Builders.EntityBuilders;

public class BoatCabinBuilder
{
    private Guid _id = TestGuids.CabinId;
    private Guid _boatId = TestGuids.BoatId;
    private string _name = "Cabin VIP";
    private int _capacity = 4;
    private decimal _price = 500_000m;
    private int _totalRooms = 5;

    public BoatCabinBuilder WithId(Guid id) { _id = id; return this; }
    public BoatCabinBuilder WithBoatId(Guid boatId) { _boatId = boatId; return this; }
    public BoatCabinBuilder WithName(string name) { _name = name; return this; }
    public BoatCabinBuilder WithCapacity(int capacity) { _capacity = capacity; return this; }
    public BoatCabinBuilder WithPrice(decimal price) { _price = price; return this; }
    public BoatCabinBuilder WithTotalRooms(int totalRooms) { _totalRooms = totalRooms; return this; }

    public boat_cabin Build() => new()
    {
        id = _id,
        boat_id = _boatId,
        name = _name,
        capacity = _capacity,
        price = _price,
        total_rooms = _totalRooms,
        created_at = DateTime.UtcNow.AddDays(-100),
        updated_at = DateTime.UtcNow.AddDays(-100)
    };
}
