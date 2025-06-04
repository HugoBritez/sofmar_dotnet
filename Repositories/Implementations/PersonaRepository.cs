using Api.Data;
using Api.Models.Entities;
using Api.Repositories.Interfaces;

namespace Api.Repositories.Implementations
{
    public class PersonaRepository : IPersonaRepository
    {
        private readonly ApplicationDbContext _context;

        public PersonaRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<Persona> CreateAsync(Persona persona)
        {
            // Agregar la entidad a la base de datos
            var personaCreada =  await _context.Personas.AddAsync(persona);
            // Guardar los cambios de forma asíncrona
            await _context.SaveChangesAsync();
            // Retornar la entidad creada
            return personaCreada.Entity;
        }
    }
}