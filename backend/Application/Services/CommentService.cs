using Backend.Application.DTOs;
using Backend.Application.Interfaces;
using Backend.Domain.Entities;
using Backend.Domain.Exceptions;
using Backend.Domain.Interfaces;

namespace Backend.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICommentRepository _commentRepository;

    public CommentService(ICustomerRepository customerRepository, ICommentRepository commentRepository)
    {
        _customerRepository = customerRepository;
        _commentRepository = commentRepository;
    }

    public async Task AddComment(Guid customerId, CreateCommentDto commentDto)
    {
        var customer = await _customerRepository.GetById(customerId);
        if (customer == null)
        {
            throw new NotFoundException("Customer not found");
        }

        var comment = new Comment
        {
            Text = commentDto.Text,
            CustomerId = customerId
        };

        await _commentRepository.Add(comment);
    }

    public async Task UpdateComment(Guid commentId, UpdateCommentDto commentDto)
    {
        var comment = await _commentRepository.GetById(commentId);
        if (comment == null)
        {
            throw new NotFoundException("Comment not found");
        }

        if (!string.IsNullOrWhiteSpace(commentDto.Text))
        {
            comment.Text = commentDto.Text;
        }

        await _commentRepository.Update(comment);
    }

    public async Task DeleteComment(Guid commentId)
    {
        var comment = await _commentRepository.GetById(commentId);
        if (comment == null)
        {
            throw new NotFoundException("Comment not found");
        }

        await _commentRepository.Delete(comment);
    }
}