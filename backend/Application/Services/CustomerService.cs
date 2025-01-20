using Backend.Application.DTOs;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Domain.Exceptions;
using Backend.Domain.Interfaces;

namespace Backend.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task AddCustomer(CreateCustomerDto customerDto)
    {
        var customer = new Customer
        {
            Name = customerDto.Name,
            Email = customerDto.Email,
            PhoneNumber = customerDto.PhoneNumber,
        };

        await _customerRepository.Add(customer);
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomers()
    {
        var customers = await _customerRepository.GetAll();

        return customers.Select(c => new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            PhoneNumber = c.PhoneNumber,
            CreatedAt = c.CreatedAt
        });
    }

    public async Task<CustomerDto> GetCustomerById(Guid id)
    {
        var customer = await _customerRepository.GetById(id);

        if (customer == null)
        {
            throw new NotFoundException("Customer not found");
        }

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            CreatedAt = customer.CreatedAt,
            Comments = customer.Comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Text = c.Text,
                CreatedAt = c.CreatedAt
            }).ToList()
        };
    }

    public async Task UpdateCustomer(Guid id, UpdateCustomerDto customerDto)
    {
        var customer = await _customerRepository.GetById(id);

        if (customer == null)
        {
            throw new NotFoundException("Customer not found");
        }

        if (!string.IsNullOrWhiteSpace(customerDto.Name))
        {
            customer.Name = customerDto.Name;
        }

        if (!string.IsNullOrWhiteSpace(customerDto.Email))
        {
            if (!IsValidEmail(customerDto.Email))
            {
                throw new ValidationException("Invalid email format");
            }
            customer.Email = customerDto.Email;
        }

        if (!string.IsNullOrWhiteSpace(customerDto.PhoneNumber))
        {
            if (!IsValidPhoneNumber(customerDto.PhoneNumber))
            {
                throw new ValidationException("Invalid phone number format");
            }
            customer.PhoneNumber = customerDto.PhoneNumber;
        }

        await _customerRepository.Update(customer);
    }

    public async Task DeleteCustomer(Guid id)
    {
        var customer = await _customerRepository.GetById(id);

        if (customer == null)
        {
            throw new NotFoundException("Customer not found");
        }

        await _customerRepository.Delete(customer);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        return phoneNumber.All(char.IsDigit) && phoneNumber.Length >= 10;
    }
}