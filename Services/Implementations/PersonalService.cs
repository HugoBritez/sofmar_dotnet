using Api.Models.Dtos;
using Api.Repositories.Interfaces;
using Api.Services.Interfaces;

namespace Api.Services.Implementations
{
    public class PersonalService : IPersonalService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IProveedoresRepository _proveedoresRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPersonaRepository _personaRepository;

        public PersonalService(
            IClienteRepository clienteRepository,
            IProveedoresRepository proveedoresRepository,
            IUsuarioRepository usuarioRepository,
            IPersonaRepository personaRepository
        )
        {
            _clienteRepository = clienteRepository;
            _proveedoresRepository = proveedoresRepository;
            _usuarioRepository = usuarioRepository;
            _personaRepository = personaRepository;
        }


        public async Task<bool?> CrearPersona(CrearPersonaDTO data)
        {
            bool? result = null;
            
            if (data == null)
            {
                Console.WriteLine("No se recibieron datos para crear la persona.");
                return result;
            }
            if (data.persona == null)
            {
                Console.WriteLine("No se proporcionó información de la persona.");
                return result;
            }

            await _personaRepository.CreateAsync(data.persona); // primero se crea la entidad padre 

            foreach (int tipo in data.Tipo.Distinct()) // Evita procesar tipos repetidos
            {
                switch (tipo)
                {
                    case 0 when data.Cliente != null:
                        Console.WriteLine("Creando cliente...");
                        await _clienteRepository.CrearCliente(data.Cliente);
                        result = true;
                        break;
                    case 1 when data.Proveedor != null:
                        Console.WriteLine("Creando proveedor...");
                        await _proveedoresRepository.CrearProveedor(data.Proveedor);
                        result = true;
                        break;
                    case 2 when data.Operador != null:
                        Console.WriteLine("Creando operador...");
                        await _usuarioRepository.CrearOperador(data.Operador);
                        result = false;
                        break;
                }
            }
            return result;
        }
    }
}