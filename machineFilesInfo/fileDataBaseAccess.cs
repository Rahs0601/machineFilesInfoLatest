﻿using System;
using System.Data.SqlClient;
using System.Linq;

namespace machineFilesInfo
{
    internal static class fileDataBaseAccess
    {
        public static void InsertOrUpdateStandardIntoDatabase(FileInformation SFile, SqlConnection conn)
        {
            try
            {
                string standardfileName = SFile.FileName;
                string fileType = SFile.FileType;
                string filePath = SFile.FolderPath;
                string fileSize = SFile.FileSize.ToString();
                string fileDateCreated = SFile.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");
                string StandardModifiedDate = SFile.ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss");
                string fileOwner = SFile.Owner;
                string computer = SFile.ComputerName;
                int isMoved = 0;
                string operation = SFile.FolderPath.Split('\\').Reverse().Skip(1).First();
                string componentid = SFile.FolderPath.Split('\\').Reverse().Skip(3).First();
                int operationno = int.Parse(operation.Split('_').First());
                string operationDespcription = operation.Split('_').Last();
                string insertOrUpdateQry = @"IF EXISTS (SELECT * FROM machineFileInfo WHERE operationno = @operationno AND componentid = @componentid)
                                        BEGIN
                                            UPDATE machineFileInfo 
                                            SET StandardModifiedDate = @StandardModifiedDate,
                                                isMoved = @isMoved,
                                                UpdatedTS = GETDATE() 
                                            WHERE operationno = @operationno AND componentid = @componentid;
                                        END
                                        ELSE
                                            BEGIN
                                                INSERT INTO machineFileInfo (standardfileName, fileType, filePath, fileSize, fileDateCreated, StandardModifiedDate, fileOwner, computer, isMoved, operationno, operationDespcription, componentid, UpdatedTS)
                                                VALUES (@standardfileName, @fileType, @filePath, @fileSize, @fileDateCreated, @StandardModifiedDate, @fileOwner, @computer, @isMoved, @operationno, @operationDespcription, @componentid, GETDATE());
                                            END";


                using (SqlCommand cmd = new SqlCommand(insertOrUpdateQry, conn))
                {
                    _ = cmd.Parameters.AddWithValue("@standardfileName", standardfileName);
                    _ = cmd.Parameters.AddWithValue("@fileType", fileType);
                    _ = cmd.Parameters.AddWithValue("@filePath", filePath);
                    _ = cmd.Parameters.AddWithValue("@fileSize", fileSize);
                    _ = cmd.Parameters.AddWithValue("@fileDateCreated", fileDateCreated);
                    _ = cmd.Parameters.AddWithValue("@StandardModifiedDate", StandardModifiedDate);
                    _ = cmd.Parameters.AddWithValue("@fileOwner", fileOwner);
                    _ = cmd.Parameters.AddWithValue("@computer", computer);
                    _ = cmd.Parameters.AddWithValue("@isMoved", isMoved);
                    _ = cmd.Parameters.AddWithValue("@operationno", operationno);
                    _ = cmd.Parameters.AddWithValue("@operationDespcription", operationDespcription);
                    _ = cmd.Parameters.AddWithValue("@componentid", componentid);
                    _ = cmd.ExecuteNonQuery();
                    Logger.WriteExtraLog($"File {SFile.FileName} information inserted or updated in the database." + DateTime.Now);
                }
            }
            catch (Exception ex)
            {

                Logger.WriteErrorLog("Error in InsertOrUpdateStandardIntoDatabase: " + ex.Message);
            }
        }

        public static void InsertOrUpdateProvenIntoDatabase(FileInformation PFile, SqlConnection conn)
        {
            string provenfileName = PFile.FileName;
            string fileType = PFile.FileType;
            string filePath = PFile.FolderPath;
            string fileSize = PFile.FileSize.ToString();
            string fileDateCreated = PFile.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");
            string ProvenModifiedDate = PFile.ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss");
            string fileOwner = PFile.Owner;
            string computer = PFile.ComputerName;
            int isMoved = 1;
            string operation = PFile.FolderPath.Split('\\').Reverse().Skip(1).First();
            string componentid = PFile.FolderPath.Split('\\').Reverse().Skip(3).First();
            int operationno = int.Parse(operation.Split('_').First());
            string operationDespcription = operation.Split('_').Last();
            string insertOrUpdateQry = @"IF EXISTS (SELECT * FROM machineFileInfo WHERE operationno = @operationno AND componentid = @componentid)
                                        BEGIN
                                            UPDATE machineFileInfo 
                                            SET provenfileName = @provenfileName,
                                                ProvenModifiedDate = @ProvenModifiedDate,
                                                isMoved = @isMoved,
                                                UpdatedTS = GETDATE() 
                                            WHERE operationno = @operationno AND componentid = @componentid;
                                        END
                                        ELSE
                                        BEGIN
                                            INSERT INTO machineFileInfo (provenfileName, fileType, filePath, fileSize, fileDateCreated, ProvenModifiedDate, fileOwner, computer, isMoved, operationno, operationDespcription, componentid, UpdatedTS)
                                            VALUES (@provenfileName, @fileType, @filePath, @fileSize, @fileDateCreated, @ProvenModifiedDate, @fileOwner, @computer, @isMoved, @operationno, @operationDespcription, @componentid, GETDATE());
                                        END;
                                        ";

            using (SqlCommand cmd = new SqlCommand(insertOrUpdateQry, conn))
            {
                _ = cmd.Parameters.AddWithValue("@provenfileName", provenfileName);
                _ = cmd.Parameters.AddWithValue("@fileType", fileType);
                _ = cmd.Parameters.AddWithValue("@filePath", filePath);
                _ = cmd.Parameters.AddWithValue("@fileSize", fileSize);
                _ = cmd.Parameters.AddWithValue("@fileDateCreated", fileDateCreated);
                _ = cmd.Parameters.AddWithValue("@ProvenModifiedDate", ProvenModifiedDate);
                _ = cmd.Parameters.AddWithValue("@fileOwner", fileOwner);
                _ = cmd.Parameters.AddWithValue("@computer", computer);
                _ = cmd.Parameters.AddWithValue("@isMoved", isMoved);
                _ = cmd.Parameters.AddWithValue("@operationno", operationno);
                _ = cmd.Parameters.AddWithValue("@operationDespcription", operationDespcription);
                _ = cmd.Parameters.AddWithValue("@componentid", componentid);
                _ = cmd.ExecuteNonQuery();
            }
        }

        public static void UpdateStatus(SqlConnection conn)
        {
            try
            {
                string updateStatusQry = @"UPDATE machineFileInfo SET isMoved=CAST(T1.ABC AS bit)FROM(SELECT componentid,operationno,(CASE WHEN provenModifiedDate>=StandardModifiedDate THEN 1 ELSE 0 END) AS ABCFROM machineFileInfo A1) T1INNER JOIN machineFileInfo T2 ON T1.componentID=T2.componentID AND T1.OperationNo=t2.operationno";
                using (SqlCommand cmd = new SqlCommand(updateStatusQry, conn))
                {
                    _ = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {

                Logger.WriteErrorLog("Error in UpdateStatus: " + ex.Message);
            }
        }

        public static void UpdateStatusStandardToNUll(FileInformation PFile, SqlConnection conn)
        {

            try
            {
                string operation = PFile.FolderPath.Split('\\').Reverse().Skip(1).First();
                string componentid = PFile.FolderPath.Split('\\').Reverse().Skip(3).First();
                int operationno = int.Parse(operation.Split('_').First());
                string updateStatusQry = @"Update machineFileInfo SET standardfileName = NULL, StandardModifiedDate = NULL WHERE operationno = @operationno AND componentid = @componentid";
                using (SqlCommand cmd = new SqlCommand(updateStatusQry, conn))
                {
                    _ = cmd.Parameters.AddWithValue("@provenfileName", PFile.FileName);
                    _ = cmd.Parameters.AddWithValue("@operationno", operationno);
                    _ = cmd.Parameters.AddWithValue("@componentid", componentid);
                    _ = cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog("Error in UpdateStatusStandardToNUll: " + ex.Message);
            }
        }
    }
}