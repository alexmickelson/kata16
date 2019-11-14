
using console.Enums;
using console.Models;

namespace console.Services 
{
    public interface IPackingSlipBuilder
    {
        PackingSlip Generate();
        void SendPackingSlipTo(Departments destinationDepartment);
        void AddItemToOrder(int firstAidVideoId);
    }
}