using AdminUAT.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AdminUAT.Dependencias
{
    public class EnvioCorreo :IEnvioCorreo
    {
        //private BodyBuilder builder = new BodyBuilder();
        private string builder;
        private readonly NewUatDbContext _context;

        public EnvioCorreo(NewUatDbContext context)
        {
            _context = context;
        }

        public bool SendCorreo(string titulo, int paso, long idDenuncia, string path)
        {
            var denunciante = _context.Victima
                .Where(x => x.DenunciaId == idDenuncia && x.Email != "")
                .FirstOrDefault();

            MailMessage email = new MailMessage();

            var emailAddress = denunciante.Email;

            email.To.Add(new MailAddress(denunciante.Email));
            email.Subject = titulo;


            string name = denunciante.Nombre + " " + denunciante.PrimerApellido + " " + denunciante.SegundoApellido;
            if (paso == 1) { ConstruirBody(name, idDenuncia, path); }
            else if (paso == 3) { ConstruirBodyMailFinal(name, idDenuncia, path); }
            email.Body = builder;
            email.IsBodyHtml = true;
            email.Priority = MailPriority.Normal;
            SmtpClient smtp = new SmtpClient();

            try
            {
                /*
                email.From = new MailAddress("uat.fiscalia.puebla.3@outlook.com");
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("uat.fiscalia.puebla.3@outlook.com", "Fge.2020**", "outlook.com");
                smtp.Host = "smtp-mail.outlook.com";
                smtp.TargetName = "STARTTLS/smtp-mail.outlook.com";
                smtp.Port = 25;
                smtp.EnableSsl = true;
                */
                email.From = new MailAddress("uat.fiscalia.puebla.7@gmail.com");
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("uat.fiscalia.puebla.7@gmail.com", "Fge.2020**");

                smtp.Send(email);
                email.Dispose();
            }
            catch (Exception ex)
            {
                /*
                email.From = new MailAddress("uat.fiscalia.puebla.1@outlook.com");
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("uat.fiscalia.puebla.1@outlook.com", "Fge.2020**", "outlook.com");
                smtp.Host = "smtp-mail.outlook.com";
                smtp.TargetName = "STARTTLS/smtp-mail.outlook.com";
                smtp.Port = 25;
                smtp.EnableSsl = true;
                */
                email.From = new MailAddress("uat.fiscalia.puebla.8@gmail.com");
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("uat.fiscalia.puebla.8@gmail.com", "Fge.2020**");

                smtp.Send(email);
                email.Dispose();
            }

            return true;
        }

        public void ConstruirBody(string path)
        {
            /*builder.HtmlBody*/
            builder = string.Format(
@"<p>Favor de verificar a que kiosco pertenece la ip {0} 
                ya que no se encuentra registrada el la DB.</p>", path);
        }

        public void ConstruirBody(string nombre, long idDenuncia, string path)
        {
            string token = GeneraCodigo(idDenuncia);
            var otherPath= "http://10.24.1.36:8099/";
            /*builder.HtmlBody*/
            builder = string.Format(
@"<center><b>Fiscalía General del Estado de Puebla</b></center>
                <p>Estimado <b>{0}:</b></p>
                <p>Con la recepción de este mensaje estamos validado tu correo electrónico, mismo que proporcionaste al iniciar tu denuncia con el número: </p>
                <p><b>{1}</b></p>
                <p>No olvides conservar este número en un lugar seguro, lo puedes necesitar posteriormente para la consulta de tu denuncia. </p>
                <p>La presentación de tu denuncia consta de tres pasos: 1) Datos, 2) Lugar, 3) Delito. Hasta el momento sólo has realizado el primero, lo cual significa que tu denuncia aún está inconclusa y por tal motivo, no ha sido recibida en la agencia del Ministerio Público que corresponde.</p>
                <p>Para que puedas continuar con el llenado de tu denuncia y concluirla, requerimos que nos confirmes la recepción del presente mensaje y que aceptas conocer tu folio. Para realizarlo, es necesario que utilices el siguiente vínculo haciendo clic sobre el mismo o copiarlo a la barra de dirección de tu navegador de internet: </p>
                <p>{2}valida-email/{3}/{4}</p>
                <p>Si no funciona el acceso proporcionado anteriormente por favor ingrese el siguiente:</p>
                <p>{5}valida-email/{3}/{4}</p>
                <p>Una vez que aparezca el mensaje de confirmación, podrás continuar con tu denuncia.</p>
                <p>Estamos para servirte en <b>Fiscalínea: 2-11-7900 extensión 2036:</b></p>
                <p>
                Te recordamos nuestro domicilio:<br/>
                Boulevard “Héroes del 5 de Mayo” esquina con Avenida 31 Oriente<br/>
                Col. Ladrillera de Benítez, C.P. 72530<br/>
                Puebla, Pue. MX.<br/>
                </p>", nombre, idDenuncia, path, idDenuncia, token, otherPath);
        }

        public void ConstruirBodyMailFinal(string nombre, long idDenuncia, string path)
        {
            var denuncia = _context.Denuncia.Find(idDenuncia);
            /*builder.HtmlBody*/
            builder = string.Format(
@"<center><b>Fiscalía General del Estado de Puebla</b></center>
                <p>Estimado <b>{0}:</b></p>
                <p>¡Has concluido con la presentación de tu denuncia!</p>
                <p>Folio: <b>{1}</b></p>
                <p>Para atender e informar sobre el trámite de su denuncia se asignó al {2}</p>
                
                <p>Recuerde llevar consigo su <b>credencial de elector y comprobante de domicilio</b> al momento de presentarse con el Agente de Ministerio Público asignado.</p>                

                <p>En caso que tengas queja de la atención recibida, envíanos un correo electrónico a la siguiente dirección: quejas_uat@fiscalia.puebla.gob.mx.</p>

                <p><center><b>AVISO DE PROTECCIÓN DE DATOS PERSONALES</b></center></p>
                <p>Los datos personales que nos has proporcionado serán utilizados con confidencialidad y se almacenarán con medidas de seguridad que garanticen su protección, conforme a la Ley de Protección de Datos Personales en Posesión de los Sujetos Obligados del Estado de Puebla, la Ley General de Transparencia y Acceso a la Información Pública, así como la Ley de Transparencia y Acceso a la información Pública del Estado de Puebla. Ninguna persona que no esté autorizada podrá conocer tus datos personales.</p>
                <p>En cumplimiento a lo dispuesto en los artículos 1, 7, 11, 15, 20, 37 y demás aplicables de la Ley de Protección de Datos Personales en Posesión de los Sujetos Obligados del Estado de Puebla y 3 fracción IV de las Políticas y Lineamientos de Observancia General para el Manejo, Tratamiento, Seguridad y Protección de Datos Personales del Estado emitido por la Comisión para el Acceso a la Información Pública y Protección de Datos Personales del Estado.</p>                
                <p>Para consultar el Aviso de Protección de Datos Personales en su versión completa, te invitamos a consultar nuestro sitio web en www.fiscalia.puebla.gob.mx</p>

                <p>Estamos para servirte en <b>Fiscalínea: 2-11-7900 extensión 2036:</b></p>

                <p>
                También estamos presentes en redes sociales:<br/>
                www.facebook.com/fiscaliapuebla<br/>
                www.twitter.com/FiscaliaPuebla<br/>
                </p>", nombre, denuncia.Expediente, path);
        }

        public string GeneraCodigo(long idDenuncia)
        {
            var a = _context.Denuncia.Find(idDenuncia);
            string nombre = String.Format("{0:s}", a.AltaSistema).ToString();
            nombre = nombre.Replace(":", "");
            nombre = nombre.Replace("-", "");
            nombre = nombre.Replace("T", "");
            return nombre;
        }
    }
}
