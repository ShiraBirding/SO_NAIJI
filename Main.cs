using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SO_NAIJI
{
    internal class Main
    {
        public string _CustCode;
        public string _CustSoNoName;
        public string _DeliName;
        public string _SOdate;
        public Boolean _boolHasHeader;
        public Boolean _boolWriteHeader;
        public string _OutputFolderPath;
        public string _InputFolderPath;
        public string _HeaderFilePath;
        public string _strCSVword;
        public string _strDelimiter;
        public Boolean _boolCopyPDF;
        public Boolean _boolMoveFolder;
        public string _txtOrderDate;
        public string _strPDFstate;
        public string _strPDFtag;
        public string _strPDForder;

        public void MainExecute()
        {
            string CustCode = _CustCode;
            string CustSoNoName = _CustSoNoName;    
            string DeliName = _DeliName;
            string SOdate = _SOdate;
            Boolean boolHasHeader = _boolHasHeader;
            Boolean boolWriteHeader = _boolWriteHeader;
            string OutputFolderPath = _OutputFolderPath;
            string InputFolderPath = _InputFolderPath;
            string HeaderFilePath = _HeaderFilePath;
            string strCSVword = _strCSVword;
            string strDelimiter = _strDelimiter;
            Boolean boolCopyPDF = _boolCopyPDF;
            Boolean boolMoveFolder = _boolMoveFolder;
            string txtOrderDate = _txtOrderDate;
            string strPDFstate = _strPDFstate;
            string strPDFtag = _strPDFtag;
            string strPDForder = _strPDForder;

            try
            {
                string[] CSVnames = Directory.GetFiles(InputFolderPath, "*" + strCSVword + "*.csv", System.IO.SearchOption.AllDirectories);

                DataTable dtCSV = new DataTable();
                dtCSV = SetColumns(HeaderFilePath, strDelimiter, true);

                foreach (string name in CSVnames)
                {
                    if (name != HeaderFilePath)
                    {
                        ReadCSV(dtCSV, boolHasHeader, name, strDelimiter, true);
                    }
                }

                //SQLクラスでデータベースに接続し、データ出力を実行
                DataTable dtCustSoNo = SQL.GetCustSoNo(CustCode);

                string lastCSV = "";
                Boolean finishFlg = false;
                List<string> finishCSV = new List<string>();

                foreach (DataRow dr in dtCSV.Rows)
                {
                    if (lastCSV != dr.Field<string>("CSVfileName"))
                    {
                        if (finishFlg)
                        {
                            finishCSV.Add(lastCSV);
                        }
                        finishFlg = true;
                        lastCSV = dr.Field<string>("CSVfileName");
                    }

                    if (dtCustSoNo.AsEnumerable().Any(row => dr.Field<string>(CustSoNoName) == row.Field<String>("CUST_SO_NO")))
                    {
                        dr.SetField(CustSoNoName, "受注済");
                    }
                    else
                    {
                        finishFlg = false;
                    }
                }

                if (!Directory.Exists(OutputFolderPath + @"\受注済") && finishCSV.Count != 0)
                {
                    Directory.CreateDirectory(OutputFolderPath + @"\受注済");
                }

                if (boolMoveFolder)
                {
                    foreach (string path in finishCSV)
                    {
                        if (Directory.Exists(OutputFolderPath + @"\受注済\" + Path.GetFileName(Path.GetDirectoryName(path))))
                        {
                            Directory.Delete(OutputFolderPath + @"\受注済\" + Path.GetFileName(Path.GetDirectoryName(path)));
                        }
                        Directory.Move(Path.GetDirectoryName(path), OutputFolderPath + @"\受注済\" + Path.GetFileName(Path.GetDirectoryName(path)));
                    }
                }
                else
                {
                    foreach (string path in finishCSV)
                    {
                        if (File.Exists(OutputFolderPath + @"\受注済\" + Path.GetFileName(path)))
                        {
                            File.Delete(OutputFolderPath + @"\受注済\" + Path.GetFileName(path));
                        }
                        File.Move(path, OutputFolderPath + @"\受注済\" + Path.GetFileName(path));
                    }
                }

                dtCSV.Columns.Remove("CSVfileName");

                DataRow[] drSO = dtCSV.Select(DeliName + "<= '" + SOdate + "' AND " + CustSoNoName + " <> '受注済'");
                DataRow[] drNAIJI = dtCSV.Select(DeliName + "> '" + SOdate + "' AND " + CustSoNoName + " <> '受注済'");

                string filePath;
                if (drSO.Length > 0)
                {
                    filePath = OutputFolderPath + @"\受注_" + CustCode + "_" + System.DateTime.Now.ToString("yyyyMMdd") + ".csv";
                    DataRowToCSV(drSO, dtCSV, filePath, boolWriteHeader);

                    if (boolCopyPDF)
                    {
                        CopyPDF(drSO, txtOrderDate, CustSoNoName, InputFolderPath, OutputFolderPath, strPDFstate, strPDFtag,strPDForder);
                        MessageBox.Show("PDFファイルをコピーしました");
                    }
                }
                else
                {
                    MessageBox.Show("該当する受注はありませんでした");
                }

                if(drNAIJI.Length > 0)
                {
                    filePath = OutputFolderPath + @"\内示_" + CustCode + "_" + System.DateTime.Now.ToString("yyyyMMdd") + ".csv";
                    DataRowToCSV(drNAIJI, dtCSV, filePath, boolWriteHeader);
                }
                else
                {
                    MessageBox.Show("該当する内示はありませんでした");
                }

                MessageBox.Show("受注ファイルと内示ファイルの作成が完了しました");
            }
            catch(System.Exception ex)
            {
                MessageBox.Show("エラーが発生しました 入力項目を確認してください" + "\r\n" + ex.ToString());
            }
        }

        public DataTable SetColumns(string fileName, string separator, bool quote)
        {
            DataTable dt = new DataTable();
            //CSVを便利に読み込んでくれるTextFieldParserを使います。
            TextFieldParser parser = new TextFieldParser(fileName, Encoding.GetEncoding("shift_jis"));
            //これは可変長のフィールドでフィールドの区切りのマーカーが使われている場合です。
            //フィールドが固定長の場合は
            //parser.TextFieldType = FieldType.FixedWidth;
            parser.TextFieldType = FieldType.Delimited;
            //区切り文字を設定します。
            parser.SetDelimiters(separator);
            //クォーテーションがあるかどうか。
            //但しダブルクォーテーションにしか対応していません。シングルクォーテーションは認識しません。
            parser.HasFieldsEnclosedInQuotes = quote;
            string[] data;
            if (!parser.EndOfData)
            {
                //CSVファイルから1行読み取ります。
                data = parser.ReadFields();
                //カラムの数を取得します。
                int cols = data.Length;
                for (int i = 0; i < cols; i++)
                {
                    dt.Columns.Add(new DataColumn(data[i]));
                }
                dt.Columns.Add(new DataColumn("CSVfileName"));
            }
            return dt;
        }

        public void ReadCSV(DataTable dt, bool hasHeader, string fileName, string separator, bool quote)
        {
            //CSVを便利に読み込んでくれるTextFieldParserを使います。
            TextFieldParser parser = new TextFieldParser(fileName, Encoding.GetEncoding("shift_jis"));
            //これは可変長のフィールドでフィールドの区切りのマーカーが使われている場合です。
            //フィールドが固定長の場合は
            //parser.TextFieldType = FieldType.FixedWidth;
            parser.TextFieldType = FieldType.Delimited;
            //区切り文字を設定します。
            parser.SetDelimiters(separator);
            //クォーテーションがあるかどうか。
            //但しダブルクォーテーションにしか対応していません。シングルクォーテーションは認識しません。
            parser.HasFieldsEnclosedInQuotes = quote;
            string[] data;

            //ヘッダーがある場合はparserを一個進めておく
            if (!parser.EndOfData && hasHeader)
            {
                data = parser.ReadFields();
                data = null;
            }

            //ここのループがCSVを読み込むメインの処理です。
            //内容は先ほどとほとんど一緒です。
            while (!parser.EndOfData)
            {
                data = parser.ReadFields();
                DataRow row = dt.NewRow();
                for (int i = 0; i < dt.Columns.Count - 1; i++)
                {
                    if (data[i].Contains(","))
                    {
                        row[i] = "\"" + data[i] + "\"";
                    }
                    else
                    {
                        row[i] = data[i];
                    }
                }
                row[dt.Columns.Count - 1] = fileName;
                dt.Rows.Add(row);
            }
            //ファイルを閉じる
            parser.Close();
        }

        public void DataRowToCSV(DataRow[] drInput, DataTable dtInput, string filePath, Boolean BoolWriteHeader)
        {
            System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.GetEncoding("shift_jis"));

            //ヘッダを書き込む
            if (BoolWriteHeader)
            {
                for (int i = 0; i < dtInput.Columns.Count; i++)
                {
                    //ヘッダの取得
                    string field = dtInput.Columns[i].Caption;
                    //フィールドを書き込む
                    writer.Write(field);
                    //カンマを書き込む
                    if (dtInput.Columns.Count - 1 > i)
                    {
                        writer.Write(',');
                    }
                }
                //改行する
                writer.Write("\r\n");
            }

            foreach (DataRow row in drInput)
            {
                for (int i = 0; i < dtInput.Columns.Count - 1; i++)
                {
                    writer.Write(row[i]);
                    writer.Write(",");
                }
                //最後の列はカンマではなく改行を行う
                writer.Write(row[dtInput.Columns.Count - 1]);
                writer.Write("\r\n");//VB.NETだとエスケープシーケンスが使えないのでvbCrLfを使う
            }
            writer.Close();//保存
        }

        public void CopyPDF(DataRow[] datarow, string txtOrderDate, string CustSoNoName, string InputFolderPath, string OutputFolderPath, string strPDFstate, string strPDFtag,string strPDForder)
        {
            HashSet<string> receiveDates = new HashSet<string>();
            HashSet<string> SoNoList = new HashSet<string>();

            foreach (DataRow dr in datarow)
            {
                receiveDates.Add(dr.Field<string>(txtOrderDate));
                SoNoList.Add(dr.Field<string>(CustSoNoName));
            }

            PDF pdf = new PDF();
            Document stateDoc = new Document();
            PdfCopy stateCopy = new PdfCopy(stateDoc, new FileStream(OutputFolderPath + @"\" + strPDFstate + "_" + System.DateTime.Now.ToString("yyyyMMdd") + ".pdf", FileMode.Create));

            Document tagDoc = new Document();
            PdfCopy tagCopy = new PdfCopy(tagDoc, new FileStream(OutputFolderPath + @"\" + strPDFtag + "_" + System.DateTime.Now.ToString("yyyyMMdd") + ".pdf", FileMode.Create));

            Document orderDoc = new Document();
            PdfCopy orderCopy = new PdfCopy(orderDoc, new FileStream(OutputFolderPath + @"\" + strPDForder + "_" + System.DateTime.Now.ToString("yyyyMMdd") + ".pdf", FileMode.Create));

            stateDoc.Open();
            stateCopy.Open();
            tagDoc.Open();
            tagCopy.Open();
            orderDoc.Open();
            orderCopy.Open();

            if (receiveDates.Count > 0)
            {
                foreach (string receiveDate in receiveDates)
                {
                    string[] statePaths = Directory.GetFiles(InputFolderPath, "*" + strPDFstate + "*" + receiveDate + "*.pdf", System.IO.SearchOption.AllDirectories);
                    string[] tagPaths = Directory.GetFiles(InputFolderPath, "*" + strPDFtag + "*" + receiveDate + "*.pdf", System.IO.SearchOption.AllDirectories);
                    string[] orderPaths = Directory.GetFiles(InputFolderPath, "*" + strPDForder + "*" + receiveDate + "*.pdf", System.IO.SearchOption.AllDirectories);

                    if (strPDFstate != null && strPDFstate != "")
                    {
                        if (statePaths.Length > 0)
                        {
                            pdf.InsertTargetPages(stateCopy, statePaths[0], SoNoList.ToList());
                        }
                        else
                        {
                            MessageBox.Show(receiveDate + "の" + strPDFstate + "のPDFファイルがありませんでした");
                        }
                    }
                    if (strPDFtag != null && strPDFtag != "")
                    {
                        if (tagPaths.Length > 0)
                        {
                            pdf.InsertTargetPages(tagCopy, tagPaths[0], SoNoList.ToList());
                        }
                        else
                        {
                            MessageBox.Show(receiveDate + "の" + strPDFtag + "のPDFファイルがありませんでした");
                        }
                    }
                    if (strPDForder != null && strPDForder != "")
                    {
                        if (orderPaths.Length > 0)
                        {
                            pdf.InsertTargetPages(orderCopy, orderPaths[0], SoNoList.ToList());
                        }
                        else
                        {
                            MessageBox.Show(receiveDate + "の" + strPDForder + "のPDFファイルがありませんでした");
                        }
                    }
                }
            }
            stateDoc.Close();
            stateCopy.Close();
            tagDoc.Close();
            tagCopy.Close();
            orderDoc.Close();
            orderCopy.Close();
        }
    }
}
