using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using webapi.event_.Domains;
using webapi.event_.Interfaces;
using webapi.event_.Repositories;

namespace webapi.event_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ComentariosEventoController : ControllerBase
    {
        private IComentariosEventoRepository _comentariosEventoRepository { get; set; }


        //armazena os dados do serviço da API Externa(IA - Azure)
        private readonly ContentModeratorClient _contentModeratorClient;


        /// <summary>
        /// Construtor que recebe os dados necessários para acesso ao serviço externo
        /// </summary>
        /// <param name="contentModeratorClient">Objeto do tipo ContentModeratorClient</param>
        public ComentariosEventoController(ContentModeratorClient contentModeratorClient)
        {
            _comentariosEventoRepository = new ComentariosEventoRepository();

            _contentModeratorClient = contentModeratorClient;
        }

        [HttpPost("Comentario")]
        public async Task<IActionResult> PostIA(ComentariosEvento novoComentario)
        {
            try
            {
                if ((novoComentario.Descricao).IsNullOrEmpty())
                {
                    return BadRequest("A descrição do comentário não pode estar vazio! ou nulo!");
                }

                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(novoComentario.Descricao));

                var moderationResult = await _contentModeratorClient.TextModeration
                    .ScreenTextAsync("text/plain", stream, "por", false, false, null, true);

                if(moderationResult.Terms != null)
                {
                    novoComentario.Exibe = false;

                    _comentariosEventoRepository.Cadastrar(novoComentario);
                }
                else
                {
                    novoComentario.Exibe = true;

                    _comentariosEventoRepository.Cadastrar(novoComentario);
                }
                return StatusCode(201, _comentariosEventoRepository);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("ListarSomenteExibe/{id}")]
        public IActionResult GetTrue(Guid id)
        {
            try
            {
                return Ok(_comentariosEventoRepository.ListarSomenteExibe(id));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("ListarTodos/{id}")]
        public IActionResult Get(Guid id)
        {
            try
            {
                return Ok(_comentariosEventoRepository.Listar(id));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("BuscarPorIdUsuario")]
        public IActionResult Get(Guid idUsuario, Guid idEvento)
        {
            try
            {
                return Ok(_comentariosEventoRepository.BuscarPorIdUsuario(idUsuario, idEvento));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public IActionResult Post(ComentariosEvento novoComentario)
        {
            try
            {
                _comentariosEventoRepository.Cadastrar(novoComentario);

                return StatusCode(201, novoComentario);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            try
            {
                _comentariosEventoRepository.Deletar(id);

                return StatusCode(204);
            }
            catch (Exception e)
            {

                return BadRequest(e.Message);
            }
        }
       
    }
}
