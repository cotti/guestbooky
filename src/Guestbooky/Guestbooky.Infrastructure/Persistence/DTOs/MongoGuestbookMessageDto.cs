using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guestbooky.Infrastructure.Persistence.DTOs;

public class GuestbookMessageDto
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public required string Id { get; set; }

    [BsonElement("Author")]
    public required string Author { get; set; }

    [BsonElement("Message")]
    public required string Message { get; set; }

    [BsonElement("Timestamp")]
    public required DateTime Timestamp { get; set; }
}