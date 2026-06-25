using System.ComponentModel;
using Microsoft.SemanticKernel;

public class TrainingPlugin
{
    [KernelFunction]
    [Description(
    "Sistemde kayıtlı kullanıcının alması gereken eğitim listesini getirir.")]
    public string GetRequiredTrainings()
    {
        Console.WriteLine(">>> GetRequiredTrainings çalıştı");
        Console.WriteLine(
            $"{DateTime.Now:HH:mm:ss} GetRequiredTrainings");
        return """
        Yangın Eğitimi
        İlk Yardım Eğitimi
        Yüksekte Çalışma Eğitimi
        """;
    }

    [KernelFunction]
    [Description("Kullanıcının başarısız olduğu sınavları getirir.")]
    public string GetFailedExams()
    {
        Console.WriteLine(">>> GetFailedExams çalıştı");
        Console.WriteLine(
            $"{DateTime.Now:HH:mm:ss} GetFailedExams");
        return """
        Elektrik Güvenliği Sınavı
        Yüksekte Çalışma Sınavı
        """;
    }

    [KernelFunction]
    [Description("Belirli bir çalışanın başarısız olduğu sınavları getirir.")]
    public string GetFailedExamsByEmployee(
        [Description("Çalışanın tam adı")] string employeeName)
    {
        Console.WriteLine(
            $">>> employeeName = {employeeName}");

        return $"""
        {employeeName} adlı çalışanın başarısız olduğu sınavlar:

        - Elektrik Güvenliği Sınavı
        - Yüksekte Çalışma Sınavı
        """;
    }

    [KernelFunction]
    [Description("Belirli bir çalışanın sınav sonuçlarını getirir.")]
    public string GetExamResults(
        [Description("Çalışanın tam adı")] string employeeName,
        [Description("Sınav yılı")] int year,
        [Description("Geçilen sınavlar da dahil edilsin mi")] bool includePassedExams)
    {
        Console.WriteLine(
            $">>> employeeName={employeeName}");
        Console.WriteLine(
            $">>> year={year}");
        Console.WriteLine(
            $">>> includePassedExams={includePassedExams}");

        return "Test";
    }
}