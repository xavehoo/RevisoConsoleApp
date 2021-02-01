using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace RevisoConsoleApp
{
    class Program
    {
        private static string _appSecretToken = "";
        private static string _agreementGrantToken = "";
        private static string _jsonDocumentStringList = "";

        public enum RevisoObjectType
        {
            ORDINE,
            PREVENTIVO,
            FATTURA
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Console.WriteLine("Inserisci AppSecretToken:");
            _appSecretToken = Console.ReadLine();
            Console.WriteLine("Inserisci AgreementGrantToken:");
            _agreementGrantToken = Console.ReadLine();

            // Stampa istruzioni
            Console.WriteLine("Istruzioni:" +
                                            "\r\n\t- Premi 1 per aggiornare codice iva ordini" +
                                            "\r\n\t- Premi 2 per aggiornare codice iva preventivi" +
                                            "\r\n\t- Premi 9 per uscire.");

            ConsoleKeyInfo pressedKey;

            while (true)
            {

                Console.WriteLine("\r\nPremi un pulsante:");

                pressedKey = Console.ReadKey();
                switch (pressedKey.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        UpdateFieldDocument(RevisoObjectType.ORDINE);
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        UpdateFieldDocument(RevisoObjectType. PREVENTIVO);
                        break;
                    case ConsoleKey.D9:
                    case ConsoleKey.NumPad9:
                        Console.WriteLine("Bye World!!");
                        return;
                    default:
                        break;
                }
            }
        }
        
        private static void UpdateFieldDocument(RevisoObjectType docType)
        {
            JObject response = new JObject();
            if ((response = GetDocumentList(docType)) != null)
            {
                _jsonDocumentStringList = "";
                RevisoCollection docList= JsonConvert.DeserializeObject<RevisoCollection>(response.ToString());
                int docId = -1;
                foreach (JObject jObject in docList.Collection)
                {
                    JToken jToken = jObject.SelectToken("id");
                    if (Int32.TryParse(jToken.ToString(), out docId))
                    {
                        //if (docId.ToString() == "3728") continue;
                        if ((response = GetDocument(docId, docType)) != null)
                        {
                            if (UpdateDocument(docId, docType, response) != null)
                            {
                                Console.WriteLine($"{docType} {docId} correttamente aggiornato.");
                            }
                            else
                            {
                                Console.WriteLine($"Errore durante l'update di {docType} {docId}.");
                                return;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Errore durante la get di {docType} {docId}.");
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Errore durante il calcolo dell'id di {docType}: \r\n{jObject}");
                        return;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Errore durante la get della lista {docType}.");
                return;
            }
        }
        
        /*
        private static void UpdateOrderList(JObject response)
        {

            string jsonString = orderList.ToString();
            jsonString = jsonString.Replace("\"V22\"", "\"V022\"");

            RevisoCollection orderCollection = JsonConvert.DeserializeObject<RevisoCollection>(orderList.ToString());

            int orderId = -1;

            foreach (JObject jObject in orderCollection.Collection)
            {
                JToken jToken = jObject.SelectToken("id");
                if (Int32.TryParse(jToken.ToString(), out orderId))
                {
                    UpdateOrder(orderId);
                }
            }
        }
        */

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static JObject GetDocumentList(RevisoObjectType docType)
        {
            Console.WriteLine($"\r\nRecupero lista {docType} in corso...");
            JObject docList = null;
            switch (docType)
            {
                case RevisoObjectType.ORDINE:
                    docList = RevisoGetObjects(RevisoObjectType.ORDINE);
                    break;
                case RevisoObjectType.PREVENTIVO:
                    docList = RevisoGetObjects(RevisoObjectType.PREVENTIVO);
                    break;
                default:
                    break;
            }

            RevisoResponseError responseError = JsonConvert.DeserializeObject<RevisoResponseError>(docList.ToString());

            if (docList != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(responseError.errorCode))
                    {
                        Console.WriteLine($"\r\nERRORE GET ALL:\r\n{responseError}");
                        return null;
                    }

                    return docList;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERRORE: {ex?.Message}");
                }
            }
            return null;
        }

/// <summary>
/// 
/// </summary>
/// <param name="docId"></param>
/// <param name="docType"></param>
/// <returns></returns>
        private static JObject GetDocument(int docId, RevisoObjectType docType)
        {
            Console.WriteLine($"\r\nRecupero {docType} '{docId}' in corso...");
            JObject doc = null;
            switch (docType)
            {
                case RevisoObjectType.ORDINE:
                    doc = RevisoGetObject(docId, RevisoObjectType.ORDINE);
                    break;
                case RevisoObjectType.PREVENTIVO:
                    doc = RevisoGetObject(docId, RevisoObjectType.PREVENTIVO);
                    break;
                default:
                    break;
            }

            RevisoResponseError responseError = JsonConvert.DeserializeObject<RevisoResponseError>(doc.ToString());

            if (doc != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(responseError.errorCode))
                    {
                        Console.WriteLine($"\r\nERRORE GET {docType}:\r\n{responseError}");
                        return null;
                    }
                    _jsonDocumentStringList += $"|||BEFORE:::{doc}";
                    return doc;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERRORE: {ex?.Message}");
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="docId"></param>
        /// <param name="docType"></param>
        /// <param name="jObjectOld"></param>
        /// <returns></returns>
        private static JObject UpdateDocument(int docId, RevisoObjectType docType, JObject jObjectOld)
        {
            Console.WriteLine($"\r\nAggiornamento {docType} '{docId}' in corso...");

            string jsonString = jObjectOld.ToString();
            jsonString = jsonString.Replace("\"vatCode\": \"V22\"", "\"vatCode\": \"V022\"");
            jsonString = jsonString.Replace("\"https://rest.reviso.com/vat-accounts/V22\"", "\"https://rest.reviso.com/vat-accounts/V022\"");
            jsonString = jsonString.Replace('\'', ' ').Replace('&', 'e').Replace('€', 'E');

            JObject jObjectNew = JsonConvert.DeserializeObject<JObject>(jsonString);

            _jsonDocumentStringList += $"|||AFTER:::{jObjectNew}";

            byte[] bytes = Encoding.UTF8.GetBytes(jObjectNew.ToString());
            int lenghtBody = bytes.Length;

            JObject doc = RevisoUpdateObject(jsonString, bytes, lenghtBody, docId, docType);
            RevisoResponseError responseError = JsonConvert.DeserializeObject<RevisoResponseError>(doc.ToString());

            if (doc != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(responseError.errorCode))
                    {
                        Console.WriteLine($"\r\nERRORE GET {docType}:\r\n{responseError}");
                        return null;
                    }

                    return doc;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERRORE: {ex?.Message}");
                }
            }
            return null;
        }



        #region API

        /// <summary>
        /// 
        /// </summary>
        /// <param name="revisoObjectType"></param>
        /// <returns></returns>
        private static JObject RevisoGetObjects(RevisoObjectType revisoObjectType)
        {
            var client = new HttpClient();

            HttpWebRequest request = null;

            switch (revisoObjectType)
            {
                case RevisoObjectType.ORDINE:
                    request = (HttpWebRequest)WebRequest.Create("https://rest.reviso.com/orders?filter=isArchived$eq:false$and:isSent$eq:false&pagesize=10000");
                    break;
                case RevisoObjectType.PREVENTIVO:
                    request = (HttpWebRequest)WebRequest.Create("https://rest.reviso.com/quotations?filter=isArchived$eq:false&pagesize=10000");
                    break;
                default:
                    return null;
            }

            request.Headers["X-AppSecretToken"] = _appSecretToken;
            request.Headers["X-AgreementGrantToken"] = _agreementGrantToken;
            request.ContentType = "application/json";

            HttpStatusCode code;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    code = response.StatusCode;
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        var jsonObj = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                        return jsonObj;
                    }
                }
            }
            catch (WebException webEx)
            {
                using (HttpWebResponse response = (HttpWebResponse)webEx.Response)
                {
                    if (response == null) 
                    { 
                        var jsonObj = JsonConvert.DeserializeObject<JObject>("{\"message\":\"" + webEx.Message + "\"}");
                        return jsonObj;
                    }
                    code = response.StatusCode;
                    if (code != HttpStatusCode.BadRequest) 
                    { 
                        var jsonObj = JsonConvert.DeserializeObject<JObject>("{\"message\":\"" + webEx.Message + "\"}");
                        return jsonObj;
                    }
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        var jsonObj = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                        return jsonObj;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="revisoObjectType"></param>
        /// <returns></returns>
        private static JObject RevisoGetObject(int id, RevisoObjectType revisoObjectType)
        {
            var client = new HttpClient();

            HttpWebRequest request = null;

            switch (revisoObjectType)
            {
                case RevisoObjectType.ORDINE:
                    request = (HttpWebRequest)WebRequest.Create($"https://rest.reviso.com/orders/{id}");
                    break;
                case RevisoObjectType.PREVENTIVO:
                    request = (HttpWebRequest)WebRequest.Create($"https://rest.reviso.com/quotations/{id}");
                    break;
                default:
                    return null;
            }

            request.Headers["X-AppSecretToken"] = _appSecretToken;
            request.Headers["X-AgreementGrantToken"] = _agreementGrantToken;
            request.ContentType = "application/json";

            HttpStatusCode code;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    code = response.StatusCode;
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        var jsonObj = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                        return jsonObj;
                    }
                }
            }
            catch (WebException webEx)
            {
                using (HttpWebResponse response = (HttpWebResponse)webEx.Response)
                {
                    if (response == null)
                    {
                        var jsonObj = JsonConvert.DeserializeObject<JObject>("{\"message\":\"" + webEx.Message + "\"}");
                        return jsonObj;
                    }
                    code = response.StatusCode;
                    if (code != HttpStatusCode.BadRequest)
                    {
                        var jsonObj = JsonConvert.DeserializeObject<JObject>("{\"message\":\"" + webEx.Message + "\"}");
                        return jsonObj;
                    }
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        var jsonObj = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                        return jsonObj;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updatestring"></param>
        /// <param name="bytes"></param>
        /// <param name="lenght"></param>
        /// <param name="existOnReviso"></param>
        /// <returns></returns>
        private static JObject RevisoUpdateObject(string updatestring, byte[] bytes, int lenght, int id, RevisoObjectType revisoObjectType)
        {


            var client = new HttpClient();

            HttpWebRequest request = null;

            switch (revisoObjectType)
            {
                case RevisoObjectType.ORDINE:
                    request = (HttpWebRequest)WebRequest.Create($"https://rest.reviso.com/orders/{id}");
                    break;
                case RevisoObjectType.PREVENTIVO:
                    request = (HttpWebRequest)WebRequest.Create($"https://rest.reviso.com/quotations/{id}");
                    break;
                default:
                    return null;
            }

            request.Method = "PUT";

            request.Headers["X-AppSecretToken"] = _appSecretToken;
            request.Headers["X-AgreementGrantToken"] = _agreementGrantToken;
            request.ContentType = "application/json";
            request.ContentLength = lenght;

            HttpStatusCode code;
            try
            {
                Stream newStream = request.GetRequestStream();
                newStream.Write(bytes, 0, bytes.Length);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    code = response.StatusCode;
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        var jsonObj = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                        return jsonObj;
                    }
                }
            }
            catch (WebException webEx)
            {
                using (HttpWebResponse response = (HttpWebResponse)webEx.Response)
                {
                    if (response == null)
                    {
                        var jsonObj = JsonConvert.DeserializeObject<JObject>("{\"message\":\"" + webEx.Message + "\"}");
                        return jsonObj;
                    }
                    code = response.StatusCode;
                    if (code != HttpStatusCode.BadRequest && code != HttpStatusCode.NotFound)
                    {
                        var jsonObj = JsonConvert.DeserializeObject<JObject>("{\"message\":\"" + webEx.Message + "\"}");
                        return jsonObj;
                    }
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        var jsonObj = JsonConvert.DeserializeObject<JObject>(reader.ReadToEnd());
                        return jsonObj;
                    }
                }
            }
        }

        #endregion

    }

    public class RevisoCollection
    {
        public List<JObject> Collection { get; set; }
    }

    public class RevisoResponseError
    {
        public string developerHint { get; set; }
        public string errorCode { get; set; }
        public List<RevisoResponseErrorDetail> errors { get; set; }
        public int httpStatusCode { get; set; }
        public string logId { get; set; }
        public DateTime logTime { get; set; }
        public string message { get; set; }
        public RDetails details { get; set; }
    }

    public class RevisoResponseErrorDetail
    {
        public string developerHint { get; set; }
        public string errorCode { get; set; }
        public DateTime logTime { get; set; }
        public string message { get; set; }
    }

    public class RDetails
    {
        public List<object> entity { get; set; }
    }

}
