using System;
using System.IO;
using System.Net;
using RestSharp;
using Newtonsoft.Json;
using System.Threading;
using System.Net.Mail;
//using System.Net.Http;
//using RestSharp.Authenticators;
// Если есть желание, чтобы в первый запуск программы она не отправила письмо 
// запустив старт теста, необходимо: создать файл dockertag.txt по соответствующему 
// расположению notePath. В него следует поместить название текущего тестируемого билда, 
// например: "Docker Tag: 180bb" (без кавычек)


namespace ConsoleApplication4
{
    class Program
    {
        public static void Main()
        {
            Thread trace = new Thread(Trace);
            trace.Start();
        }
        static void Trace()
        {
            while(true)
            {
                try
                {
                    string _DockerID = "transpqasig";
                     string _username = "user";
                    string _password = "pass";
                    string notePath = @"C:\Users\VyacheslavT\Desktop\dockertag.txt"; //расположение всезнающего блокнота
                    string mailSubject = "старт Транспортабле"; //тема письма
                    string noteContentBefore = null; //содержимое всезнающего блокнота
                    //Thread.Sleep(3600000); //час ожидания


                    RestClient client = new RestClient();
                    client.BaseUrl = new Uri("https://hub.docker.com/v2/users/login/");

                    RestRequest loginRequest = new RestRequest(Method.POST);

                    loginRequest.RequestFormat = DataFormat.Json;// ("Content-Type", "application/json");
                    loginRequest.AddBody(new
                    {
                        username = _username,
                        password = _password
                    });

                    //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    IRestResponse tokenResponse;

                    tokenResponse = client.Execute(loginRequest);
                    dynamic tokenDeserializeResponce = JsonConvert.DeserializeObject<dynamic>(tokenResponse.Content); //десериализация токена

                    var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://hub.docker.com/v2/repositories/mirionmeriden/dataanalyst-full/tags/?page_size=1");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "GET";
                    httpWebRequest.Headers.Add("Authorization", "Bearer " + tokenDeserializeResponce.token); //авторизация токеном
                    string lastNameData;
                    using (Stream stream = httpWebRequest.GetResponse().GetResponseStream())
                    {
                        using (StreamReader streamReader = new StreamReader(stream))
                        {
                            var repoListResponse = streamReader.ReadToEnd();
                            dynamic repoListDeserializeResponce = JsonConvert.DeserializeObject<dynamic>(repoListResponse);

                            lastNameData = repoListDeserializeResponce.results[0].last_updated; //нулевое значение массива results, индекс name
                        }
                    }
                    string noteContentAfter = ("Последняя версия была загружена: " + lastNameData); //формирование запроса



                    try
                    {
                        using (StreamReader streamReader = new StreamReader(notePath))
                        {

                            while ((noteContentBefore = streamReader.ReadLine()) != null)
                            {
                                Console.WriteLine("Build Current " + noteContentBefore); //пытаемся считать содержимое существующего всезнающего блокнота
                                //можно добавить вывод лога в посторонний файл
                            }
                        }
                    }
                    catch
                    {
                        using (var streamWriter = new StreamWriter(notePath))
                        {
                            if (noteContentBefore != noteContentAfter) //если содежимое всезнающего блокнота отличается от версии билда с сайта
                            {
                                streamWriter.Write(noteContentAfter); //замена содержимого
                                
                                MailAddress from = new MailAddress("gimmedockertag@gmail.com", "QA");
                                MailAddress to = new MailAddress("testStart@box.ru");
                                MailMessage m = new MailMessage(from, to);
                                m.Subject = mailSubject;
                                m.Body = noteContentAfter;
                                m.IsBodyHtml = true;
                                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                                smtp.Credentials = new NetworkCredential("gimmedockertag@gmail.com", "pass");
                                smtp.EnableSsl = true;
                                smtp.Send(m);
                            }
                        }
                    }
                    Console.WriteLine(lastNameData);
                    Thread.Sleep(5000);
                }
                catch
                {
                    Thread.Sleep(360000); //если что-то по какой-то причине не работает ждем час
                }
            }
        }
    }
}