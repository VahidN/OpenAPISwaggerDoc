using AutoMapper;
using OpenAPISwaggerDoc.Models;
using Author = OpenAPISwaggerDoc.Entities.Author;

namespace OpenAPISwaggerDoc.Profiles;

public class AuthorProfile : Profile
{
    public AuthorProfile()
    {
        CreateMap<Author, Models.Author>();

        CreateMap<AuthorForUpdate, Author>();
    }
}