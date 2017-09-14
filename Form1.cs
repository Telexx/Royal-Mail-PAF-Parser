using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PAFParser
{
    public partial class Form1 : Form
    {

        String connstring = @"Data Source={SERVER};Initial Catalog={DATABASE};Password={PASSWORD};Persist Security Info=True;User ID={USERNAME}";
        String conS = "";
        String conDB = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void btnParse_Click(object sender, EventArgs e)
        {
             conS = connstring.Replace("{SERVER}", txtServer.Text).Replace("{PASSWORD}", txtPass.Text).Replace("{USERNAME}", txtUser.Text).Replace("{DATABASE}", txtDBName.Text);
             conDB = connstring.Replace("{SERVER}", txtServer.Text).Replace("{PASSWORD}", txtPass.Text).Replace("{USERNAME}", txtUser.Text).Replace("Initial Catalog={DATABASE};","");
             ExecuteSQL(conDB, "CREATE DATABASE " + txtDBName.Text);
            //   Parallel.Invoke(() => importThoroughfare(), () => importThoroughfareDescriptor(), ()=> importCompanyData(), () => importBuildingName(), () => importSubBuildingName());
            importLocality();
            importThoroughfare();
            importThoroughfareDescriptor();
            importCompanyData();
            importBuildingName();
            importSubBuildingName();
            importaAddressLines();
        }

        private async Task importaAddressLines()
        {

            Int64 count = 0;
            try
            {
                ExecuteSQL(conS, @"CREATE TABLE tblAddresses (ID int identity(1,1),CompanyName nvarchar(500),BuildingName nvarchar(500), SubBuildingName nvarchar(500), Address1 nvarchar(500),Locality1 nvarchar(100),Locality2 nvarchar(100),Town varchar(500),County nvarchar(500),Postcode nvarchar(10),lat decimal(9,6),lng decimal(9,6))");
                ExecuteSQL(conS, @"
CREATE CLUSTERED INDEX [_dta_index_tblBuildingName_c_6_677577452__K2] ON [dbo].[tblBuildingName]
(
	[BuildingKey] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]


CREATE CLUSTERED INDEX [_dta_index_tblCompany_c_6_661577395__K2] ON [dbo].[tblCompany]
(
	[CompanyKey] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]


CREATE CLUSTERED INDEX [_dta_index_tblSubBuildingName_c_6_693577509__K2] ON [dbo].[tblSubBuildingName]
(
	[SubBuildingKey] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]


CREATE CLUSTERED INDEX [_dta_index_tblThoroughfare_c_6_581577110__K2] ON [dbo].[tblThoroughfare]
(
	[ThoroughfareKey] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]


CREATE CLUSTERED INDEX [_dta_index_tblLocality_c_6_565577053__K2] ON [dbo].[tblLocality]
(
	[LocalityKey] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]


");
                ExecuteSQL(conS, @"CREATE PROC usp_ParseAddress

         @postcode nvarchar(10),
         @addresskey nvarchar(10),
         @localitykey nvarchar(10),
         @thoroughfarekey nvarchar(10),
         @thoroughfaredescriptorkey  nvarchar(10),
         @depthoroughfarekey  nvarchar(10),
         @depthoroughfaredescriptorkey  nvarchar(10),
         @buildingnumber nvarchar(10),
         @buildingnamekey  nvarchar(10),
         @subbuildingnamekey  nvarchar(10),
         @numberofhouseholds  nvarchar(10),
         @orginisationkey nvarchar(10)
AS

DECLARE @Company nvarchar(500) = (select top 1 descriptor from tblCompany where companykey = @orginisationkey and @orginisationkey > 0)
DECLARE @buildingname nvarchar(500) = (select top 1 descriptor from tblBuildingName where BuildingKey = @buildingnamekey and BuildingKey > 0)
DECLARE @subbuildingname nvarchar(500) = (select top 1 descriptor from tblSubBuildingName where subBuildingKey = @subbuildingnamekey and SubBuildingKey > 0)
DECLARE @tfare nvarchar(500) = (select top 1 isnull(descriptor, '') from tblThoroughfare where ThoroughfareKey = @thoroughfarekey and ThoroughfareKey > 0)
DECLARE @tfaredesc nvarchar(500) = (select top 1 isnull(descriptor, '') from tblThoroughfareDescriptor where ThoroughfareKey = @thoroughfaredescriptorkey and ThoroughfareKey> 0)
DECLARE @address1 nvarchar(max) = (select top 1 CASE WHEN cast(@buildingnumber as int) = 0 then '' else @buildingnumber end+' ' + isnull(@tfare, '') + ' ' + isnull(@tfaredesc, ''))
DECLARE @Town nvarchar(35) = (select top 1 PostTown from tblLocality where LocalityKey = @localitykey)
DECLARE @DependantLocality nvarchar(35) = (select top 1 DependantLocality from tblLocality where LocalityKey = @localitykey)
DECLARE @DoubleDependantLocality nvarchar(35) = (select top 1 DoubleDependantLocality from tblLocality where LocalityKey = @localitykey)
INSERT INTO tblAddresses
(CompanyName,
 BuildingName,
 SubBuildingName,
 Address1,
 Town,
 Locality1,
 Locality2,
 Postcode)
 VALUES
 (@Company,
  @buildingname,
  @subbuildingname,
  @address1,
  @Town,
  @DependantLocality,
  @DoubleDependantLocality,
  @postcode
  )
");
            }
            catch (Exception ex) {
                ex.Message.ToString(); }

            try
            {
                string partialName = "fpmainfl";
                DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(tbDataDirectory.Text);
                FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles("*" + partialName + "*.*");

                foreach (FileInfo foundFile in filesInDir)
                {
                    ;
                  //  var filetoread = System.IO.File.OpenRead(foundFile.FullName);
                    //    var fileLines = System.IO.File.ReadAllLines(foundFile.FullName);
                      Parallel.ForEach(File.ReadLines(foundFile.FullName), (singleLine) =>
                    // Read the file and display it line by line.
                  //  foreach (var singleLine in File.ReadLines(foundFile.FullName))
                    {

                  
                    
                        String postcode = singleLine.Substring(0, 7);
                        String addresskey = singleLine.Substring(7, 8);
                        String localitykey = singleLine.Substring(15, 6);
                        String thoroughfarekey = singleLine.Substring(21, 8);
                        String thoroughfaredescriptorkey = singleLine.Substring(29, 4);
                        String depthoroughfarekey = singleLine.Substring(33, 8);
                        String depthoroughfaredescriptorkey = singleLine.Substring(41, 4);
                        String buildingnumber = singleLine.Substring(45, 4);
                        String buildingnamekey = singleLine.Substring(49, 8);
                        String subbuildingnamekey = singleLine.Substring(57, 8);
                        String numberofhouseholds = singleLine.Substring(65, 4);
                        String orginisationkey = singleLine.Substring(69, 8);
                        String postcodetype = singleLine.Substring(77, 1);
                        String concatinationindicator = singleLine.Substring(78, 1);

                        if (postcode.Trim().Length > 0)
                        {
                            using (SqlConnection con = new SqlConnection(conS))
                            {
                                using (SqlCommand cmd = new SqlCommand("usp_ParseAddress", con))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;

                                    cmd.Parameters.Add("@postcode", SqlDbType.VarChar).Value = postcode;
                                    cmd.Parameters.Add("@addresskey", SqlDbType.VarChar).Value = addresskey;
                                    cmd.Parameters.Add("@localitykey", SqlDbType.VarChar).Value = localitykey;
                                    cmd.Parameters.Add("@thoroughfarekey", SqlDbType.VarChar).Value = thoroughfarekey;
                                    cmd.Parameters.Add("@thoroughfaredescriptorkey", SqlDbType.VarChar).Value = thoroughfaredescriptorkey;
                                    cmd.Parameters.Add("@depthoroughfarekey", SqlDbType.VarChar).Value = depthoroughfarekey;
                                    cmd.Parameters.Add("@depthoroughfaredescriptorkey", SqlDbType.VarChar).Value = depthoroughfaredescriptorkey;
                                    cmd.Parameters.Add("@buildingnumber", SqlDbType.VarChar).Value = Convert.ToInt32(buildingnumber).ToString();
                                    cmd.Parameters.Add("@buildingnamekey", SqlDbType.VarChar).Value = buildingnamekey;
                                    cmd.Parameters.Add("@subbuildingnamekey", SqlDbType.VarChar).Value = subbuildingnamekey;
                                    cmd.Parameters.Add("@orginisationkey", SqlDbType.VarChar).Value = orginisationkey;
                                    cmd.Parameters.Add("@numberofhouseholds", SqlDbType.VarChar).Value = "";

                                    con.Open();
                                    cmd.ExecuteNonQuery();
                                    con.Close();
                                }
                            }
                        }

                    });

                }
            }
            catch (Exception ex)
            {
                 ex.Message.ToString();
            }

       
        }


        private async Task importCompanyData()
        {

            Int64 count = 0;
            try
            {
                ExecuteSQL(conS, @"CREATE TABLE tblCompany (ID int identity(1,1), CompanyKey int, Descriptor nvarchar(max),[Type] nvarchar(10))");
            }
            catch { }

            try
            {
                var fileLines = System.IO.File.ReadAllLines(tbDataDirectory.Text + "org.c01");
                Parallel.ForEach(fileLines, (singleLine) =>
                {
                    count = count + 1;
                    //lblCurrentJob.Text = "Import Company Data " + count.ToString() + "/" + fileLines.Count().ToString();
                    //MessageBox.Show(singleLine);
                    string ID = singleLine.Substring(0, 8);
                    string Type;
                    if (singleLine.Substring(8, 1) == "S")
                    {
                        Type = "Small";
                    }
                    else
                    {
                        Type = "Large";
                    }
                    string Descriptor = singleLine.Substring(9, singleLine.Length - 9).Trim().Replace("'", "''"); ;
                    ExecuteSQL(conS, @"INSERT into tblCompany (CompanyKey, Descriptor,[Type]) VALUES (" + ID + ",'" + Descriptor + "','" + Type + "')");
                });
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }

 
        }

        private async Task importLocality()
        {

            Int64 count = 0;
            try
            {
                ExecuteSQL(conS, @"CREATE TABLE tblLocality (ID int identity(1,1), LocalityKey int, PostTown nvarchar(max),DependantLocality nvarchar(max),DoubleDependantLocality nvarchar(max))");
            }
            catch { }

            try
            {
                var fileLines = System.IO.File.ReadAllLines(tbDataDirectory.Text + "local.c01");
                Parallel.ForEach(fileLines, (singleLine) =>
                {
                    count = count + 1;
                    //    lblCurrentJob.Text = "Import Thoroughfare Descriptor " + count.ToString() + "/" + fileLines.Count().ToString();
                    //MessageBox.Show(singleLine);
                    string ID = singleLine.Substring(0, 6);
                    string PostTown = singleLine.Substring(51, 30).Trim().Replace("'", "''"); 
                    string DependantLocality = singleLine.Substring(81, 35).Trim().Replace("'", "''"); 
                    string DoubleDependantLocality = singleLine.Substring(116, 35).Trim().Replace("'", "''"); 
                    ExecuteSQL(conS, @"INSERT into tblLocality(LocalityKey, PostTown,DependantLocality,DoubleDependantLocality) VALUES (" + ID + ",'" + PostTown + "','"+DependantLocality+"','"+DoubleDependantLocality+"')");
                });
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }


        }

        private async Task importThoroughfareDescriptor()
        {
           
            Int64 count = 0;
            try
            {
                ExecuteSQL(conS, @"CREATE TABLE tblThoroughfareDescriptor (ID int identity(1,1), ThoroughfareKey int, Descriptor nvarchar(max))");
            }
            catch { }

            try
            {
                var fileLines = System.IO.File.ReadAllLines(tbDataDirectory.Text + "thdesc.c01");
                Parallel.ForEach(fileLines, (singleLine) =>
                {
                    count = count + 1;
                //    lblCurrentJob.Text = "Import Thoroughfare Descriptor " + count.ToString() + "/" + fileLines.Count().ToString();
                    //MessageBox.Show(singleLine);
                    string ID = singleLine.Substring(0, 4);
                    string Descriptor = singleLine.Substring(4, singleLine.Length - 4).Trim().Replace("'", "''"); ;
                    ExecuteSQL(conS, @"INSERT into tblthoroughfareDescriptor (ThoroughfareKey, Descriptor) VALUES (" + ID + ",'" + Descriptor + "')");
                });
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }

   
        }

        private async Task importBuildingName()
        {

            Int64 count = 0;
            try
            {
                ExecuteSQL(conS, @"CREATE TABLE tblBuildingName (ID int identity(1,1), BuildingKey int, Descriptor nvarchar(max))");
            }
            catch { }

            try
            {
                var fileLines = System.IO.File.ReadAllLines(tbDataDirectory.Text + "bname.c01");
                Parallel.ForEach(fileLines, (singleLine) =>
                {
                    count = count + 1;
               //     lblCurrentJob.Text = "Import Building Name " + count.ToString() + "/" + fileLines.Count().ToString();
                    //MessageBox.Show(singleLine);
                    string ID = singleLine.Substring(0, 8);
                    string Descriptor = singleLine.Substring(8, singleLine.Length - 8).Trim().Replace("'", "''"); ;
                    ExecuteSQL(conS, @"INSERT into tblBuildingName (BuildingKey, Descriptor) VALUES (" + ID + ",'" + Descriptor + "')");
                });
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }

      
        }

        private async Task importSubBuildingName()
        {

            Int64 count = 0;
            try
            {
                ExecuteSQL(conS, @"CREATE TABLE tblSubBuildingName (ID int identity(1,1), SubBuildingKey int, Descriptor nvarchar(max))");
            }
            catch { }

            try
            {
                var fileLines = System.IO.File.ReadAllLines(tbDataDirectory.Text + "subbname.c01");
                Parallel.ForEach(fileLines, (singleLine) =>
                {
                    count = count + 1;
                 //   lblCurrentJob.Text = "Import Building Name " + count.ToString() + "/" + fileLines.Count().ToString();
                    //MessageBox.Show(singleLine);
                    string ID = singleLine.Substring(0, 8);
                    string Descriptor = singleLine.Substring(8, singleLine.Length - 8).Trim().Replace("'", "''"); ;
                    ExecuteSQL(conS, @"INSERT into tblSubBuildingName (SubBuildingKey, Descriptor) VALUES (" + ID + ",'" + Descriptor + "')");
                });
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }


        }



        private async Task importThoroughfare()
        {
            Int64 count = 0;
            try
            {
                ExecuteSQL(conS, @"CREATE TABLE tblThoroughfare (ID int identity(1,1), ThoroughfareKey int, Descriptor nvarchar(max))");
            }
            catch{}

            try
            {
                var fileLines = System.IO.File.ReadAllLines(tbDataDirectory.Text+ "thfare.c01");
                Parallel.ForEach(fileLines, (singleLine) =>
                 {
                    Stopwatch sw = Stopwatch.StartNew();
                     count = count + 1;
                     //lblCurrentJob.Text = "Import Thoroughfare " + count.ToString() + "/" + fileLines.Count().ToString();
                    //MessageBox.Show(singleLine);
                    string ID = singleLine.Substring(0, 8);
                     string Descriptor = singleLine.Substring(8, singleLine.Length - 8).Trim().Replace("'", "''");
                     ExecuteSQL(conS, @"INSERT into tblthoroughfare (ThoroughfareKey, Descriptor) VALUES (" + ID + ",'" + Descriptor + "')");
                   //  Console.WriteLine("Parallel.ForEach() execution time = {0} seconds", sw.Elapsed.TotalSeconds);
                 });
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }


        }

        private void ExecuteSQL(String constring, String SQL)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(constring))
                {
                    SqlCommand command = new SqlCommand(SQL, connection);
                    connection.Open();
                    command.ExecuteNonQuery();
                    try
                    {
                       
                    }
                    catch (Exception ex)
                    {
                        ex.Message.ToString();
                    }
                    finally
                    {
                        // Always call Close when done reading.
                        connection.Close();
                     
                    }
                }
            }
            catch(Exception ex) {
                ex.Message.ToString();
            }
        }

        private void bgImport_DoWork(object sender, DoWorkEventArgs e)
        {

        }
    }
}
