using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using PsebJunior.Models;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PsebJunior.AbstractLayer;
using PsebPrimaryMiddle.Filters;
using System.Threading;

namespace PsebPrimaryMiddle.Controllers
{
    public class WebserviceController : Controller
    {
        //WebSerDB WebSerDB = new WebSerDB();
        //AbstractLayer.DBClass DBClass = new AbstractLayer.DBClass();
        private static readonly ingovepunjabschool.service_pseb_5th_and_8th abc = new ingovepunjabschool.service_pseb_5th_and_8th();
        private string F = "5";
        private string A = "8";
        // GET: Webservice

        [SessionCheckFilter]
        public ActionResult ImportDataFifth()
        {
            string formName = "F1";
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.fifth == "N")
            { return RedirectToAction("Index", "Home"); }

            string udic = WebSerDB.GetUdiCode(loginSession.SCHL);
            webSerModel wm = new webSerModel();
            wm.UdiseCode = udic;

            if (udic != "" || udic != null)
            {
                wm.StoreAllData = WebSerDB.GetudiCodeDetails_SpJunior(formName, udic);
                if (wm.StoreAllData == null || wm.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(wm);
                }
                else
                {
                    ViewBag.TotalCount = wm.StoreAllData.Tables[0].Rows.Count;
                    return View(wm);
                }
            }

            return View(wm);
        }
        [SessionCheckFilter]
        [HttpPost]
        public ActionResult ImportDataFifth(FormCollection frc, webSerModel wm, string cmd, string importBy, string SearchString, string chkImportid)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.fifth == "N")
            { return RedirectToAction("Index", "Home"); }

            string formName = "F1";
            string EpunSearch = "";
            string aadharnoSearch = "";
            string res = null;
            if (cmd == "Import Records")
            {
                try
                {
                    // epunjabschool.service_pseb abc = new epunjabschool.service_pseb();
                    if (importBy != "")
                    {
                        ViewBag.SelectedItem = importBy;
                        int SelValueSch = Convert.ToInt32(importBy.ToString());
                        if (SelValueSch == 1)
                        {
                            EpunSearch = SearchString.ToString().Trim();
                            try
                            {
                                // string strParameter = Encrypt("PSEB4488~08059585PSEB~" + F + "~" + EpunSearch);
                                //string JsonString = Decrypt(abc.get9thClassStudentDetails_ByStudents(strParameter));                           
                                string strParameter = WebSerDB.Encrypt("PSEB4488~08059585PSEB~" + EpunSearch + "~" + F);
                                string JsonString = WebSerDB.Decrypt(abc.get5thAnd8thClassStudentDetails_ByStudents(strParameter));
                                DataTable dt1 = new DataTable();
                                dt1 = JsonStringToDataTable(JsonString);
                                string epunid = dt1.Rows[0]["studentdID"].ToString();
                                string aadhar = dt1.Rows[0]["studentUID"].ToString();
                                string Name = dt1.Rows[0]["studentName"].ToString();
                                string Pname = DBClass.getPunjabiName(Name);

                                string fname = dt1.Rows[0]["fatherName"].ToString();
                                string Pfname = DBClass.getPunjabiName(fname);

                                string mname = dt1.Rows[0]["motherName"].ToString();
                                string Pmname = DBClass.getPunjabiName(mname);

                                string dob = "";
                                //string inputString = string.Format("{0:d/MM/yyyy}", dt1.Rows[0]["dob"]).ToString();
                                //DateTime dDate;
                                //if (DateTime.TryParse(inputString, out dDate))
                                //{
                                //    dob = Convert.ToDateTime(dt1.Rows[0]["dob"]).ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                //}
                                //else
                                //{
                                //    dob = "";
                                //    //Console.WriteLine("Invalid"); // <-- Control flow goes here
                                //}


                                string sex = WebSerDB.GetGender(dt1.Rows[0]["genderID"].ToString());
                                string caste = WebSerDB.GetCaste(dt1.Rows[0]["casteCategoryCode"].ToString());
                                string reli = WebSerDB.GetReligion(dt1.Rows[0]["religionCode"].ToString());
                                string DA = WebSerDB.GetPHY_CHL(dt1.Rows[0]["disabilityType"].ToString());
                                //
                                string mob = dt1.Rows[0]["contactNo"].ToString();
                                string admno = dt1.Rows[0]["admissionNo"].ToString();
                                string Address = dt1.Rows[0]["village"].ToString() + ',' + dt1.Rows[0]["gramPanchayat"].ToString();
                                string pincode = dt1.Rows[0]["pinCode"].ToString();

                                res = WebSerDB.insert_EPubjabWebservice_DataJunior(formName, loginSession.SCHL, epunid, aadhar, Name, Pname, fname, Pfname, mname, Pmname, dob, sex, caste, reli, DA, mob, admno, Address, pincode);
                            }
                            catch (Exception ex)
                            {
                                TempData["result"] = 5;
                                return View(wm);
                                throw;
                            }
                            if (res == "0" || res == null)
                            {
                                //--------------Not saved
                                TempData["result"] = 0;
                                return View(wm);
                            }
                            else if (res == "-1")
                            {
                                //-----alredy exist
                                TempData["result"] = -1;
                                return View(wm);
                            }
                            else if (res == "-3")
                            {
                                //-----alredy exist
                                TempData["result"] = -3;
                                return View(wm);
                            }
                            else if (res == "3")
                            {
                                //-----alredy exist
                                TempData["TotImported"] = 1;
                                TempData["result"] = 1;
                                return View(wm);
                            }
                            else
                            {
                                TempData["TotImported"] = 1;
                                TempData["result"] = 1;
                                return View(wm);
                            }

                        }
                        if (SelValueSch == 2)
                        {
                            aadharnoSearch = SearchString.ToString().Trim();
                            try
                            {
                                string strParameter = WebSerDB.Encrypt("PSEB4488~08059585PSEB~" + aadharnoSearch + "~" + F);
                                // string JsonString = Decrypt(abc.get9thClassStudentDetails_ByUID(strParameter));
                                string JsonString = WebSerDB.Decrypt(abc.get5thAnd8ththClassStudentDetails_ByUID(strParameter));
                                DataTable dt1 = new DataTable();
                                dt1 = JsonStringToDataTable(JsonString);
                                string epunid = dt1.Rows[0]["studentdID"].ToString();
                                string aadhar = dt1.Rows[0]["studentUID"].ToString();
                                string Name = dt1.Rows[0]["studentName"].ToString();
                                string Pname = DBClass.getPunjabiName(Name);

                                string fname = dt1.Rows[0]["fatherName"].ToString();
                                string Pfname = DBClass.getPunjabiName(fname);

                                string mname = dt1.Rows[0]["motherName"].ToString();
                                string Pmname = DBClass.getPunjabiName(mname);
                                string dob = "";
                                //string inputString = string.Format("{0:d/MM/yyyy}", dt1.Rows[0]["dob"]).ToString();
                                //DateTime dDate;
                                //if (DateTime.TryParse(inputString, out dDate))
                                //{
                                //    dob = Convert.ToDateTime(dt1.Rows[0]["dob"]).ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                //}
                                //else
                                //{
                                //    dob = "";
                                //    //Console.WriteLine("Invalid"); // <-- Control flow goes here
                                //}
                                string sex = WebSerDB.GetGender(dt1.Rows[0]["genderID"].ToString());
                                string caste = WebSerDB.GetCaste(dt1.Rows[0]["casteCategoryCode"].ToString());
                                string reli = WebSerDB.GetReligion(dt1.Rows[0]["religionCode"].ToString());
                                string DA = WebSerDB.GetPHY_CHL(dt1.Rows[0]["disabilityType"].ToString());
                                //
                                string mob = dt1.Rows[0]["contactNo"].ToString();
                                string admno = dt1.Rows[0]["admissionNo"].ToString();
                                string Address = dt1.Rows[0]["village"].ToString() + ',' + dt1.Rows[0]["gramPanchayat"].ToString();
                                string pincode = dt1.Rows[0]["pinCode"].ToString();

                                res = WebSerDB.insert_EPubjabWebservice_DataJunior(formName, loginSession.SCHL, epunid, aadhar, Name, Pname, fname, Pfname, mname, Pmname, dob, sex, caste, reli, DA, mob, admno, Address, pincode);
                            }
                            catch (Exception ex)
                            {
                                TempData["result"] = 5;
                                return View(wm);
                                throw;
                            }
                            if (res == "0" || res == null)
                            {
                                //--------------Not saved
                                TempData["result"] = 0;
                                return View(wm);
                            }
                            else if (res == "-1")
                            {
                                //-----alredy exist
                                TempData["result"] = -1;
                                return View(wm);
                            }
                            else if (res == "-3")
                            {
                                //-----alredy exist
                                TempData["result"] = -3;
                                return View(wm);
                            }
                            else if (res == "3")
                            {
                                //-----alredy exist
                                TempData["TotImported"] = 1;
                                TempData["result"] = 1;
                                return View(wm);
                            }
                            else
                            {
                                TempData["TotImported"] = 1;
                                TempData["result"] = 1;
                                return View(wm);
                            }

                        }
                    }


                }
                catch (Exception ex)
                {

                    throw;
                }

            }
            if (cmd == "Submit")
            {
                try
                {
                    string UdiseCode = frc["UdiseCode"].ToString(); //03010307002;
                    string strParameter = WebSerDB.Encrypt("PSEB4488~08059585PSEB~" + UdiseCode + "~" + F);
                    //string strParameter = Encrypt("PSEB4488~08059585PSEB~" + F + "~" + UdiseCode);
                    //string JsonString = Decrypt(abc.get9thClassAllStudents(strParameter));
                    string JsonString = WebSerDB.Decrypt(abc.get5thAnd8ththClassAllStudents(strParameter));
                    DataTable dt = new DataTable();
                    dt = JsonStringToDataTable(JsonString);

                    string result = WebSerDB.Insert_udiCodeTemp_SpJunior(formName, wm, frc, dt);
                    if (result == "0" || result == null)
                    {
                        //--------------Not saved
                        TempData["result"] = 0;
                    }
                    else if (result == "-1")
                    {
                        //-----alredy exist
                        TempData["result"] = -1;
                    }
                    else
                    {
                        TempData["TotImported"] = 1;
                        TempData["result"] = "Inserted";
                    }

                    //ViewBag.TotalCount = dt.Rows.Count;
                    //wm.StoreAllDataTable = dt;
                    wm.StoreAllData = WebSerDB.GetudiCodeDetails_SpJunior(formName, UdiseCode);
                    if (wm.StoreAllData == null || wm.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View(wm);
                    }
                    else
                    {
                        ViewBag.TotalCount = wm.StoreAllData.Tables[0].Rows.Count;
                        return View(wm);
                    }
                }
                catch (Exception ex)
                {
                    TempData["result"] = 5;
                    return View(wm);
                    throw;
                }


            }
            //else
            if (cmd == "Import Selected Record")
            {
                string collectId = frc["ChkCNinthClass"];
                if (collectId == null || collectId == "")
                {
                    return View(wm);
                }
                int cnt = collectId.Count(x => x == ',');
                TempData["TotImported"] = 0;
                int recr = 0;

                DataTable dt = new DataTable();
                if (chkImportid != "")
                {
                    string[] words = collectId.Split(',');

                    if (words.Count() > 5)
                    {
                        TempData["result"] = 10;
                        return View(wm);
                    }

                    foreach (string word in words)
                    {
                        try
                        {
                            string strParameter = WebSerDB.Encrypt("PSEB4488~08059585PSEB~" + word + "~" + F);
                            string JsonString = WebSerDB.Decrypt(abc.get5thAnd8thClassStudentDetails_ByStudents(strParameter));
                            DataTable dt1 = new DataTable();
                            dt1 = JsonStringToDataTable(JsonString);
                            if (dt1 != null)
                            {

                                string epunid = dt1.Rows[0]["studentdID"].ToString();
                                string aadhar = dt1.Rows[0]["studentUID"].ToString();
                                string Name = dt1.Rows[0]["studentName"].ToString();
                                string Pname = DBClass.getPunjabiName(Name);

                                string fname = dt1.Rows[0]["fatherName"].ToString();
                                string Pfname = DBClass.getPunjabiName(fname);

                                string mname = dt1.Rows[0]["motherName"].ToString();
                                string Pmname = DBClass.getPunjabiName(mname);


                                string dob = "";
                                //string inputString = string.Format("{0:d/MM/yyyy}", dt1.Rows[0]["dob"]).ToString();
                                //DateTime dDate;
                                //if (DateTime.TryParse(inputString, out dDate))
                                //{
                                //    dob = Convert.ToDateTime(dt1.Rows[0]["dob"]).ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                //}
                                //else
                                //{
                                //    dob = "";
                                //    //Console.WriteLine("Invalid"); // <-- Control flow goes here
                                //}
                                string sex = WebSerDB.GetGender(dt1.Rows[0]["genderID"].ToString());
                                string caste = WebSerDB.GetCaste(dt1.Rows[0]["casteCategoryCode"].ToString());
                                string reli = WebSerDB.GetReligion(dt1.Rows[0]["religionCode"].ToString());
                                string DA = WebSerDB.GetPHY_CHL(dt1.Rows[0]["disabilityType"].ToString());
                                //
                                string mob = dt1.Rows[0]["contactNo"].ToString();
                                string admno = dt1.Rows[0]["admissionNo"].ToString();
                                string Address = dt1.Rows[0]["village"].ToString() + ',' + dt1.Rows[0]["gramPanchayat"].ToString();
                                string pincode = dt1.Rows[0]["pinCode"].ToString();

                                res = WebSerDB.insert_EPubjabWebservice_DataJunior(formName, loginSession.SCHL, epunid, aadhar, Name, Pname, fname, Pfname, mname, Pmname, dob, sex, caste, reli, DA, mob, admno, Address, pincode);
                                if (res != "-1")
                                {
                                    TempData["TotImported"] = recr + 1;
                                    recr = recr + 1;
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            TempData["result"] = 5;
                            return View(wm);
                            throw;
                        }

                    }
                    if (res == "0" || res == null)
                    {
                        //--------------Not saved
                        TempData["result"] = 0;
                    }
                    else if (res == "-1")
                    {
                        //-----alredy exist
                        TempData["result"] = -1;
                    }
                    else if (res == "-3")
                    {
                        //-----alredy exist
                        TempData["result"] = -3;
                    }
                    else
                    {
                        TempData["result"] = 1;
                    }

                }

                //return View(wm);
                //---------------End Import Selected Data here----------------//
            }
            return View(wm);
        }


        #region A1 (8th Class)
        //[SessionCheckFilter]
        public ActionResult ImportDataEighth()
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.middle == "N")
            { return RedirectToAction("Index", "Home"); }
            string formName = "A1";



            string udic = WebSerDB.GetUdiCode(loginSession.SCHL);
            webSerModel wm = new webSerModel();
            wm.UdiseCode = udic;

            if (udic != "" || udic != null)
            {
                wm.StoreAllData = WebSerDB.GetudiCodeDetails_SpJunior(formName, udic);
                if (wm.StoreAllData == null || wm.StoreAllData.Tables[0].Rows.Count == 0)
                {
                    ViewBag.Message = "Record Not Found";
                    ViewBag.TotalCount = 0;
                    return View(wm);
                }
                else
                {
                    ViewBag.TotalCount = wm.StoreAllData.Tables[0].Rows.Count;
                    return View(wm);
                }
            }

            return View(wm);
        }

        //[SessionCheckFilter]
        [HttpPost]
        public ActionResult ImportDataEighth(FormCollection frc, webSerModel wm, string cmd, string importBy, string SearchString, string chkImportid)
        {
            LoginSession loginSession = (LoginSession)Session["LoginSession"];
            if (loginSession.middle == "N")
            { return RedirectToAction("Index", "Home"); }
            string formName = "A1";
            string EpunSearch = "";
            string aadharnoSearch = "";
            string res = null;
            if (cmd == "Import Records")
            {
                try
                {
                    // epunjabschool.service_pseb abc = new epunjabschool.service_pseb();
                    if (importBy != "")
                    {
                        ViewBag.SelectedItem = importBy;
                        int SelValueSch = Convert.ToInt32(importBy.ToString());
                        if (SelValueSch == 1)
                        {
                            EpunSearch = SearchString.ToString().Trim();
                            try
                            {
                                string strParameter = WebSerDB.Encrypt("PSEB4488~08059585PSEB~" + EpunSearch + "~" + A);
                                Thread.Sleep(5000);
                                string JsonString = WebSerDB.Decrypt(abc.get5thAnd8thClassStudentDetails_ByStudents(strParameter));
                                Thread.Sleep(5000);
                                DataTable dt1 = new DataTable();
                                dt1 = JsonStringToDataTable(JsonString);
                                string epunid = dt1.Rows[0]["studentdID"].ToString();
                                string aadhar = dt1.Rows[0]["studentUID"].ToString();
                                string Name = dt1.Rows[0]["studentName"].ToString();
                                string Pname = DBClass.getPunjabiName(Name);

                                string fname = dt1.Rows[0]["fatherName"].ToString();
                                string Pfname = DBClass.getPunjabiName(fname);

                                string mname = dt1.Rows[0]["motherName"].ToString();
                                string Pmname = DBClass.getPunjabiName(mname);

                                string dob = "";
                                //string inputString = string.Format("{0:d/MM/yyyy}", dt1.Rows[0]["dob"]).ToString();
                                //DateTime dDate;
                                //if (DateTime.TryParse(inputString, out dDate))
                                //{
                                //    dob = Convert.ToDateTime(dt1.Rows[0]["dob"]).ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                //}
                                //else
                                //{
                                //    dob = "";
                                //    //Console.WriteLine("Invalid"); // <-- Control flow goes here
                                //}
                                string sex = WebSerDB.GetGender(dt1.Rows[0]["genderID"].ToString());
                                string caste = WebSerDB.GetCaste(dt1.Rows[0]["casteCategoryCode"].ToString());
                                string reli = WebSerDB.GetReligion(dt1.Rows[0]["religionCode"].ToString());
                                string DA = WebSerDB.GetPHY_CHL(dt1.Rows[0]["disabilityType"].ToString());
                                //
                                string mob = dt1.Rows[0]["contactNo"].ToString();
                                string admno = dt1.Rows[0]["admissionNo"].ToString();
                                string Address = dt1.Rows[0]["village"].ToString() + ',' + dt1.Rows[0]["gramPanchayat"].ToString();
                                string pincode = dt1.Rows[0]["pinCode"].ToString();

                                res = WebSerDB.insert_EPubjabWebservice_DataJunior(formName, loginSession.SCHL, epunid, aadhar, Name, Pname, fname, Pfname, mname, Pmname, dob, sex, caste, reli, DA, mob, admno, Address, pincode);
                            }
                            catch (Exception ex)
                            {
                                TempData["result"] = 5;
                                return View(wm);
                                throw;
                            }
                            if (res == "0" || res == null)
                            {
                                //--------------Not saved
                                TempData["result"] = 0;
                                return View(wm);
                            }
                            else if (res == "-1")
                            {
                                //-----alredy exist
                                TempData["result"] = -1;
                                return View(wm);
                            }
                            else if (res == "-3")
                            {
                                //-----alredy exist
                                TempData["result"] = -3;
                                return View(wm);
                            }
                            else if (res == "3")
                            {
                                //-----alredy exist
                                TempData["TotImported"] = 1;
                                TempData["result"] = 1;
                                return View(wm);
                            }
                            else
                            {
                                TempData["TotImported"] = 1;
                                TempData["result"] = 1;
                                return View(wm);
                            }

                        }
                        if (SelValueSch == 2)
                        {
                            aadharnoSearch = SearchString.ToString().Trim();
                            try
                            {
                                //  string strParameter = Encrypt("PSEB4488~08059585PSEB~" + A + "~" + aadharnoSearch);
                                string strParameter = WebSerDB.Encrypt("PSEB4488~08059585PSEB~" + aadharnoSearch + "~" + A);
                                Thread.Sleep(5000);
                                // string JsonString = Decrypt(abc.get9thClassStudentDetails_ByUID(strParameter));
                                string JsonString = WebSerDB.Decrypt(abc.get5thAnd8ththClassStudentDetails_ByUID(strParameter));
                                Thread.Sleep(5000);
                                DataTable dt1 = new DataTable();
                                dt1 = JsonStringToDataTable(JsonString);
                                string epunid = dt1.Rows[0]["studentdID"].ToString();
                                string aadhar = dt1.Rows[0]["studentUID"].ToString();
                                string Name = dt1.Rows[0]["studentName"].ToString();
                                string Pname = DBClass.getPunjabiName(Name);

                                string fname = dt1.Rows[0]["fatherName"].ToString();
                                string Pfname = DBClass.getPunjabiName(fname);

                                string mname = dt1.Rows[0]["motherName"].ToString();
                                string Pmname = DBClass.getPunjabiName(mname);
                                string dob = "";
                                //string inputString = string.Format("{0:d/MM/yyyy}", dt1.Rows[0]["dob"]).ToString();
                                //DateTime dDate;
                                //if (DateTime.TryParse(inputString, out dDate))
                                //{
                                //    dob = Convert.ToDateTime(dt1.Rows[0]["dob"]).ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                //}
                                //else
                                //{
                                //    dob = "";
                                //    //Console.WriteLine("Invalid"); // <-- Control flow goes here
                                //}
                                string sex = WebSerDB.GetGender(dt1.Rows[0]["genderID"].ToString());
                                string caste = WebSerDB.GetCaste(dt1.Rows[0]["casteCategoryCode"].ToString());
                                string reli = WebSerDB.GetReligion(dt1.Rows[0]["religionCode"].ToString());
                                string DA = WebSerDB.GetPHY_CHL(dt1.Rows[0]["disabilityType"].ToString());
                                //
                                string mob = dt1.Rows[0]["contactNo"].ToString();
                                string admno = dt1.Rows[0]["admissionNo"].ToString();
                                string Address = dt1.Rows[0]["village"].ToString() + ',' + dt1.Rows[0]["gramPanchayat"].ToString();
                                string pincode = dt1.Rows[0]["pinCode"].ToString();
                                res = WebSerDB.insert_EPubjabWebservice_DataJunior(formName, loginSession.SCHL, epunid, aadhar, Name, Pname, fname, Pfname, mname, Pmname, dob, sex, caste, reli, DA, mob, admno, Address, pincode);

                            }
                            catch (Exception ex)
                            {
                                TempData["result"] = 5;
                                return View(wm);
                                throw;
                            }
                            if (res == "0" || res == null)
                            {
                                //--------------Not saved
                                TempData["result"] = 0;
                                return View(wm);
                            }
                            else if (res == "-1")
                            {
                                //-----alredy exist
                                TempData["result"] = -1;
                                return View(wm);
                            }
                            else if (res == "-3")
                            {
                                //-----alredy exist
                                TempData["result"] = -3;
                                return View(wm);
                            }
                            else if (res == "3")
                            {
                                //-----alredy exist
                                TempData["TotImported"] = 1;
                                TempData["result"] = 1;
                                return View(wm);
                            }
                            else
                            {
                                TempData["TotImported"] = 1;
                                TempData["result"] = 1;
                                return View(wm);
                            }

                        }
                    }


                }
                catch (Exception ex)
                {

                    throw;
                }

            }
            if (cmd == "Submit")
            {
                try
                {
                    string UdiseCode = frc["UdiseCode"].ToString(); //03010307002;
                                                                    // string strParameter = Encrypt("PSEB4488~08059585PSEB~" + UdiseCode + "~"+  A  );
                    string strParameter = WebSerDB.Encrypt("PSEB4488~08059585PSEB~" + UdiseCode + "~" + A);
                    //Thread.Sleep(5000);
                    //string JsonString = Decrypt(abc.get9thClassAllStudents(strParameter));
                    string JsonString = WebSerDB.Decrypt(abc.get5thAnd8ththClassAllStudents(strParameter));
                    
                    DataTable dt = new DataTable();
                    dt = JsonStringToDataTable(JsonString);

                    string result = WebSerDB.Insert_udiCodeTemp_SpJunior(formName, wm, frc, dt);
                    if (result == "0" || result == null)
                    {
                        //--------------Not saved
                        TempData["result"] = 0;
                    }
                    else if (result == "-1")
                    {
                        //-----alredy exist
                        TempData["result"] = -1;
                    }
                    else
                    {
                        TempData["result"] = "Inserted";
                    }

                    //ViewBag.TotalCount = dt.Rows.Count;
                    //wm.StoreAllDataTable = dt;
                    wm.StoreAllData = WebSerDB.GetudiCodeDetails_SpJunior(formName, UdiseCode);
                    if (wm.StoreAllData == null || wm.StoreAllData.Tables[0].Rows.Count == 0)
                    {
                        ViewBag.Message = "Record Not Found";
                        ViewBag.TotalCount = 0;
                        return View(wm);
                    }
                    else
                    {
                        ViewBag.TotalCount = wm.StoreAllData.Tables[0].Rows.Count;
                        return View(wm);
                    }
                }
                catch (Exception ex)
                {
                    TempData["result"] = 5;
                    return View(wm);
                    throw;
                }


            }
            //else
            if (cmd == "Import Selected Record")
            {
                string collectId = frc["ChkCNinthClass"];
                if (collectId == null || collectId == "")
                {
                    return View(wm);
                }
                int cnt = collectId.Count(x => x == ',');
                TempData["TotImported"] = 0;
                int recr = 0;

                DataTable dt = new DataTable();
                if (chkImportid != "")
                {
                    string[] words = collectId.Split(',');
                    if (words.Count() > 5)
                    {
                        TempData["result"] = 10;
                        return View(wm);
                    }

                    foreach (string word in words)
                    {
                        try
                        {
                            // string strParameter = Encrypt("PSEB4488~08059585PSEB~" + A + "~" + word);
                            string strParameter = WebSerDB.Encrypt("PSEB4488~08059585PSEB~" + word + "~" + A);
                            string JsonString = WebSerDB.Decrypt(abc.get5thAnd8thClassStudentDetails_ByStudents(strParameter));
                            DataTable dt1 = new DataTable();
                            dt1 = JsonStringToDataTable(JsonString);
                            if (dt1 != null)
                            {

                                string epunid = dt1.Rows[0]["studentdID"].ToString();
                                string aadhar = dt1.Rows[0]["studentUID"].ToString();
                                string Name = dt1.Rows[0]["studentName"].ToString();
                                string Pname = DBClass.getPunjabiName(Name);

                                string fname = dt1.Rows[0]["fatherName"].ToString();
                                string Pfname = DBClass.getPunjabiName(fname);

                                string mname = dt1.Rows[0]["motherName"].ToString();
                                string Pmname = DBClass.getPunjabiName(mname);


                                string dob = "";
                                //string inputString = string.Format("{0:d/MM/yyyy}", dt1.Rows[0]["dob"]).ToString();
                                //DateTime dDate;
                                //if (DateTime.TryParse(inputString, out dDate))
                                //{
                                //    dob = Convert.ToDateTime(dt1.Rows[0]["dob"]).ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                //}
                                //else
                                //{
                                //    dob = "";
                                //    //Console.WriteLine("Invalid"); // <-- Control flow goes here
                                //}
                                string sex = WebSerDB.GetGender(dt1.Rows[0]["genderID"].ToString());
                                string caste = WebSerDB.GetCaste(dt1.Rows[0]["casteCategoryCode"].ToString());
                                string reli = WebSerDB.GetReligion(dt1.Rows[0]["religionCode"].ToString());
                                string DA = WebSerDB.GetPHY_CHL(dt1.Rows[0]["disabilityType"].ToString());
                                //
                                string mob = dt1.Rows[0]["contactNo"].ToString();
                                string admno = dt1.Rows[0]["admissionNo"].ToString();
                                string Address = dt1.Rows[0]["village"].ToString() + ',' + dt1.Rows[0]["gramPanchayat"].ToString();
                                string pincode = dt1.Rows[0]["pinCode"].ToString();

                                res = WebSerDB.insert_EPubjabWebservice_DataJunior(formName, loginSession.SCHL, epunid, aadhar, Name, Pname, fname, Pfname, mname, Pmname, dob, sex, caste, reli, DA, mob, admno, Address, pincode);
                                if (res != "-1")
                                {
                                    TempData["TotImported"] = recr + 1;
                                    recr = recr + 1;
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            TempData["result"] = 5;
                            return View(wm);
                            throw;
                        }

                    }
                    if (res == "0" || res == null)
                    {
                        //--------------Not saved
                        TempData["result"] = 0;
                    }
                    else if (res == "-1")
                    {
                        //-----alredy exist                        
                        TempData["result"] = -1;
                    }
                    else if (res == "-3")
                    {
                        //-----alredy exist
                        TempData["result"] = -3;
                    }
                    else
                    {

                        TempData["result"] = 1;
                    }

                }

                //return View(wm);
                //---------------End Import Selected Data here----------------//
            }
            return View(wm);
        }

        #endregion


        public DataTable JsonStringToDataTable(string jsonString)
        {
            DataTable dt1 = (DataTable)JsonConvert.DeserializeObject(jsonString, (typeof(DataTable)));
            return dt1;
        }



    }
}