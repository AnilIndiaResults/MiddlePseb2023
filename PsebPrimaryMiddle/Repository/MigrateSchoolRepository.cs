using Microsoft.Practices.EnterpriseLibrary.Data;
using PsebJunior.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PsebPrimaryMiddle.Repository
{
    public interface IMigrateSchoolRepository
    {
        DataSet ViewAllCandidatesOfSchool(string schl, string search);
        DataSet ChekResultCompairSubjects(string schoolcode, string chkSub);
        DataSet Insert_MigrationForm(MigrateSchoolModels MS);
        DataSet GetMigrationDataByStudentId(int type, string StudentId);
        DataSet GetMigrationDataByIdandSearch(int type, string MigrationId,string search);
        string DeleteMigrationData(string stdid);
    }
}