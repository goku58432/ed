using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EduAPI.Services
{
    public class CertificadoService
    {
        public byte[] GenerarCertificado(string nombreUsuario, string nombreCurso, string nombreProfesor, DateTime fechaCompletado)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(0);
                    page.DefaultTextStyle(x => x.FontFamily("Arial"));

                    page.Content().Background("#121212").Padding(40).Column(col =>
                    {
                        col.Spacing(0);

                        // Borde decorativo
                        col.Item().Border(4).BorderColor("#e63946").Padding(30).Column(inner =>
                        {
                            inner.Spacing(16);

                            // Header
                            inner.Item().AlignCenter().Text("🎓 EduPlatform")
                                .FontSize(14).FontColor("#e63946").Bold();

                            inner.Item().AlignCenter().Text("CERTIFICADO DE FINALIZACIÓN")
                                .FontSize(28).FontColor("#FFFFFF").Bold();

                            inner.Item().AlignCenter().Text("Este certificado acredita que")
                                .FontSize(14).FontColor("#AAAAAA");

                            // Nombre del usuario
                            inner.Item().AlignCenter().Text(nombreUsuario)
                                .FontSize(36).FontColor("#e63946").Bold();

                            inner.Item().AlignCenter().Text("ha completado satisfactoriamente el curso")
                                .FontSize(14).FontColor("#AAAAAA");

                            // Nombre del curso
                            inner.Item().AlignCenter().Text(nombreCurso)
                                .FontSize(26).FontColor("#FFFFFF").Bold();

                            inner.Item().AlignCenter().Text($"Impartido por: {nombreProfesor}")
                                .FontSize(13).FontColor("#BBBBBB");

                            inner.Item().AlignCenter().Text($"Fecha: {fechaCompletado:dd/MM/yyyy}")
                                .FontSize(12).FontColor("#888888");

                            // Separador
                            inner.Item().PaddingTop(10).LineHorizontal(1).LineColor("#333333");

                            inner.Item().AlignCenter().Text("Plataforma de Educación en Línea")
                                .FontSize(11).FontColor("#666666");
                        });
                    });
                });
            });

            return doc.GeneratePdf();
        }
    }
}
