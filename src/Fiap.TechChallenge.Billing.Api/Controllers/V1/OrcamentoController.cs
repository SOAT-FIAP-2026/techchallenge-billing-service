using Fiap.TechChallenge.Billing.Application.DTOs.Orcamento;
using Fiap.TechChallenge.Billing.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.TechChallenge.Billing.Api.Controllers.V1
{
    [ApiController]
    [Route("api/v1/orcamentos")]
    [Produces("application/json")]
    public class OrcamentoController : ControllerBase
    {
        private readonly IOrcamentoAppService _orcamentoAppService;
        private readonly ILogger<OrcamentoController> _logger;

        public OrcamentoController(IOrcamentoAppService orcamentoAppService, ILogger<OrcamentoController> logger)
        {
            _orcamentoAppService = orcamentoAppService ?? throw new ArgumentNullException(nameof(orcamentoAppService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Criação/cálculo de orçamento para uma Ordem de Serviço.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(OrcamentoResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CriarOrcamento([FromBody] CriarOrcamentoDto dto, CancellationToken cancellationToken)
        {
            var orcamento = await _orcamentoAppService.CriarAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(ObterPorId), new { id = orcamento.Id }, orcamento);
        }

        /// <summary>
        /// Busca orçamento por ID com histórico de itens e status.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OrcamentoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
        {
            var orcamento = await _orcamentoAppService.ObterPorIdAsync(id, cancellationToken);
            if (orcamento == null)
                return NotFound(new { error = $"Orçamento com ID '{id}' não encontrado." });

            return Ok(orcamento);
        }

        /// <summary>
        /// Consulta orçamento vinculado a uma Ordem de Serviço específica.
        /// </summary>
        [HttpGet("os/{osId:guid}")]
        [ProducesResponseType(typeof(OrcamentoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorOrdemServicoId(Guid osId, CancellationToken cancellationToken)
        {
            var orcamento = await _orcamentoAppService.ObterPorOrdemServicoIdAsync(osId, cancellationToken);
            if (orcamento == null)
                return NotFound(new { error = $"Nenhum orçamento encontrado para a Ordem de Serviço '{osId}'." });

            return Ok(orcamento);
        }

        /// <summary>
        /// Aprova o orçamento e o deixa pronto para pagamento.
        /// </summary>
        [HttpPatch("{id:guid}/aprovar")]
        [ProducesResponseType(typeof(OrcamentoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AprovarOrcamento(Guid id, CancellationToken cancellationToken)
        {
            var orcamento = await _orcamentoAppService.AprovarAsync(id, cancellationToken);
            return Ok(orcamento);
        }

        /// <summary>
        /// Cancela ou rejeita o orçamento.
        /// </summary>
        [HttpPatch("{id:guid}/cancelar")]
        [ProducesResponseType(typeof(OrcamentoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelarOrcamento(Guid id, [FromQuery] string? motivo, CancellationToken cancellationToken)
        {
            var orcamento = await _orcamentoAppService.CancelarAsync(id, motivo, cancellationToken);
            return Ok(orcamento);
        }
    }
}
