using Api.Data;
using Api.Models.Entities;
using Api.Repositories.Interfaces;
using Api.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories.Implementations
{
    public class AreaSecuenciaRepository : DapperRepositoryBase, IAreaSecuenciaRepository
    {
        private readonly IAreaSecuenciaRepository _areaSecuenciaRepository;
        private readonly ApplicationDbContext _context;

        public AreaSecuenciaRepository(IConfiguration configuration, IAreaSecuenciaRepository areaSecuenciaRepository, ApplicationDbContext context) : base(configuration)
        {
            _areaSecuenciaRepository = areaSecuenciaRepository;
            _context = context;
        }
        public async Task<uint> GetSiguienteArea(uint areaActual)
        {
            var areaSecuencia = await _context.AreaSecuencia.Where(As => As.Area == areaActual).FirstOrDefaultAsync();
            return areaSecuencia != null ? areaSecuencia.SecuenciaArea : 0;
        }
        }
}
