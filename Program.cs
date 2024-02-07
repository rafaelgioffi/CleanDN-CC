using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CleanDN_CC
{
    public class Program
    {
        static void Main(string[] args)
        {
            string DNBan = ConfigurationSettings.AppSettings["BannedDN"];
            string LogFile = ConfigurationSettings.AppSettings["LogFile"];
            string Folder = ConfigurationSettings.AppSettings["FolderToProcessFiles"];
            string FileToProcess = ConfigurationSettings.AppSettings["FileToProcess"];
            string[] filesInFolder;
            int fileQuantity = 0;

            Log("\n########## Inicio do processamento ##########", true);
            try
            {
                filesInFolder = Directory.GetFiles(Folder, FileToProcess + "*");  //procura os LHDIFs que tiverem na pasta
                fileQuantity = filesInFolder.Length;  //quantidade de arquivos encontrados
            }
            catch (Exception ex)
            {
                Log("\nNenhum arquivo LHDIF encontrado! Ignorando o processamento...\n\n", false);
                return;
            }
            string FileWithoutBanName = "";    //nome do novo arquivo SEM o(s) DN(s) banidos
            string FileWithBanName = "";    //nome do novo arquivo COM o(s) DN(s) banidos

            List<string> FileWithoutBan = new List<string>();
            List<string> FileWithBan = new List<string>();

            if (fileQuantity > 0)
            {
                Log($"Iniciando o processamento de {fileQuantity} arquivos.",false);
                int counter = 1;
                foreach (var file in filesInFolder) //processa arquivo por arquivo encontrado na pasta...
                {
                    Log($"Processando o arquivo {counter}/{fileQuantity}...", false);
                    try
                    {
                        string[] allLines = File.ReadAllLines(file);

                        foreach (var l in allLines) //validação de cada linha...
                        {
                            string DnActual = l.Substring(0, 6);

                            if (DnActual == DNBan)
                            {
                                FileWithBan.Add(l);
                            }
                            else
                            {
                                FileWithoutBan.Add(l);
                            }
                        }

                        if (FileWithBan.Count > 0)  //só executa alguma ação se encontrar algum DN banido...
                        {
                            string[] actual = file.Split('\\');
                            string actualName = actual.Last();
                            actual = actualName.Split('.');
                            actualName = $"{actual[0]}.{actual[1]}";
                            //FileWithBanName = $"{FileToProcess}.D{DateTime.Now.ToString("yyyyMMdd")}.T{DateTime.Now.ToString("HHmmss")}.txt";   //nome do arquivo com ban...
                            FileWithBanName = $"{actualName}.TXT.CB.D{DateTime.Now.ToString("yyyyMMdd")}.T{DateTime.Now.ToString("HHmmss")}";   //nome do arquivo com ban...

                            using (StreamWriter sw = new StreamWriter(Folder + FileWithBanName)) //cria o arquivo somente dos banidos...
                            {
                                foreach (string newLines in FileWithBan)
                                {
                                    sw.WriteLine(newLines);
                                }
                            }
                            Log($"Gerado o arquivo com DNs banidos {FileWithBanName}...",false);

                            Thread.Sleep(3000); //intervalo de 3s por segurança...

                            FileWithoutBanName = $"{actualName}.TXT.SB.D{DateTime.Now.ToString("yyyyMMdd")}.T{DateTime.Now.ToString("HHmmss")}";   //nome do arquivo sem ban...
                            
                            using (StreamWriter sw = new StreamWriter(Folder + FileWithoutBanName)) //cria o arquivo somente sem os banidos...
                            {
                                foreach (string newLines in FileWithoutBan)
                                {
                                    sw.WriteLine(newLines);
                                }
                            }
                            Log($"Gerado o arquivo sem os DNs banidos {FileWithoutBanName}...",false);

                            Thread.Sleep(3000); //intervalo de 3s por segurança...

                            string BkpOriginalFile = $"{actualName}.TXT.OLD.D{DateTime.Now.ToString("yyyyMMdd")}.T{DateTime.Now.ToString("HHmmss")}";
                            File.Move(file,Folder + BkpOriginalFile);    //renomeia o arquivo original para não ser processado...
                            Log($"Arquivo {actualName}.txt renomeado para {BkpOriginalFile}...", false);
                        }
                        else
                        {
                            Log($"Nenhum DN banido encontrado!", false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao processar o arquivo {file}.\n{ex.Message}");
                        Log($"Erro ao processar o arquivo {file}. {ex.Message}", false);
                    }
                    counter++;
                }
            }
            else
            {
                Log($"Nenhum arquivo para processar em {Folder}", false);
                Console.WriteLine($"Nenhum arquivo para processar em {Folder}");
            }
            
            Log("########## Fim do processamento ##########\n", true);

            void Log(string message, bool special)
            {
                using (StreamWriter swLog = new StreamWriter(LogFile, true))
                {
                    if (special)
                    {
                        swLog.WriteLine(message);
                    }
                    else
                    {
                    swLog.WriteLine($"{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} => {message}");
                    }
                }
            }

        }
    }
}
