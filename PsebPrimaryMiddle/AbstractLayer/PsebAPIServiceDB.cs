using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using PsebJunior.Models;
using System.IO;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;
using System.Collections;
using System.Data.Odbc;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

namespace PsebJunior.AbstractLayer
{
    public class PsebAPIServiceDB : IDisposable
    {
       
        //private readonly static HttpClient _httpClient = new HttpClient();

        private static HttpClient _httpClient = null;
        private static readonly object threadlock = new object();


        public static async Task<string> UpdateUSIPSEBJuniorToPSEBMain(SchoolModels model)
        {
            string status = "0";
            try
            {             
                lock (threadlock)
                {
                    if (_httpClient == null)
                    {
                        _httpClient = new HttpClient();
                       // _httpClient.BaseAddress = new Uri("https://localhost:57470/api/");
                        _httpClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["PSEBAPI"]);
                    }
                }

               // _httpClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["PSEBAPI"]);
                var putTask = await _httpClient.PutAsJsonAsync<SchoolModels>("School/UpdateUSIPSEBJuniorToPSEBMain/@pSeb4395m/" + model.SCHL, model);
                var result = putTask;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<SchoolApiViewModel>();
                    readTask.Wait();

                    if (readTask.Result.statusCode == "200")
                    {
                        model = readTask.Result.Object;
                        status = "200";
                    }
                    else
                    {
                        model = readTask.Result.Object;
                        status = readTask.Result.statusCode;
                    }
                }
                else
                {
                    model = null;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }            
            return status;
        }



        public static async Task<string> SchoolChangePasswordPSEBJunior(SchoolChangePasswordModel model)
        {
            string status = "0";
            try
            {

               // string url = "https://localhost:57470/api/";
                lock (threadlock)
                {
                    if (_httpClient == null)
                    {
                        _httpClient = new HttpClient();
                        // _httpClient.BaseAddress = new Uri(url);
                        _httpClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["PSEBAPI"]);
                    }
                }
                var putTask = await _httpClient.PutAsJsonAsync<SchoolChangePasswordModel>("School/SchoolChangePasswordPSEBJunior/@pSeb4395m/" + model.SCHL, model);
                var result = putTask;
                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsAsync<SchoolPasswordAPIViewModel>();
                    readTask.Wait();

                    if (readTask.Result.statusCode == "200")
                    {
                        model = readTask.Result.Object;
                        status = "200";
                    }
                    else
                    {
                        model = readTask.Result.Object;
                        status = readTask.Result.statusCode;
                    }
                }
                else
                {
                    model = null;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return status;
        }



        //public string UpdateUSIPSEBJuniorToPSEBMain(SchoolModels model)
        //{
        //    try
        //    {

        //        string status = "0";
        //        using (var client = new HttpClient())
        //        {
        //            //https://api.psebonline.in/api/School/GetSchoolDataBySchlPSEBMain/@pSeb4395m/0010026

        //            //string url = "https://localhost:57470/api/";
        //          //  client.BaseAddress = new Uri(url);

        //            //<add key="PSEBAPI" value="https://api.psebonline.in/api/" />
        //            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["PSEBAPI"]);


        //            //HTTP POST
        //            var putTask = client.PutAsJsonAsync<SchoolModels>("School/UpdateUSIPSEBJuniorToPSEBMain/@pSeb4395m/" + model.SCHL, model);
        //            // var putTask = client.PutAsJsonAsync<SchoolModels>("value", model);

        //            putTask.Wait();

        //            var result = putTask.Result;
        //            if (result.IsSuccessStatusCode)
        //            {
        //                var readTask = result.Content.ReadAsAsync<SchoolApiViewModel>();
        //                readTask.Wait();

        //                if (readTask.Result.statusCode == "200")
        //                {
        //                    model = readTask.Result.Object;
        //                    status = "200";
        //                }
        //                else
        //                {
        //                    model = readTask.Result.Object;
        //                    status = readTask.Result.statusCode;
        //                }
        //            }
        //            else
        //            {
        //                model = null;
        //            }
        //        }
        //        return status;
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.Message;
        //    }

        //}


        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_httpClient != null)
                {
                    _httpClient.Dispose();
                }
            }
        }

        ~PsebAPIServiceDB()
        {
            Dispose(false);
        }



    }
} 