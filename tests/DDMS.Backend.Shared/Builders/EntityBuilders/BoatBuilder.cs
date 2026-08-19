using DDMS.Backend.Common.Constants;
using DDMS.Backend.Models.Entities;
using DDMS.Backend.Shared.Constants;

namespace DDMS.Backend.Shared.Builders.EntityBuilders;

/// <summary>Builder cho entity <see cref="boat"/> — mặc định compliance hợp lệ (không bị block).</summary>
public class BoatBuilder
{
    private Guid _id = TestGuids.BoatId;
    private Guid? _ownerId = TestGuids.OwnerId;
    private string _name = "Rồng Vàng";
    private string _status = "active";
    private string _complianceStatus = BoatComplianceStatuses.Valid;
    private readonly List<boat_cabin> _cabins = new();
    private bool _isDeleted;
    private int _maxPassengers = 20;

    public BoatBuilder WithId(Guid id) { _id = id; return this; }
    public BoatBuilder WithOwnerId(Guid? ownerId) { _ownerId = ownerId; return this; }
    public BoatBuilder WithName(string name) { _name = name; return this; }
    public BoatBuilder WithComplianceStatus(string complianceStatus) { _complianceStatus = complianceStatus; return this; }
    public BoatBuilder WithCabins(params boat_cabin[] cabins) { _cabins.Clear(); _cabins.AddRange(cabins); return this; }
    public BoatBuilder WithDeleted(bool isDeleted) { _isDeleted = isDeleted; return this; }
    public BoatBuilder WithMaxPassengers(int maxPassengers) { _maxPassengers = maxPassengers; return this; }

    public boat Build()
    {
        var boat = new boat
        {
            id = _id,
            owner_id = _ownerId,
            name = _name,
            max_passengers = _maxPassengers,
            status = _status,
            compliance_status = _complianceStatus,
            created_at = DateTime.UtcNow.AddDays(-100),
            updated_at = DateTime.UtcNow.AddDays(-100),
            is_deleted = _isDeleted,
            boat_cabins = _cabins
        };

        foreach (var cabin in _cabins)
        {
            cabin.boat_id = boat.id;
            cabin.boat = boat;
        }

        return boat;
    }
}
