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

        public PersonalService(
            IClienteRepository clienteRepository,
            IProveedoresRepository proveedoresRepository,
            IUsuarioRepository usuarioRepository
        )
        {
            _clienteRepository = clienteRepository;
            _proveedoresRepository = proveedoresRepository;
            _usuarioRepository = usuarioRepository;
        }


        public async Task<bool?> CrearPersona(CrearPersonaDTO data)
        {
            bool? result = null;

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