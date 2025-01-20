using Backend.Application.DTOs;

namespace Backend.Application.Interfaces;

public interface ICustomerService
{
    Task AddCustomer(CreateCustomerDto customerDto);
    Task<IEnumerable<CustomerDto>> GetAllCustomers();
    Task<CustomerDto> GetCustomerById(Guid id);
    Task UpdateCustomer(Guid id, UpdateCustomerDto customerDto);
    Task DeleteCustomer(Guid id);
}