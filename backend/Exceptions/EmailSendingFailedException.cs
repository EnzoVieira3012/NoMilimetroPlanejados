using System;

namespace Backend.Domain.Exceptions;

public class EmailSendingFailedException : Exception
{
    public EmailSendingFailedException(string message) : base(message)
    {
    }
}