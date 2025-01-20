using Backend.Domain.Entities;

namespace Backend.Domain.Interfaces;

public interface ICustomerRepository
{
    Task Add(Customer customer);
    Task<IEnumerable<Customer>> GetAll();
    Task<Customer?> GetById(Guid id);
    Task Update(Customer customer);
    Task Delete(Customer customer);
}