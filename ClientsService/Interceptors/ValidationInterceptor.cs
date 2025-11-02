using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using FluentValidation;
namespace ClientsService.Interceptors
{
    public class ValidationInterceptor : Interceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
    TRequest request,
    ServerCallContext context,
    UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                // 1. Intenta ejecutar el método de servicio (ej. CreateClient, GetClientById)
                return await continuation(request, context);
            }
            catch (ValidationException ex)
            {
                // 2. ¡ATRAPADO! Si es un error de FluentValidation
                var errorMessages = string.Join("\n", ex.Errors.Select(e => e.ErrorMessage));

                // Devolvemos un error gRPC CLARO
                throw new RpcException(new Status(StatusCode.InvalidArgument, errorMessages));
            }
            catch (FormatException ex) 
            {

                var errorMessage = $"Invalid format in request. {ex.Message}";


                throw new RpcException(new Status(StatusCode.InvalidArgument, errorMessage));
            }
        }
    }
}