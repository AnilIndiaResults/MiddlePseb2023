using PsebJunior.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PsebJunior.AbstractLayer;
using System.Threading.Tasks;
using System.IO;
using PsebPrimaryMiddle.Filters;
using PsebPrimaryMiddle.Repository;
using Newtonsoft.Json;
using System.Web.UI;
using ClosedXML.Excel;
using CCA.Util;
using System.Configuration;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using encrypt;
namespace PsebPrimaryMiddle.Controllers
{
    public class AdmitCardController : Controller
    {
        private readonly ISchoolRepository _schoolRepository;

        public AdmitCardController(ISchoolRepository schoolRepository)
        {
            _schoolRepository = schoolRepository;
        }
        public static Byte[] QRCoder(string qr)
        {
            QRCodeGenerator _qrCode = new QRCodeGenerator();
            QRCodeData _qrCodeData = _qrCode.CreateQrCode("https://middleprimary2022.pseb.ac.in/AdmitCard/Index/" + QueryStringModule.Encrypt(qr), QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(_qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            return (BitmapToBytesCode(qrCodeImage));

        }
        [NonAction]
        private static Byte[] BitmapToBytesCode(Bitmap image)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }


        // GET: AdmitCard
        public ActionResult Index(string id)
        {
            ViewBag.TotalCount = 0;
            ViewBag.Message = "";


            if (string.IsNullOrEmpty(id))
            {
                ViewBag.Message = "URL is not valid, Please try after sometimes.";
                return View();
            }
            else
            {
                RegistrationModels rm = new RegistrationModels();
                try
                {

                    string sID = id;
                    try
                    {
                        sID = QueryStringModule.Decrypt(id);
                    }
                    catch
                    {
                        sID = id;
                    }
                    id = sID;


                    try
                    {
                        //ViewBag.SelectedItem
                        //AbstractLayer.RegistrationDB objDB = new AbstractLayer.RegistrationDB();

                        rm.QRCode = QRCoder(id);
                        //string ClsType = "4";
                        //rm.Correctiondata = null;
                        ViewBag.TotalCountadded = "";
                        rm.ExamRoll = id;
                        string search = "";
                        search = " 1=1 ";

                        if (rm.ExamRoll.Trim() != "")
                        {
                            search += " and exm.ROLL='" + rm.ExamRoll + "'";
                        }
                        DataSet ds = new DataSet();

                        rm.StoreAllData = _schoolRepository.PrintAdmitCard(search, " ", "8");
                        if (rm.StoreAllData.Tables[0].Rows.Count==0)
                        {
                            rm.StoreAllData = _schoolRepository.PrintAdmitCard(search, " ", "5");
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                catch (Exception ex)
                {

                }
                return View(rm);


            }
        }
    }
}