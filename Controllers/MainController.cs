﻿using System.Linq;
using CadastroDeFornecedoresApi.Notificacoes;
using CadastroDeFornecedoresApi.Notificacoes.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CadastroDeFornecedoresApi.Controllers
{
    [ApiController]
    public abstract class MainController : ControllerBase
    {

        private readonly INotificador _notificador;
        

        public MainController(INotificador notificador)
        {
            _notificador = notificador;
        }

        protected bool OperacaoValida()
        {
            return !_notificador.TemNotificacão();
        }

        protected ActionResult CustomResponse(object result = null)
        {
            if(OperacaoValida()) return Ok( new{
                success = true,
                data = result
            });

            return BadRequest(new{
                success = false,
                errors = _notificador.ObterNotificacoes().Select(n => n.Mensagem)
            });
        }

        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            if (!modelState.IsValid) NotificarErroModelInvalida(modelState);
            return CustomResponse();
        }



        protected void NotificarErroModelInvalida(ModelStateDictionary modelState)
        {
            var erros = modelState.Values.SelectMany(e => e.Errors);
            foreach (var erro in erros)
            {
                var errorMessage = erro.Exception == null ? erro.ErrorMessage : erro.Exception.Message;
                NotificarErro(errorMessage);
            }
        }

        protected void NotificarErro(string errorMessage)
        {
            _notificador.Handle(new Notificacao(errorMessage));
        }
    }
}
