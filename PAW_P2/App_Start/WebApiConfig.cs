using System.Web.Http;
using Newtonsoft.Json.Serialization;

namespace PAW_P2
{
    // Configuración de Web API
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Habilitar atributos de rutas
            config.MapHttpAttributeRoutes();

            // Ruta por defecto de la API
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Configurar JSON como formato por defecto
            var jsonFormatter = config.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            // Remover formato XML
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}
